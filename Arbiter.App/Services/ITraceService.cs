
using System.Collections.Generic;
using System.Threading.Tasks;
using Arbiter.App.Models;

namespace Arbiter.App.Services;

public interface ITraceService
{
    Task<TraceFile> LoadTraceFileAsync(string filePath);
    
    Task SaveTraceFileAsync(IReadOnlyCollection<TracePacket> packets, string filePath)
        => SaveTraceFileAsync(new TraceFile { Packets = packets }, filePath);
    Task SaveTraceFileAsync(TraceFile trace, string filePath);
}