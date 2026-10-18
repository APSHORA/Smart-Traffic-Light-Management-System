using System;
using System.ComponentModel.DataAnnotations;

namespace TrafficLightManagement.Models
{
    public class TrafficLight
    {
        [Key]
        public int LightID { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public string CurrentState { get; set; } = "Red";

        [Range(0, 100)]
        public int TrafficDensity { get; set; }

        [Required]
        public DateTime LastMaintenanceDate { get; set; }

        public bool HasPedestrianButton { get; set; }

        public DateTime LastStateChange { get; set; } = DateTime.MinValue;

        // ✅ Maintenance check: 30 days rule
        public bool IsMaintenanceDue()
        {
            return (DateTime.Now - LastMaintenanceDate).Days > 30;
        }
    }
}
