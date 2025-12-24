// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK.Primitives;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DateOnlyJsonConverter() : JsonConverter<DateTime?>
{
	readonly string _format = "yyyy-MM-dd";

	public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? value = reader.GetString();
		if (DateTime.TryParseExact(value, _format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime result))
		{
			return result;
		}

		return null;
	}

	public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNullValue();
		}
		else
		{
			writer.WriteStringValue(value.Value.ToString(_format));
		}
	}
}
