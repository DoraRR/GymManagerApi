using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManager.Data;
using GymManager.Models;

namespace GymManagerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GymUsersController : ControllerBase
    {
        private readonly GymManagerContext _context;

        public GymUsersController(GymManagerContext context)
        {
            _context = context;
        }

        // GET: api/GymUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GymUser>>> GetGymUser()
        {
            return await _context.GymUser.ToListAsync();
        }

        // GET: api/GymUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GymUser>> GetGymUser(int id)
        {
            var gymUser = await _context.GymUser.FindAsync(id);

            if (gymUser == null)
            {
                return NotFound();
            }

            return gymUser;
        }

        [HttpPut("SetWorkoutPlan/{id}")]
        public async Task<IActionResult> SetWorkoutPlan(int id, int[] selectedWorkoutPlans)
        {
            var gymuser = await _context.GymUser.Include(s => s.WorkoutPlans).FirstOrDefaultAsync(m => m.Id == id);
            if (gymuser == null)
                return NotFound();

            UpdateWorkoutPlan(_context, selectedWorkoutPlans, gymuser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/GymUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGymUser(int id, GymUser gymUser)
        {
            if (id != gymUser.Id)
            {
                return BadRequest();
            }

            _context.Entry(gymUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GymUserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/GymUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<GymUser>> PostGymUser(GymUser gymUser)
        {
            _context.GymUser.Add(gymUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGymUser", new { id = gymUser.Id }, gymUser);
        }

        // DELETE: api/GymUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGymUser(int id)
        {
            var gymUser = await _context.GymUser.FindAsync(id);
            if (gymUser == null)
            {
                return NotFound();
            }

            _context.GymUser.Remove(gymUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GymUserExists(int id)
        {
            return _context.GymUser.Any(e => e.Id == id);
        }

        private void UpdateWorkoutPlan(GymManagerContext context, int[] selectedWorkoutPlans, GymUser gymUserToUpdate)
        {
            if (selectedWorkoutPlans == null)
            {
                gymUserToUpdate.WorkoutPlans = new List<WorkoutPlan>();
                return;
            }
            var workoutPlans = new HashSet<int>(gymUserToUpdate.WorkoutPlans.Select(c => c.Id));

            var newWorkoutPlans = new List<WorkoutPlan>();
            foreach (var workoutPlan in selectedWorkoutPlans)
            {
                var thisWorkoutPlan = gymUserToUpdate.WorkoutPlans.SingleOrDefault(x => x.Id == workoutPlan);

                if (thisWorkoutPlan == null)
                {
                    thisWorkoutPlan = context.WorkoutPlan.FirstOrDefault(x => x.Id == workoutPlan);
                    gymUserToUpdate.WorkoutPlans.Add(thisWorkoutPlan);
                }

                newWorkoutPlans.Add(thisWorkoutPlan);
            }

            var workoutPlansToRemove = new List<WorkoutPlan>();
            foreach (var workout in gymUserToUpdate.WorkoutPlans.Except(newWorkoutPlans))
            {
                workoutPlansToRemove.Add(workout);

            }
            foreach (var workout in workoutPlansToRemove)
            {
                gymUserToUpdate.WorkoutPlans.Remove(workout);
            }
        }
    }
}
