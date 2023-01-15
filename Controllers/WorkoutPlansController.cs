using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManager.Data;
using GymManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using GymManagerApi.Models;
using GymManagerApi.Models.Responses;
using GymManagerApi.Models.Request;

namespace GymManagerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public class WorkoutPlansController : ControllerBase
    {
        private readonly GymManagerContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public WorkoutPlansController(GymManagerContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/WorkoutPlans
        [HttpGet]
        public async Task<ActionResult<List<WorkoutPlanResponse>>> GetWorkoutPlan()
        {
            return await _context.WorkoutPlan.Include(b=>b.Trainer).Include(b => b.Workouts).ThenInclude(b => b.Workout).Select(x=>new WorkoutPlanResponse
            {
                Id = x.Id,
                Name = x.Name,
                Trainer = new TrainerResponse { Id = x.Trainer.Id, Name = x.Trainer.Name, Email = x.Trainer.Email },
                TrainerId = x.TrainerId,
                Workouts = x.Workouts.Select(x=>new WorkoutResponse
                {
                    Id = x.WorkoutId,
                    Name = x.Workout.Name,
                    AltName = x.Day + " - " + x.Workout.Name,
                    Description = x.Workout.Description,
                    Day = x.Day
                }).ToList(),

            }).ToListAsync();
        }

        // GET: api/WorkoutPlans/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkoutPlan>> GetWorkoutPlan(int id)
        {
            var workoutPlan = await _context.WorkoutPlan.FindAsync(id);

            if (workoutPlan == null)
            {
                return NotFound();
            }

            return workoutPlan;
        }

        // PUT: api/WorkoutPlans/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "ApiKey", Roles = "Trainer")]
        public async Task<IActionResult> PutWorkoutPlan(int id, WorkoutPlanRequest workoutPlan)
        {
            if (id != workoutPlan.Id)
            {
                return BadRequest();
            }


            //_context.Attach(WorkoutPlan).State = EntityState.Modified;
            var workoutplan = await _context.WorkoutPlan.Include(b => b.Trainer).Include(b => b.Workouts).ThenInclude(b => b.Workout).FirstOrDefaultAsync(m => m.Id == id);
            if (workoutplan == null)
                return NotFound();

            //ignoram proprietate Trainer la validare
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<WorkoutPlan>(
                    workoutplan,
                    "WorkoutPlan",
                    i => i.Name))
                {
                    UpdateWorkoutPlan(_context, workoutPlan.SelectedWorkouts, workoutplan);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
            }

            return BadRequest(ModelState);


        }

        // POST: api/WorkoutPlans
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(AuthenticationSchemes = "ApiKey", Roles = "Trainer")]
        public async Task<ActionResult<WorkoutPlan>> PostWorkoutPlan(WorkoutPlanRequest workoutPlan)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }
                var trainer = _context.Trainer.FirstOrDefault(m => m.AspNetUserId == user.Id);
                if (trainer == null)
                {
                    return NotFound();
                }
                var newWorkoutPlan = new WorkoutPlan();
                newWorkoutPlan.Name = workoutPlan.Name;
                newWorkoutPlan.TrainerId = trainer.Id;
                if (workoutPlan.SelectedWorkouts != null)
                {
                    newWorkoutPlan.Workouts = new List<WorkoutWorkoutPlans>();
                    foreach (var selectedWorkout in workoutPlan.SelectedWorkouts.Where(x => x.Id != 0))
                    {
                        newWorkoutPlan.Workouts.Add(new WorkoutWorkoutPlans
                        {
                            WorkoutId = selectedWorkout.Id,
                            Day = selectedWorkout.Day
                        });
                    }
                }

                _context.WorkoutPlan.Add(newWorkoutPlan);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetWorkoutPlan", new { id = workoutPlan.Id }, workoutPlan);
            }

            return BadRequest(ModelState);
        }

        // DELETE: api/WorkoutPlans/5
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "ApiKey", Roles = "Trainer")]
        public async Task<IActionResult> DeleteWorkoutPlan(int id)
        {
            var workoutPlan = await _context.WorkoutPlan.FindAsync(id);
            if (workoutPlan == null)
            {
                return NotFound();
            }

            _context.WorkoutPlan.Remove(workoutPlan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WorkoutPlanExists(int id)
        {
            return _context.WorkoutPlan.Any(e => e.Id == id);
        }

        private void UpdateWorkoutPlan(GymManagerContext context, List<SelectedWorkouts> selectedWorkouts, WorkoutPlan planToUpdate)
        {
            if (selectedWorkouts == null)
            {
                planToUpdate.Workouts = new List<WorkoutWorkoutPlans>();
                return;
            }
            var workouts = new HashSet<int>(planToUpdate.Workouts.Select(c => c.WorkoutId));

            var newWorkouts = new List<WorkoutWorkoutPlans>();
            foreach (var workout in selectedWorkouts.Where(x => x.Id != 0))
            {
                var thisWorkout = planToUpdate.Workouts.SingleOrDefault(x => x.WorkoutId == workout.Id);

                if (thisWorkout == null)
                {
                    thisWorkout = new WorkoutWorkoutPlans
                    {
                        WorkoutPlanId = planToUpdate.Id,
                        WorkoutId = workout.Id,
                        Day = workout.Day
                    };
                    planToUpdate.Workouts.Add(thisWorkout);
                }
                else
                {
                    thisWorkout.Day = workout.Day;
                }

                newWorkouts.Add(thisWorkout);
            }

            var workoutsToRemove = new List<WorkoutWorkoutPlans>();
            foreach (var workout in planToUpdate.Workouts.Except(newWorkouts))
            {
                workoutsToRemove.Add(workout);

            }
            foreach (var workout in workoutsToRemove)
            {
                planToUpdate.Workouts.Remove(workout);
                context.Remove(workout);
            }
        }
    }
}
