using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpAutomation.API;

public static class ApiJsonOptions
{
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
