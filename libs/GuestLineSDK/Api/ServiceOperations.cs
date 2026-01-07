// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK.Api;

using System.Net.NetworkInformation;
using System.Text.Json.Serialization;

using FluentValidation;

using GuestLineSDK.Ari;
using GuestLineSDK.Primitives;

partial interface IGuestLineApiClient
{
		/// <summary>
		/// Service Operations
		/// </summary>
		IServiceOperations Service { get; }
}

partial class GuestLineApiClient
{
	Lazy<IServiceOperations>? _service;
	public IServiceOperations Service => (_service ??= Defer<IServiceOperations>(
		c => new ServiceOperations(c))).Value;
}

public partial interface IServiceOperations
{
	Task<GuestLineResponse<AriUpdate>> GetAriAsync(
		GuestLineAriRequest request,
		CancellationToken cancellationToken = default);

	Task<GuestLineResponse<PropertyRef>> GetPropertyAsync(
		GuestLinePropertyRequest request,
		CancellationToken cancellationToken = default);
}

public partial class ServiceOperations(ApiClient client) : IServiceOperations
{
	readonly ApiClient _client = client;

	public async Task<GuestLineResponse<AriUpdate>> GetAriAsync(
		GuestLineAriRequest request,
		CancellationToken cancellationToken = default)
	{
		Ensure.IsNotNull(request, nameof(request));

		request.ApiKey ??= _client.Settings.ApiKey;
		request.Version ??= _client.Settings.Version;

		request.Validate();

		var req = new GuestLineRequest<GuestLineAriRequest>(
			GuestLineService.ARI,
			HttpMethod.Post,
			PathString.Empty,
			request);

		return await _client.FetchAsync<GuestLineAriRequest, AriUpdate>(req, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<GuestLineResponse<PropertyRef>> GetPropertyAsync(
		GuestLinePropertyRequest request,
		CancellationToken cancellationToken = default)
	{
		Ensure.IsNotNull(request, nameof(request));

		request.ApiKey ??= _client.Settings.ApiKey;
		request.Version ??= _client.Settings.Version;

		request.Validate();

		var req = new GuestLineRequest<GuestLinePropertyRequest>(
			GuestLineService.ARI,
			HttpMethod.Post,
			PathString.Empty,
			request);

		return await _client.FetchAsync<GuestLinePropertyRequest, PropertyRef>(req, cancellationToken)
			.ConfigureAwait(false);
	}
}

public static class ServiceOperationExtensions
{
	public static async Task<GuestLineResponse<PropertyRef>> GetPropertyAsync(
		this IServiceOperations ops,
		string propertyId,
		CancellationToken cancellationToken = default)
		=> await ops.GetPropertyAsync(new GuestLinePropertyRequest(propertyId), cancellationToken).ConfigureAwait(false);
}

#region ARI
public enum AriAction
{
	/// <summary>
	/// Request a range of up to 28 days of ARI data.
	/// </summary>
	Range,

	/// <summary>
	/// Request a full year of ARI data.
	/// </summary>
	FullYear
}

/// <summary>
/// Represents a request for GuestLine ARI data.
/// </summary>
/// <param name="action">The ARI action (range or full year)</param>
/// <param name="start">The start date of the range (if provided)</param>
/// <param name="end">The end date of the range (if provided)</param>
public record GuestLineAriRequest(
	[property: JsonPropertyName("propertyid")] string PropertyId,
	[property: JsonPropertyName("room_id")] string RoomId,
	[property: JsonPropertyName("rate_id")] string RateId,

	[property: JsonIgnore] AriAction AriAction = AriAction.FullYear,
	[property: JsonPropertyName("from_date")] DateTime? Start = null,
	[property: JsonPropertyName("to_date")] DateTime? End = null)
	: GuestLineRequestBase<GuestLineAriRequest>(GetAction(AriAction))
{
	public override IValidator<GuestLineAriRequest>? GetValidator()
		=> GuestLineAriRequestValidator.Instance;

	public static string GetAction(AriAction action)
	{
		return action switch
		{
			AriAction.FullYear => "year_info_ARR",
			AriAction.Range => "ARR_info",
			_ => throw new NotSupportedException($"The ARI action '{action}' is not supported.")
		};
	}
}

public class GuestLineAriRequestValidator : GuestLineRequestBaseValidator<GuestLineAriRequest>
{
	public static readonly GuestLineAriRequestValidator Instance = new();

	public GuestLineAriRequestValidator()
	{
		RuleFor(s => s.PropertyId)
			.NotEmpty()
			.WithMessage("The Property ID parameter must not be empty.");

		RuleFor(s => s.RoomId)
			.NotEmpty()
			.WithMessage("The Room ID parameter must not be empty.");

		RuleFor(s => s.RateId)
			.NotEmpty()
			.WithMessage("The Rate ID parameter must not be empty.");

		When(r => r.AriAction == AriAction.Range, () =>
		{
			RuleFor(s => s.Start)
				.NotEmpty()
				.GreaterThanOrEqualTo(DateTime.Today.Date)
				.WithMessage("The start of the ARI range must be today or after");

			RuleFor(s => s.End)
				.Custom((end, c) =>
				{
					var start = c.InstanceToValidate.Start;
					if (!end.HasValue)
					{
						c.AddFailure("The end of the ARI range must be on or after the start of the range");
					}
					else if (!start.HasValue)
					{
						c.AddFailure("Cannot request a range without a valid start and end date");
					}
					else
					{
						var diff = (end.Value - start.Value).Days;
						if (diff < 0)
						{
							c.AddFailure("The end of the range cannot be before the start");
						}
						else if (diff >= 28)
						{
							c.AddFailure("The range cannot extend beyond 28 days when requesting ARI data.");
						}
					}
				});
		});
	}
}
#endregion

#region Property Ref
public record GuestLinePropertyRequest(
	[property: JsonPropertyName("propertyid")] string PropertyId)
	: GuestLineRequestBase<GuestLinePropertyRequest>(Action: "property_info")
{
	public override IValidator<GuestLinePropertyRequest>? GetValidator()
		=> GuestLinePropertyRequestValidator.Instance;
}

public class GuestLinePropertyRequestValidator : GuestLineRequestBaseValidator<GuestLinePropertyRequest>
{
	public static readonly GuestLinePropertyRequestValidator Instance = new();

	public GuestLinePropertyRequestValidator()
	{
		RuleFor(s => s.PropertyId)
			.NotEmpty()
			.WithMessage("The Property ID parameter must not be empty.");
	}
}

public class PropertyRef : Result<PropertyRef>
{
	[JsonPropertyName("currency")]
	public string? CurrencyCode { get; set; }

	[JsonPropertyName("propertyname")]
	public string? PropertyName { get; set; }

	[JsonPropertyName("checkintime")]
	public string? CheckInTime { get; set; }

	[JsonPropertyName("checkouttime")]
	public string? CheckOutTime { get; set; }

	[JsonPropertyName("contactinfo")]
	public ContactRef? ContactInfo { get; set; }
}

public class ContactRef : Model<ContactRef>
{
	[JsonPropertyName("addressline")]
	public string? Address { get; set; }

	[JsonPropertyName("city")]
	public string? City { get; set; }

	[JsonPropertyName("country")]
	public string? Country { get; set; }

	[JsonPropertyName("fax")]
	public string? Fax { get; set; }

	[JsonPropertyName("latitude")]
	public decimal? Latitude { get; set; }

	[JsonPropertyName("longitude")]
	public decimal? Longitude { get; set; }

	[JsonPropertyName("location")]
	public string? Location { get; set; }

	[JsonPropertyName("telephone")]
	public string? Telephone { get; set; }

	[JsonPropertyName("zip")]
	public string? Zip { get; set; }
}

#endregion
