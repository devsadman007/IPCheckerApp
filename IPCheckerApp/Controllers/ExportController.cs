using Microsoft.AspNetCore.Mvc;
using SelectPdf;
using System.Text;
using System.Text.Json;
using IPCheckerApp.Models;

namespace IPCheckerApp.Controllers
{
    public class ExportController : Controller
    {
        public IActionResult ExportCsv()
        {
            if (TempData["IpTable"] is not string json)
                return RedirectToAction("Index", "IpChecker");

            var data = JsonSerializer.Deserialize<List<IpStatus>>(json);
            var sb = new StringBuilder("SL,IP Address,Status\r\n");
            foreach (var item in data)
                sb.AppendLine($"{item.SL},{item.IpAddress},{item.Status}");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "IpStatus.csv");
        }

        public IActionResult ExportPdf()
        {
            if (TempData["IpTable"] is not string json)
                return RedirectToAction("Index", "IpChecker");

            var data = JsonSerializer.Deserialize<List<IpStatus>>(json);
            var timestamp = TempData["Timestamp"]?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            int upCount = data.Count(x => x.Status == "UP");
            int downCount = data.Count(x => x.Status == "DOWN");

            var html = new StringBuilder();
            html.AppendLine("<html><head><meta charset='UTF-8'><style>")
                .AppendLine("table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }")
                .AppendLine("th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }")
                .AppendLine("th { background: #eee; }")
                .AppendLine("p { font-size: 14px; }")
                .AppendLine("</style></head><body>")
                .AppendLine($"<h2>IP Status Report</h2><p>Generated on: {timestamp}</p>")
                .AppendLine("<table><thead><tr><th>SL</th><th>IP Address</th><th>Status</th></tr></thead><tbody>");

            foreach (var item in data)
                html.AppendLine($"<tr><td>{item.SL}</td><td>{item.IpAddress}</td><td>{item.Status}</td></tr>");

            html.AppendLine("</tbody></table>")
                .AppendLine($"<p><strong>Total UP:</strong> <span style='color:green'>{upCount}</span></p>")
                .AppendLine($"<p><strong>Total DOWN:</strong> <span style='color:red'>{downCount}</span></p>")
                .AppendLine("</body></html>");

            var converter = new SelectPdf.HtmlToPdf();
            var doc = converter.ConvertHtmlString(html.ToString());
            var pdf = doc.Save();
            doc.Close();

            return File(pdf, "application/pdf", "IpStatus.pdf");
        }
    }
}
