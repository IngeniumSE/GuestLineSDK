// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK;

/// <summary>
/// Provides factory methods for creating a HTTP client.
/// </summary>
public interface IGuestLineHttpClientFactory
{
	/// <summary>
	/// Creates a HTTP client.
	/// </summary>
	/// <param name="name">The HTTP client name.</param>
	/// <returns>The HTTP client.</returns>
	HttpClient CreateHttpClient(string? name = null);
}

public class GuestLineHttpClientFactory(IHttpClientFactory clientFactory) : IGuestLineHttpClientFactory
{
	public HttpClient CreateHttpClient(string? name = null)
	{
		if (name is null)
		{
			return clientFactory.CreateClient();
		}
		return clientFactory.CreateClient(name);
	}
}
