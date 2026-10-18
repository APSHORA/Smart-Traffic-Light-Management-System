using System.ComponentModel.DataAnnotations;

namespace TrafficLightSystem.Models
{
    public class EmergencyVehicleRequest
    {
        [Key]
        public int RequestID { get; set; }

        [Required]
        public string VehicleType { get; set; } // Ambulance, Fire Truck, Police

        [Required]
        public string ApproachingFrom { get; set; }

        [Range(1, 5)]
        public int PriorityLevel { get; set; }
    }
}
