using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class MetricsController : ControllerBase
{
    [HttpGet("/metrics")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public ActionResult<ProcessMetrics> GetMetrics()
    {
        using var process = Process.GetCurrentProcess();
        return new ProcessMetrics(
            process.TotalProcessorTime.TotalSeconds,
            process.WorkingSet64,
            process.Id,
            (DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch).TotalSeconds);
    }
}
