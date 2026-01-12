// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK.Api;

using System.Text.Json.Serialization;

using FluentValidation;

using GuestLineSDK.Ari;
using GuestLineSDK.Primitives;

partial interface IGuestLineApiClient
{
	/// <summary>
	/// Reservation Operations
	/// </summary>
	IReservationOperations Reservation { get; }
}

partial class GuestLineApiClient
{
	Lazy<IReservationOperations>? _reservation;
	public IReservationOperations Reservation => (_reservation ??= Defer<IReservationOperations>(
		c => new ReservationOperations(c))).Value;
}

public partial interface IReservationOperations
{
	Task<GuestLineResponse<ReservationRefSet>> ProcessReservationAsync(
		GuestLineReservationRequest request,
		CancellationToken cancellationToken = default);
}

public class ReservationOperations(ApiClient client) : IReservationOperations
{
	readonly ApiClient _client = client;

	public async Task<GuestLineResponse<ReservationRefSet>> ProcessReservationAsync(GuestLineReservationRequest request, CancellationToken cancellationToken = default)
	{

		Ensure.IsNotNull(request, nameof(request));

		request.ApiKey ??= _client.Settings.ApiKey;
		request.Version ??= _client.Settings.Version;

		request.Validate();

		var req = new GuestLineRequest<GuestLineReservationRequest>(
			GuestLineService.Reservation,
			HttpMethod.Post,
			PathString.Empty,
			request);

		var (res, data) = await _client.FetchRawAsync<GuestLineReservationRequest, ReservationRef[]>(req, cancellationToken)
			.ConfigureAwait(false);

		if (!res.IsSuccess)
		{
			return new GuestLineResponse<ReservationRefSet>(
				res.RequestMethod,
				res.RequestUri,
				res.IsSuccess,
				res.StatusCode,
				error: res.Error);
		}

		string? trackingId = null;
		List<ReservationRef> refs = new();
		List<string> errors = new();
		bool success = true;

		if (data != null)
		{
			var tracker = data.FirstOrDefault(x => !string.IsNullOrEmpty(x.TrackingId));
			trackingId = tracker?.TrackingId;

			for (int i = 0; i < data.Length; i++)
			{
				var @ref = data[i];
				if (@ref == tracker)
				{
					continue;
				}

				if (@ref.Error is { Length: >0 } || string.Equals("fail", @ref.Status, StringComparison.OrdinalIgnoreCase))
				{
					success = false;
					if (@ref.Error is { Length: >0 })
					{
						errors.Add(@ref.Error);
					}
				}

				@ref.TrackingId = tracker?.TrackingId;
				refs.Add(@ref);
			}
		}

		return new GuestLineResponse<ReservationRefSet>(
			res.RequestMethod,
			res.RequestUri,
			success,
			res.StatusCode,
			data: new ReservationRefSet()
			{
				Reservations = refs.ToArray(),
				TrackingId = trackingId
			},
			error: res.Error != null || errors.Any()
				? res.Error ?? new ErrorResponse() { Error = string.Join("\n", errors), Status = "Fail", TrackingId = trackingId }
				: null
		)
		{
			RequestContent = res.RequestContent,
			ResponseContent = res.ResponseContent
		};
		
	}
}

public record GuestLineReservationRequest(
	[property: JsonPropertyName("propertyid")] string PropertyId,
	[property: JsonIgnore] ReservationRequest[] ReservationItems)
	: GuestLineRequestBase<GuestLineReservationRequest>("reservation_info")
{
	[JsonPropertyName("reservations")]
	public GuestLineReservationSet Reservations => new(ReservationItems);

	/// <summary>
	/// Validates the current instance.
	/// </summary>
	public override IValidator<GuestLineReservationRequest> GetValidator()
		=> GuestLineReservationRequestValidator.Instance;
}

public record GuestLineReservationSet(
	[property: JsonPropertyName("reservation")] ReservationRequest[] Reservations);

public class GuestLineReservationRequestValidator : GuestLineRequestBaseValidator<GuestLineReservationRequest>
{
	public static readonly GuestLineReservationRequestValidator Instance = new();

