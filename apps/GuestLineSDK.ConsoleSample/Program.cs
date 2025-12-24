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

var ari = await api.Service.GetAriAsync(new GuestLineAriRequest(
	PropertyId: "12992",
	RoomId: "1299210STANDARD",
	RateId: "1299224125",
	Action: AriAction.FullYear));

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

string GetARIUpdate()
{
	return @"{
  ""propertyid"": ""934001"",
  ""room_id"": ""10512556XPQ3"",
  ""rate_id"": ""STAAH194181"",
  ""currency"": ""INR"",
  ""apikey"": ""GeT-aPi-DemoY-U1V8-bdt-03gEp-u1D8a4Y"",
  ""data"": [
    {
      ""cta"": ""N"",
      ""amountAfterTax"": {
        ""extrachild"": ""600"",
        ""Rate"": ""4900"",
        ""obp"": {
          ""person2"": ""5405"",
          ""person3"": ""5700"",
          ""person1"": ""4900""
        },
        ""extraadult"": ""800""
      },
      ""minstay"": ""1"",
      ""from_date"": ""2024-08-22"",
      ""to_date"": ""2024-08-22"",
      ""stopsell"": ""N"",
      ""amountBeforeTax"": {
        ""Rate"": ""4900"",
        ""extrachild"": ""600.00"",
        ""extraadult"": ""800.00"",
        ""obp"": {
          ""person2"": ""5400"",
          ""person3"": ""5700"",
          ""person1"": ""4900""
        }
      },
      ""ctd"": ""N"",
      ""inventory"": ""9"",
      ""maxstay"": ""28"",
      ""minstay_through"": ""1"",
      ""maxstay_through"": ""3""
    },
    {
      ""minstay"": ""1"",
      ""amountAfterTax"": {
        ""obp"": {
          ""person1"": ""4900"",
          ""person3"": ""5700"",
          ""person2"": ""5400""
        },
        ""extraadult"": ""800"",
        ""Rate"": ""4900"",
        ""extrachild"": ""600""
      },
      ""cta"": ""N"",
      ""stopsell"": ""N"",
      ""from_date"": ""2024-08-24"",
      ""to_date"": ""2024-08-24"",
      ""amountBeforeTax"": {
        ""extrachild"": ""600.00"",
        ""Rate"": ""4900"",
        ""obp"": {
          ""person3"": ""5758"",
          ""person1"": ""4900"",
          ""person2"": ""5400""
        },
        ""extraadult"": ""800.00""
      },
      ""ctd"": ""N"",
      ""maxstay"": ""28"",
      ""inventory"": ""9"",
      ""minstay_through"": ""1"",
      ""maxstay_through"": ""3""
    }
  ],
  ""trackingId"": ""FA81B5AD-E050-4501-81C9-E33EAD371762"",
  ""version"": ""2""
}";
}
