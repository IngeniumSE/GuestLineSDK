// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK.Primitives;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ObpDictionaryJsonConverter : JsonConverter<Dictionary<int, decimal?>>
{
	public override Dictionary<int, decimal?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return null;
		}
		var dictionary = new Dictionary<int, decimal?>();
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
			{
				return dictionary;
			}
			if (reader.TokenType == JsonTokenType.PropertyName)
			{
				string propertyName = reader.GetString() ?? string.Empty;
				if (propertyName.StartsWith("person", StringComparison.OrdinalIgnoreCase))
				{
					if (int.TryParse(propertyName.Substring(6), out int key))
					{
						reader.Read();
						if (reader.TokenType == JsonTokenType.String)
						{
							string? valueString = reader.GetString();
							if (decimal.TryParse(valueString, out decimal value))
							{
								dictionary[key] = Math.Floor(value) / 100;
							}
							else
							{
								throw new JsonException($"Invalid value for key '{propertyName}' in ObpDictionaryJsonConverter.");
							}
						}
						else if (reader.TokenType == JsonTokenType.Number)
						{
							if (reader.TryGetDecimal(out var value))
							{
								dictionary[key] = Math.Floor(value) / 100;
							}
							else
							{
								throw new JsonException($"Invalid value for key '{propertyName}' in ObpDictionaryJsonConverter.");
							}
						}
						else if (reader.TokenType == JsonTokenType.Null)
						{
							dictionary[key] = null;
						}
						else
						{
							throw new JsonException($"Invalid value for key '{propertyName}' in ObpDictionaryJsonConverter.");
						}
					}
				}
			}
		}
		throw new JsonException("Invalid JSON format for ObpDictionaryJsonConverter.");
	}

	public override void Write(Utf8JsonWriter writer, Dictionary<int, decimal?> value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartObject();
		foreach (var kvp in value)
		{
			if (kvp.Value.HasValue)
			{
				writer.WriteNumber($"person{kvp.Key}", kvp.Value.Value);
			}
			else
			{
				writer.WriteNull($"person{kvp.Key}");
			}
		}
		writer.WriteEndObject();
	}
}
