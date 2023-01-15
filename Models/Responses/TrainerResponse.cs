using GymManager.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace GymManagerApi.Models.Responses
{
    public class TrainerResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; }
        List<WorkoutPlanResponse> WorkoutPlans { get; set; } = new List<WorkoutPlanResponse>();
    }
}
