// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System.Text.Json.Serialization;

using GuestLineSDK.Primitives;

namespace GuestLineSDK.Ari;

/// <summary>
/// Provides a generic base model for ARI updates, encapsulating common properties shared across different ARI update types.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class AriUpdateRef<T> : Model<T>
	where T : Model<T>
{
	/// <summary>
	/// The unqiue apikey of STAAH shared with OTA
	/// </summary>
	[JsonPropertyName("apikey")]
	public string? ApiKey { get; set; }

	/// <summary>
	/// Property currency
	/// </summary>
	[JsonPropertyName("currency")]
	public string? Currency { get; set; }

	/// <summary>
	/// The unique property id of OTA shared with STAAH
	/// </summary>
	[JsonPropertyName("propertyid")]
	public string? PropertyId { get; set; }

	/// <summary>
	/// OTA Rate ID
	/// </summary>
	[JsonPropertyName("rate_id")]
	public string? RateId { get; set; }

	/// <summary>
	/// OTA Room ID
	/// </summary>
	[JsonPropertyName("room_id")]
	public string? RoomId { get; set; }

	/// <summary>
	/// A unique tracking ID for STAAH. This can be used for troubleshooting if required
	/// </summary>
	[JsonPropertyName("trackingId")]
	public string? TrackingId { get; set; }

	/// <summary>
	/// Action name must be version
	/// </summary>
	[JsonPropertyName("version")]
	public string? Version { get; set; }
}

/// <summary>
/// Provides a slimmed-down reference model for an ARI update, excluding detailed data records.
/// </summary>
public class AriUpdateRef : AriUpdateRef<AriUpdateRef>
{

}	

/// <summary>
/// Represents a model for an ARI update, containing property-specific update records and related metadata.
/// </summary>
public class AriUpdate : AriUpdateRef<AriUpdate>
{
	/// <summary>
	/// Array of information for Inventory, Rates, Restric tion date wise of requested Room ID and Rate ID.
	/// </summary>
	[JsonPropertyName("data")]
	public AriUpdateData[]? Data { get; set; }
}

/// <summary>
/// Represents a data item in an ARI update.
/// </summary>
public class AriUpdateData : Model<AriUpdateData>
{
	/// <summary>
	/// Amount after tax.
	/// </summary>
	[JsonPropertyName("amountAfterTax")]
	public AriUpdateDataPricing? AmountAfterTax { get; set; }

	/// <summary>
	/// Amount before tax.
	/// </summary>
	[JsonPropertyName("amountBeforeTax")]
	public AriUpdateDataPricing? AmountBeforeTax { get; set; }

	/// <summary>
	/// Closed to arrival. If true, the guest cannot check in on this day.
	/// </summary>
	[JsonPropertyName("cta"), JsonConverter(typeof(BooleanCharacterJsonConverter))]
	public bool ClosedToArrival { get; set; }

	/// <summary>
	/// Closed to departure. If true, the guest cannot check out on this day.
	/// </summary>
	[JsonPropertyName("ctd"), JsonConverter(typeof(BooleanCharacterJsonConverter))]
	public bool ClosedToDeparture { get; set; }

	/// <summary>
	/// Date in YYYY-MM-DD format.
	/// </summary>
	[JsonPropertyName("date"), JsonConverter(typeof(DateOnlyJsonConverter))]
	public DateTime? Date { get; set; }

	/// <summary>
	/// Date in YYYY-MM-DD format.
	/// </summary>
	[JsonPropertyName("from_date"), JsonConverter(typeof(DateOnlyJsonConverter))]
	public DateTime? FromDate { get; set; }

	/// <summary>
	/// Number of rooms available.
	/// </summary>
	[JsonPropertyName("inventory")]
	public int? Inventory { get; set; }

	/// <summary>
	/// Maximum length of stay.
	/// </summary>
	[JsonPropertyName("maxstay")]
	public int? MaxStay { get; set; }

	/// <summary>
	/// Maximum length of stay through.
	/// </summary>
	[JsonPropertyName("maxstay_through")]
	public int? MaxStayThrough { get; set; }

	/// <summary>
	/// Minimum length of stay.
	/// </summary>
	[JsonPropertyName("minstay")]
	public int? MinStay { get; set; }

	/// <summary>
	/// Minimum length of stay through.
	/// </summary>
	[JsonPropertyName("minstay_through")]
	public int? MinStayThrough { get; set; }

	/// <summary>
	/// Flag to make date(s) not booking whether there is inventory or not
	/// </summary>
	[JsonPropertyName("stopsell"), JsonConverter(typeof(BooleanCharacterJsonConverter))]
	public bool StopSell { get; set; }

	/// <summary>
	/// Date in YYYY-MM-DD format.
	/// </summary>
	[JsonPropertyName("to_date"), JsonConverter(typeof(DateOnlyJsonConverter))]
	public DateTime? ToDate { get; set; }
}

/// <summary>
/// Represents pricing for an ARI update.
/// </summary>
public class AriUpdateDataPricing : Model<AriUpdateDataPricing>
{
	/// <summary>
	/// Room Price
	/// </summary>
	[JsonPropertyName("Rate"), JsonConverter(typeof(CoerceToMoneyDecimalJsonConverter))]
	public decimal? Rate { get; set; }

	/// <summary>
	/// Extra Adult Rate
	/// </summary>
	[JsonPropertyName("extraadult"), JsonConverter(typeof(CoerceToMoneyDecimalJsonConverter))]
	public decimal? ExtraAdultRate { get; set; }

	/// <summary>
	/// Extra Child Rate
	/// </summary>
	[JsonPropertyName("extrachild"), JsonConverter(typeof(CoerceToMoneyDecimalJsonConverter))]
	public decimal? ExtraChildRate { get; set; }

	/// <summary>
	/// Occupancy Based Pricing
	/// </summary>
	[JsonPropertyName("obp"), JsonConverter(typeof(ObpDictionaryJsonConverter))]
	public Dictionary<int, decimal?>? OccupancyBasedPricing { get; set; }
}
