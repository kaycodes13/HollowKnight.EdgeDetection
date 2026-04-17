using Newtonsoft.Json;
using Satchel.JsonConverters;

namespace EdgeDetection.Structs;

/// <summary>
/// Deserialization struct which describes the configurable parts of an <see cref="Components.EdgeDetectionPass"/>.
/// </summary>
[Serializable]
public record struct PassSettings {
	[JsonConverter(typeof(ColorConverter))]
	public Color Colour { get; init; }
	public byte Width { get; init; }
	public bool HalfRes { get; init; }
}
