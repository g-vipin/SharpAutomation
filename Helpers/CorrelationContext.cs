namespace SharpAutomation.Helpers
{
    public class CorrelationContext
    {
        public string CorrelationId { get; }

        public CorrelationContext()
        {
            CorrelationId = Guid.NewGuid().ToString();
        }
    }

    public static class CorrelationContextAccessor
    {
        private static readonly AsyncLocal<CorrelationContext> _current = new();

        public static CorrelationContext Current
        {
            get
            {
                if (_current.Value == null)
                    _current.Value = new CorrelationContext();

                return _current.Value;
            }
            set => _current.Value = value;
        }
    }
}
