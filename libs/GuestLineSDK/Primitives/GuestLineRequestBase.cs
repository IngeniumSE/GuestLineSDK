// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK.Primitives;

using System.Text.Json.Serialization;

using FluentValidation;

/// <summary>
/// Serves as the base record for GuestLine API request types, providing common properties and validation support.
/// </summary>
/// <remarks>This base record defines shared properties such as API key and version, and provides a mechanism for
/// request validation. Derived request types should inherit from this record to ensure consistent structure and
/// validation behavior across GuestLine API requests.</remarks>
/// <typeparam name="TRequest">The type of the derived request. Must inherit from GuestLineRequestBase<TRequest>.</typeparam>
public record GuestLineRequestBase<TRequest>(
	[property: JsonPropertyName("action")] string Action)
	where TRequest : GuestLineRequestBase<TRequest>
{
	[JsonPropertyName("apikey")]
	public string? ApiKey { get; set; }

	[JsonPropertyName("version")]
	public string? Version { get; set; }

	/// <summary>
	/// Retrieves a validator for the current request type, if one is available.
	/// </summary>
	/// <remarks>Override this method to provide custom validation logic for specific request types.</remarks>
	/// <returns>An <see cref="IValidator{TRequest}"/> instance that can validate the request, or <see langword="null"/> if no
	/// validator is available.</returns>
	public virtual IValidator<TRequest>? GetValidator() => null;

	/// <summary>
	/// Validates the current request instance using the associated validator, if one is available.
	/// </summary>
	/// <remarks>If no validator is configured for the request type, this method performs no action. Override this
	/// method to customize validation behavior for derived request types.</remarks>
	public virtual void Validate()
	{
		var validator = GetValidator() ?? new GuestLineRequestBaseValidator<TRequest>();
		if (validator is null)
		{
			return;
		}

		validator.Validate((TRequest)this);
	}
}

/// <summary>
/// Provides validation rules for requests derived from GuestLineRequestBase<TRequest>.
/// </summary>
/// <remarks>This validator ensures that the Action, ApiKey, and Version properties of the request are not empty.
/// It is intended to be used as a base validator for specific GuestLine request types.</remarks>
/// <typeparam name="TRequest">The type of request to validate. Must inherit from GuestLineRequestBase<TRequest>.</typeparam>
public class GuestLineRequestBaseValidator<TRequest> :
	AbstractValidator<TRequest>
	where TRequest : GuestLineRequestBase<TRequest>
{
	public GuestLineRequestBaseValidator()
	{
		RuleFor(x => x.Action)
			.NotEmpty()
			.WithMessage("Action must be provided.");

		RuleFor(x => x.ApiKey)
			.NotEmpty()
			.WithMessage("API key must be provided.");

		RuleFor(x => x.Version)
			.NotEmpty()
			.WithMessage("API version must be provided.");
	}
}
