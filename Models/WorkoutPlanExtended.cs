using GymManager.Models;

namespace GymManagerApi.Models
{
    public class WorkoutPlanExtended : WorkoutPlan
    {
        public List<SelectedWorkouts> SelectedWorkouts { get; set; }
    }
}
