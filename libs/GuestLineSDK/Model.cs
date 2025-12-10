// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System.Text.Json;

namespace GuestLineSDK;

/// <summary>
/// Provides a base implementation of a model that is convertible to JSON.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
public abstract class Model<T>
	where T : Model<T>
{
	/// <summary>
	/// Converts the given JSON string to an instance of <see cref="T"/>.
	/// </summary>
	/// <param name="json">The JSON string.</param>
	/// <returns>The model instance.</returns>
	public static T? FromJsonString(string json)
	{
		Ensure.IsNotNullOrEmpty(json, nameof(json));

		var settings = JsonUtility.CreateDeserializerOptions();

		return JsonSerializer.Deserialize<T>(json, settings);
	}

	/// <summary>
	/// Converts the current instance to a JSON string.
	/// </summary>
	/// <returns>The JSON string.</returns>
	public string ToJsonString()
	{
		var settings = JsonUtility.CreateSerializerOptions();

		return JsonSerializer.Serialize((T)this, settings);
	}
}
