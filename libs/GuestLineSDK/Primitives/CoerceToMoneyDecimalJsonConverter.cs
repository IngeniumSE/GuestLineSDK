// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK.Primitives;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CoerceToMoneyDecimalJsonConverter : JsonConverter<decimal?>
{
	public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// Values may come as strings or numbers, so handle both cases. The value represents a whole number of major currency unit, so need to divide by 100. Ignore the decimal part if present.
		if (reader.TokenType == JsonTokenType.String)
		{
			string? valueString = reader.GetString();
			if (decimal.TryParse(valueString, out decimal value))
			{
				return Math.Floor(value) / 100;
			}
		}
		else if (reader.TokenType == JsonTokenType.Number)
		{
			if (reader.TryGetDecimal(out var value))
			{
				return Math.Floor(value) / 100;
			}
		}
		else if (reader.TokenType == JsonTokenType.Null)
		{
		}

		return null;
	}

	public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
