// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK.Api;

using System.Text.Json.Serialization;

using FluentValidation;

using GuestLineSDK.Ari;

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
}

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

	[property: JsonIgnore] AriAction Action = AriAction.FullYear,
	[property: JsonPropertyName("from_date")] DateTime? Start = null,
	[property: JsonPropertyName("to_date")] DateTime? End = null)
{
	[JsonPropertyName("action")]
	public string? ActionValue
	{
		get
		{
			return Action switch
			{
				AriAction.FullYear => "year_info_ARR",
				AriAction.Range => "ARR_info",
				_ => null
			};
		}
	}

	[JsonPropertyName("apikey")]
	public string? ApiKey { get; set; }

	[JsonPropertyName("version")]
	public string? Version { get; set; }

	/// <summary>
	/// Validates the current instance.
	/// </summary>
	public void Validate()
		=> GuestLineAriRequestValidator.Instance.Validate(this);
}

public class GuestLineAriRequestValidator : AbstractValidator<GuestLineAriRequest>
{
	public static readonly GuestLineAriRequestValidator Instance = new();

	public GuestLineAriRequestValidator()
	{
		RuleFor(s => s.ActionValue)
			.NotEmpty()
			.WithMessage("The Action parameter must be a valid action.");

		RuleFor(s => s.ApiKey)
			.NotEmpty()
			.WithMessage("The API key must not be empty.");

		RuleFor(s => s.PropertyId)
			.NotEmpty()
			.WithMessage("The Property ID parameter must not be empty.");

		RuleFor(s => s.RoomId)
			.NotEmpty()
			.WithMessage("The Room ID parameter must not be empty.");

		RuleFor(s => s.RateId)
			.NotEmpty()
			.WithMessage("The Rate ID parameter must not be empty.");

		RuleFor(s => s.Version)
			.NotEmpty()
			.WithMessage("The Version parameter must not be empty.");

		When(r => r.Action == AriAction.Range, () =>
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
						else if (diff > 28)
						{
							c.AddFailure("The range cannot extend beyond 28 days when requesting ARI data.");
						}
					}
				});
		});
	}
}
