// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using GuestLineSDK.Api;

namespace GuestLineSDK;

public interface IGuestLineApiClientFactory
{
	IGuestLineApiClient CreateApiClient(GuestLineSettings settings, string? name = null);
}

/// <summary>
/// Provides factory services for creating Trybe client instances.
/// </summary>
public class GuestLineApiClientFactory : IGuestLineApiClientFactory
{
	readonly IGuestLineHttpClientFactory _httpClientFactory;

	public GuestLineApiClientFactory(IGuestLineHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = Ensure.IsNotNull(httpClientFactory, nameof(httpClientFactory));
	}

	public IGuestLineApiClient CreateApiClient(GuestLineSettings settings, string? name = null)
		=> new GuestLineApiClient(_httpClientFactory.CreateHttpClient(name), settings);
}
