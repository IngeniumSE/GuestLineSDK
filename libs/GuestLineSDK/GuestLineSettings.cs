// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System.Security.Cryptography;

using FluentValidation;

using Microsoft.Extensions.Options;

namespace GuestLineSDK;

/// <summary>
/// Represents settings for configuring GuestLine.
/// </summary>
public class GuestLineSettings
{
	public const string ConfigurationSection = "GuestLine";

	/// <summary>
	/// Gets or sets the API key.
	/// </summary>
	public string ApiKey { get; set; } = default!;

	/// <summary>
	/// Gets or sets the base URL.
	/// </summary>
	public string? BookBaseUrl { get; set; }

	/// <summary>
	/// Gets or sets the GuestLine environment, defaulting to Production.
	/// </summary>
	public GuestLineEnvironment Environment { get; set; } = GuestLineEnvironment.Production;

	/// <summary>
	/// Gets or sets whether to capture request content.
	/// </summary>
	public bool CaptureRequestContent { get; set; }

	/// <summary>
	/// Gets or sets whether to capture response content.
	/// </summary>
	public bool CaptureResponseContent { get; set; }

	/// <summary>
	/// Gets or sets the partner identifier.
	/// </summary>
	public string PartnerId { get; set; } = "";

	/// <summary>
	/// Gets or sets the service base URL.
	/// </summary>
	public string? ServiceBaseUrl { get; set; }

	/// <summary>
	/// Gets or sets the default request version.
	/// </summary>
	public string Version { get; set; } = "2";

	/// <summary>
	/// Returns the settings as a wrapped options instance.
	/// </summary>
	/// <returns>The options instance.</returns>
	public IOptions<GuestLineSettings> AsOptions()
		=> Options.Create(this);

	/// <summary>
	/// Validates the current instance.
	/// </summary>
	public void Validate()
		=> GuestLineSettingsValidator.Instance.Validate(this);
}

/// <summary>
/// Specifies the GuestLine environment.
/// </summary>
public enum GuestLineEnvironment
{
	/// <summary>
	/// Production environment.
	/// </summary>
	Production,

	/// <summary>
	/// Test environment.
	/// </summary>
	Test
}

/// <summary>
/// Validates instances of <see cref="GuestLineSettings"/>.
/// </summary>
public class GuestLineSettingsValidator : AbstractValidator<GuestLineSettings>
{
	public static readonly GuestLineSettingsValidator Instance = new();

	public GuestLineSettingsValidator()
	{
		bool ValidateUri(string value)
			=> Uri.TryCreate(value, UriKind.Absolute, out var _);

		RuleFor(s => s.ApiKey)
			.NotEmpty()
			.WithMessage("The API key must not be empty.");

		RuleFor(s => s.BookBaseUrl)
			.Custom((value, context) =>
			{
				if (value is { Length: > 0 } && !ValidateUri(value))
				{
					context.AddFailure($"'{value}' is not a valid book URI.");
				}
			});

		RuleFor(s => s.PartnerId)
			.NotEmpty()
			.WithMessage("The partner ID must not be empty.");

		RuleFor(s => s.ServiceBaseUrl)
			.Custom((value, context) =>
			{
				if (value is { Length: > 0 } && !ValidateUri(value))
				{
					context.AddFailure($"'{value}' is not a valid service URI.");
				}
			});

		RuleFor(s => s.Version)
			.NotEmpty()
			.WithMessage("The version must not be empty.");
	}
}
