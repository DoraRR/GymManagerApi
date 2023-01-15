namespace GymManagerApi.Models.Responses
{
    public class WorkoutResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AltName { get; set; }
        public string Description { get; set; }
        public int Day { get; set; }
    }
}
