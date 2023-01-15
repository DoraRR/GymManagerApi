using GymManager.Models;

namespace GymManagerApi.Models.Request
{
    public class WorkoutPlanRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SelectedWorkouts> SelectedWorkouts { get; set; }
    }
}
