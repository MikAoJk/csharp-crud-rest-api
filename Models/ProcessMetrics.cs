using System.Text.Json.Serialization;

public record ProcessMetrics(
    [property: JsonPropertyName("cpu_seconds_total")] double CpuSecondsTotal,
    [property: JsonPropertyName("memory_rss_bytes")] long MemoryRssBytes,
    [property: JsonPropertyName("process_id")] int ProcessId,
    [property: JsonPropertyName("sampled_at_unix_seconds")] double SampledAtUnixSeconds);
