using Microsoft.AspNetCore.Mvc;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Text;
using System.Text.Json;
using IPCheckerApp.Models;

public class ExportController : Controller
{
    private readonly IConverter _converter;

    public ExportController(IConverter converter)
    {
        _converter = converter;
    }

    public IActionResult ExportPdf()
    {
        if (TempData["IpTable"] is not string json)
            return RedirectToAction("Index", "IpChecker");

        var data = JsonSerializer.Deserialize<List<IpStatus>>(json);
        int up = data.Count(x => x.Status == "UP");
        int down = data.Count(x => x.Status == "DOWN");

        var html = new StringBuilder();
        html.AppendLine("<html><head><meta charset='UTF-8'><style>")
            .AppendLine("body { font-family: Arial; padding: 20px; font-size: 12px; }")
            .AppendLine("table { width: 100%; border-collapse: collapse; }")
            .AppendLine("th, td { border: 1px solid #ccc; padding: 8px; }")
            .AppendLine("th { background-color: #f2f2f2; }")
            .AppendLine(".badge { padding: 3px 6px; border-radius: 4px; color: #fff; }")
            .AppendLine(".up { background-color: green; }")
            .AppendLine(".down { background-color: red; }")
            .AppendLine("</style></head><body>")
            .AppendLine("<h2 style='text-align:center; font-size:32px; margin-bottom:20px;'>IP Status Report of All Branch Office</h2>")
            //.AppendLine("<h2>IP Status Report</h2>")
            .AppendLine($"<p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>")
            .AppendLine("<table><thead><tr><th>SL</th><th>IP Address</th><th>Status</th></tr></thead><tbody>");

        foreach (var item in data)
        {
            var statusClass = item.Status == "UP" ? "badge up" : "badge down";
            html.AppendLine($"<tr><td>{item.SL}</td><td>{item.IpAddress}</td><td><span class='{statusClass}'>{item.Status}</span></td></tr>");
        }

        html.AppendLine("</tbody></table>")
            .AppendLine($"<p><strong>Total UP:</strong> <span class='badge up'>{up}</span></p>")
            .AppendLine($"<p><strong>Total DOWN:</strong> <span class='badge down'>{down}</span></p>")
            .AppendLine("</body></html>");

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = new GlobalSettings
            {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait,
                DocumentTitle = "IP Status Report",
                Margins = new MarginSettings { Top = 20, Bottom = 20, Left = 20, Right = 20 }
            },
            Objects = {
                new ObjectSettings
                {
                    HtmlContent = html.ToString()
                }
            }
        };

        var pdf = _converter.Convert(doc);
        var fileName = $"{DateTime.Now:yyyy-MM-dd}-IpStatus.pdf";
        return File(pdf, "application/pdf", fileName);
        //return File(pdf, "application/pdf", "IpStatus_DinkToPdf.pdf");
    }
}
