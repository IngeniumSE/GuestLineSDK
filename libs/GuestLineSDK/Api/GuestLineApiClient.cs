// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK.Api;

public partial interface IGuestLineApiClient
{

}

public partial class GuestLineApiClient : ApiClient, IGuestLineApiClient
{
	public GuestLineApiClient(HttpClient http, GuestLineSettings settings)
		: base(http, settings) { }
}
