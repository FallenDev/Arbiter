using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Arbiter.App.Models.Tracing;

public class TraceFile
{
    [JsonPropertyName("version")] public string Version { get; set; } = "1.0";
    [JsonPropertyName("createdDate")] public DateTime Date { get; set; } = DateTime.UtcNow;
    [JsonPropertyName("packets")] public IReadOnlyCollection<TracePacket> Packets { get; set; } = [];
}