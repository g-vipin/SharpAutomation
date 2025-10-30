using TechTalk.SpecFlow.Tracing;

namespace SharpAutomation.Helpers
{
    public class CorrelatedTracer : ITraceListener
    {
        public void WriteTestOutput(string message)
        {
            var correlationId = CorrelationContextAccessor.Current?.CorrelationId ?? "N/A";
            Console.WriteLine($"[{correlationId}] {message}");
        }

        public void WriteToolOutput(string message)
        {
            var correlationId = CorrelationContextAccessor.Current?.CorrelationId ?? "N/A";
            Console.WriteLine($"[{correlationId}] TOOL: {message}");
        }
    }
}
