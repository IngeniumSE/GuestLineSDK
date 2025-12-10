// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GuestLineSDK;

static class JsonUtility
{
	public static JsonSerializerOptions CreateSerializerOptions()
	{
		JsonSerializerOptions options = new()
		{
			WriteIndented = false,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			Converters =
			{
				new JsonStringEnumConverter()
			}
		};

		return options;
	}

	public static JsonSerializerOptions CreateDeserializerOptions()
	{
		JsonSerializerOptions options = new()
		{
			WriteIndented = false,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			NumberHandling = JsonNumberHandling.AllowReadingFromString,
			Converters =
			{
				new JsonStringEnumConverter()
			}
		};

		return options;
	}
}
