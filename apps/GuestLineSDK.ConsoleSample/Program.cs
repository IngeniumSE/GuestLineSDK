// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System.Net.Http.Headers;

using GuestLineSDK;
using GuestLineSDK.Api;
using GuestLineSDK.Ari;

using Microsoft.Extensions.Configuration;

var settings = GetSettings();
var http = CreateHttpClient();
var api = new GuestLineApiClient(http, settings);

var prop = await api.Service.GetPropertyAsync("12992");

var ari = await api.Service.GetAriAsync(new GuestLineAriRequest(
	PropertyId: "12992",
	RoomId: "1299210STANDARD",
	RateId: "1299224125",
	AriAction: AriAction.Range,
	Start: new(2026, 02, 15),
	End: new(2026, 02, 16)
));

var res = new ReservationRequest(
	DateTime.UtcNow,
	"X12345",
	0,
	"Hotel Collect",
	999.99m,
	Math.Round(999.99m / 6),
	prop.Data!.CurrencyCode!,
	"Confirm",
	new ReservationCustomer
	{
		FirstName = "Matthew",
		LastName = "Abbott"
	},
	[
		new ReservationRoom(
			new(2026, 02, 15),
			new(2026, 02, 16),
			"1299210STANDARD",
			[
				new ReservationRate(
					new(2026, 02, 15),
					"1299224125",
					999.99m)
			],
			"Matthew",
			"Abbott",
			"Test Order",
			999.99m,
			1,
			0)
	],
	"SpaSeekers");

var reservationResult = await api.Reservation.ProcessReservationAsync(
	new GuestLineReservationRequest(
		"12992",
		[res, res]));

//string json = GetARIUpdate();

//var updateRef = AriUpdateRef.FromJsonString(json);
//var update = AriUpdate.FromJsonString(json);
Console.ReadKey();

GuestLineSettings GetSettings()
{
	var configuration = new ConfigurationBuilder()
		.AddJsonFile("./appsettings.json", optional: false)
		.AddJsonFile("./appsettings.env.json", optional: true)
		.Build();

	GuestLineSettings settings = new();
	configuration.GetSection(GuestLineSettings.ConfigurationSection).Bind(settings);

	settings.Validate();

	return settings;
}

HttpClient CreateHttpClient()
{
	var http = new HttpClient();

	http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

	return http;
}
