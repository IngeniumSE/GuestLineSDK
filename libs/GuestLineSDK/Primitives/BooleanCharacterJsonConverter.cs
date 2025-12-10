// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK.Primitives;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts from the simple boolean values to 'Y'/'N' character values in JSON, and vice versa.
/// </summary>
public class BooleanCharacterJsonConverter : JsonConverter<bool>
{
	/// <inheritdoc/>
	public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? value = reader.GetString();
		return value is not null && value.Equals("Y", StringComparison.OrdinalIgnoreCase);
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value ? "Y" : "N");
	}

}
