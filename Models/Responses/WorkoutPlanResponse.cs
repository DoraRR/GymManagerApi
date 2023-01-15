namespace GymManagerApi.Models.Responses
{
    public class WorkoutPlanResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TrainerId { get; set; }
        public TrainerResponse Trainer { get; set; }
        public List<WorkoutResponse> Workouts { get; set; } = new List<WorkoutResponse>();
    }
}
