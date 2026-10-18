using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace TrafficLightManagement.Controllers
{
    public class EmergencyController : Controller
    {
        public IActionResult ActivateEmergency(int id)
        {
            var light = TrafficLightController.lights.FirstOrDefault(x => x.LightID == id);

            if (light != null)
            {
                // ✅ All red first
                foreach (var l in TrafficLightController.lights)
                    l.CurrentState = "Red";

                // ✅ Emergency route becomes green
                light.CurrentState = "Green";
                light.LastStateChange = DateTime.Now;

                TempData["EmergencyAlert"] =
                    $"🚑 Emergency Vehicle Detected at {light.Location}! Signal turned GREEN immediately.";

                TrafficLightController.auditLog.Add($"🚑 Emergency Activated at {light.Location} on {DateTime.Now}");
            }

            return RedirectToAction("Dashboard", "TrafficLight");
        }
    }
}