	public GuestLineReservationRequestValidator()
	{
		RuleFor(s => s.PropertyId)
			.NotEmpty()
			.WithMessage("The Property ID parameter must not be empty.");
	}
}

public class ReservationRef : Result<ReservationRef>
{
	[JsonPropertyName("bookingId")]
	public string? BookingId { get; set; }
}

public class ReservationRefSet : Result<ReservationRefSet>
{
	[JsonPropertyName("reservations")]
	public ReservationRef[]? Reservations { get; set; }
}

public record ReservationRequest(
	[property: JsonPropertyName("reservation_datetime"), JsonConverter(typeof(DateTimeJsonConverter))] DateTime ReservationDateTime,
	[property: JsonPropertyName("reservation_id")] string ReservationId,
	[property: JsonPropertyName("payment_required")] decimal PaymentRequired,
	[property: JsonPropertyName("payment_type")] string PaymentType,
	[property: JsonPropertyName("totalamountaftertax")] decimal TotalAmountAfterTax,
	[property: JsonPropertyName("totaltax")] decimal TotalTax,
	[property: JsonPropertyName("currencycode")] string CurrencyCode,
	[property: JsonPropertyName("status")] string Status,
	[property: JsonPropertyName("customer")] ReservationCustomer Customer,
	[property: JsonPropertyName("room")] ReservationRoom[] Rooms,
	[property: JsonPropertyName("POS")] string Source)
{
	public ReservationRequest AsCancellation(DateTime cancellationDateTime) => this with { Status = "Cancel", ReservationDateTime = cancellationDateTime };
}

public class ReservationCustomer
{
	[JsonPropertyName("address")]
	public string? Address { get; set; }

	[JsonPropertyName("city")]
	public string? City { get; set; }

	[JsonPropertyName("country")]
	public string? Country { get; set; }

	[JsonPropertyName("email")]
	public string? Email { get; set; }

	[JsonPropertyName("salutation")]
	public string? Salutation { get; set; }

	[JsonPropertyName("first_name")]
	public string? FirstName { get; set; }

	[JsonPropertyName("last_name")]
	public string? LastName { get; set; }

	[JsonPropertyName("remarks")]
	public string? Remarks { get; set; }

	[JsonPropertyName("telephone")]
	public string? Telephone { get; set; }

	[JsonPropertyName("zip")]
	public string? Zip { get; set; }

}

public record ReservationRoom(
	[property: JsonPropertyName("arrival_date"), JsonConverter(typeof(DateOnlyJsonConverter))] DateTime ArrivalDate,
	[property: JsonPropertyName("departure_date"), JsonConverter(typeof(DateOnlyJsonConverter))] DateTime DepatureDate,
	[property: JsonPropertyName("room_id")] string RoomId,
	[property: JsonPropertyName("price")] ReservationRate[] Price,
	[property: JsonPropertyName("first_name")] string FirstName,
	[property: JsonPropertyName("last_name")] string LastName,
	[property: JsonPropertyName("remarks")] string? Remarks,
	[property: JsonPropertyName("amountaftertax")] decimal AmountAfterTax,
	[property: JsonIgnore] int Adults,
	[property: JsonIgnore] int Children)
{
	[JsonPropertyName("GuestCount")]
	public ReservationGuestGrouping[]? Guests => GetGroupings().ToArray();

	IEnumerable<ReservationGuestGrouping> GetGroupings()
	{
		if (Adults > 0)
		{
			yield return new ReservationGuestGrouping(10, Adults);
		}
		if (Children > 0)
		{
			yield return new ReservationGuestGrouping(8, Children);
		}
	}
}

public record ReservationRate(
	[property: JsonPropertyName("date"), JsonConverter(typeof(DateOnlyJsonConverter))] DateTime Date,
	[property: JsonPropertyName("rate_id")] string RateId,
	[property: JsonPropertyName("amountaftertax")] decimal AmountAfterTax);

public record ReservationGuestGrouping(
	[property: JsonPropertyName("AgeQualifyingCode")] int AgeQualifyingCode,
	[property: JsonPropertyName("Count")] int Count);
