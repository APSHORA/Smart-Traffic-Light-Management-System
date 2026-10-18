using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TrafficLightManagement.Models;

namespace TrafficLightManagement.Controllers
{
    public class TrafficLightController : Controller
    {
        public static List<TrafficLight> lights = new List<TrafficLight>();
        public static List<string> auditLog = new List<string>();

        // ✅ Adjacent light mapping
        public static Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>()
        {
            {1, new List<int>{2}},
            {2, new List<int>{1,3}},
            {3, new List<int>{2,4}},
            {4, new List<int>{3}}
        };

        public IActionResult Dashboard()
        {
            // ✅ Default data
            if (!lights.Any())
            {
                lights.Add(new TrafficLight { LightID = 1, Location = "Main Gate", CurrentState = "Red", TrafficDensity = 60, LastMaintenanceDate = DateTime.Now.AddDays(-10), HasPedestrianButton = true });
                lights.Add(new TrafficLight { LightID = 2, Location = "City Center", CurrentState = "Green", TrafficDensity = 85, LastMaintenanceDate = DateTime.Now.AddDays(-40), HasPedestrianButton = false });
                lights.Add(new TrafficLight { LightID = 3, Location = "Airport Road", CurrentState = "Red", TrafficDensity = 75, LastMaintenanceDate = DateTime.Now.AddDays(-5), HasPedestrianButton = true });
                lights.Add(new TrafficLight { LightID = 4, Location = "Uttara Sector 7", CurrentState = "Yellow", TrafficDensity = 40, LastMaintenanceDate = DateTime.Now.AddDays(-15), HasPedestrianButton = false });
            }

            // ✅ Notifications List
            List<string> notifications = new List<string>();

            foreach (var light in lights)
            {
                if (light.IsMaintenanceDue())
                    notifications.Add($"🔧 Maintenance Due Alert! {light.Location} needs maintenance.");

                if (light.TrafficDensity >= 80)
                    notifications.Add($"🚨 High Congestion at {light.Location} ({light.TrafficDensity}%)");
            }

            ViewBag.Notification = notifications;

            // ✅ Stats
            ViewBag.Total = lights.Count;
            ViewBag.GreenCount = lights.Count(l => l.CurrentState == "Green");
            ViewBag.RedCount = lights.Count(l => l.CurrentState == "Red");
            ViewBag.YellowCount = lights.Count(l => l.CurrentState == "Yellow");
            ViewBag.CongestedCount = lights.Count(l => l.TrafficDensity >= 80);

            return View(lights);
        }

        // ✅ Add Traffic Light Page
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // ✅ Add Traffic Light Submit
        [HttpPost]
        public IActionResult Add(TrafficLight light)
        {
            light.LightID = lights.Count + 1;
            light.LastStateChange = DateTime.Now;

            if (ModelState.IsValid)
            {
                lights.Add(light);
                TempData["Message"] = $"✅ Added {light.Location}";
                auditLog.Add($"➕ Added {light.Location} at {DateTime.Now}");
                return RedirectToAction("Dashboard");
            }
            return View(light);
        }

        // ✅ Manual Override + safety constraints
        public IActionResult ChangeState(int id, string newState)
        {
            var light = lights.FirstOrDefault(l => l.LightID == id);
            if (light == null) return RedirectToAction("Dashboard");

            // ✅ 5 sec rule
            if ((DateTime.Now - light.LastStateChange).TotalSeconds < 5)
            {
                TempData["Error"] = "⏱ Signal cannot change within 5 seconds!";
                return RedirectToAction("Dashboard");
            }

            // ✅ No adjacent green conflict
            if (newState == "Green" && adjacency.ContainsKey(id))
            {
                foreach (var adj in adjacency[id])
                {
                    var adjLight = lights.FirstOrDefault(x => x.LightID == adj);
                    if (adjLight != null)
                        adjLight.CurrentState = "Red";
                }
            }

            light.CurrentState = newState;
            light.LastStateChange = DateTime.Now;

            auditLog.Add($"🔄 {light.Location} changed to {newState} at {DateTime.Now}");

            return RedirectToAction("Dashboard");
        }

        // ✅ Pedestrian Request
        public IActionResult PedestrianRequest(int id)
        {
            var light = lights.FirstOrDefault(l => l.LightID == id);
            if (light != null && light.HasPedestrianButton)
            {
                light.CurrentState = "Red";
                TempData["PedestrianAlert"] = $"🚶 Pedestrian crossing activated at {light.Location}";
                auditLog.Add($"🚶 Pedestrian request at {light.Location} at {DateTime.Now}");
            }
            return RedirectToAction("Dashboard");
        }

        // ✅ Maintenance Done
        public IActionResult DoMaintenance(int id)
        {
            var light = lights.FirstOrDefault(l => l.LightID == id);
            if (light != null)
            {
                light.LastMaintenanceDate = DateTime.Now;
                TempData["MaintenanceSuccess"] = $"✅ Maintenance Completed for {light.Location}";
                auditLog.Add($"🔧 Maintenance done at {light.Location} on {DateTime.Now}");
            }
            return RedirectToAction("Dashboard");
        }

        // ✅ Delete
        public IActionResult Delete(int id)
        {
            var light = lights.FirstOrDefault(l => l.LightID == id);
            if (light != null)
            {
                lights.Remove(light);
                TempData["Message"] = $"❌ Removed {light.Location}";
                auditLog.Add($"❌ Removed {light.Location} at {DateTime.Now}");
            }
            return RedirectToAction("Dashboard");
        }

        // ✅ Road Status Checker
        public IActionResult CheckRoadStatus(string location)
        {
            var road = lights.FirstOrDefault(l => l.Location == location);
            if (road == null)
                TempData["RoadStatus"] = "❌ Road not found!";
            else if (road.TrafficDensity >= 80)
                TempData["RoadStatus"] = $"🚨 Heavy Traffic on {road.Location}. Avoid this road!";
            else
                TempData["RoadStatus"] = $"✅ {road.Location} is clear. You can go!";

            return RedirectToAction("Dashboard");
        }
    }
}
