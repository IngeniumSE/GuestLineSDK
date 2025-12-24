// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK;

using System;

static class GuestLineUriResolver
{
	public static string ResolveBookBaseUrl(GuestLineSettings settings)
	{
		if (!string.IsNullOrEmpty(settings.BookBaseUrl))
		{
			return settings.BookBaseUrl!;
		}

		return settings.Environment switch
		{
			GuestLineEnvironment.Production => $"https://cmbooking.otaswitch.com/common-cgi/test/booking_{settings.PartnerId}.pl",
			GuestLineEnvironment.Test => $"https://cmbooking.otaswitch.com/common-cgi/test/booking_{settings.PartnerId}.pl",
			_ => throw new ArgumentOutOfRangeException(nameof(settings.Environment), "Unknown GuestLine environment."),
		};
	}

	public static string ResolveServiceBaseUrl(GuestLineSettings settings)
	{
		if (!string.IsNullOrEmpty(settings.ServiceBaseUrl))
		{
			return settings.ServiceBaseUrl!;
		}

		return settings.Environment switch
		{
			GuestLineEnvironment.Production => $"https://channelconnect.otaswitch.com/common-cgi/{settings.PartnerId}/test/services.pl",
			GuestLineEnvironment.Test => $"https://channelconnect.otaswitch.com/common-cgi/{settings.PartnerId}/test/services.pl",
			_ => throw new ArgumentOutOfRangeException(nameof(settings.Environment), "Unknown GuestLine environment."),
		};
	}
}
