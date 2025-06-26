using IPCheckerApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace IPCheckerApp.Controllers
{
    public class IpCheckerController : Controller
    {

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult CheckIps(string ipList)
        {
            var ips = ipList.Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(ip => ip.Trim()).Take(80).ToList();

            List<IpStatus> results = new();
            int upCount = 0, downCount = 0;

            for (int i = 0; i < ips.Count; i++)
            {
                var ip = ips[i];
                bool isUp = PingIp(ip, 3);
                string status = isUp ? "UP" : "DOWN";
                results.Add(new IpStatus { SL = i + 1, IpAddress = ip, Status = status });

                if (isUp) upCount++; else downCount++;
            }

            ViewBag.UpCount = upCount;
            ViewBag.DownCount = downCount;
            ViewBag.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            TempData["IpTable"] = JsonSerializer.Serialize(results);
            TempData["Timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");



            return View("Result", results);
        }

        private bool PingIp(string ip, int attempts)
        {
            int success = 0;
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    Ping ping = new();
                    var reply = ping.Send(ip, 1000);
                    if (reply.Status == IPStatus.Success)
                        success++;
                }
                catch { continue; }
            }
            return success >= 1;
        }
    }
}
