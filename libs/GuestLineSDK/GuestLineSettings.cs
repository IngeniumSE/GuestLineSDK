// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

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
	public string BaseUrl { get; set; } = "";

	/// <summary>
	/// Gets or sets whether to capture request content.
	/// </summary>
	public bool CaptureRequestContent { get; set; }

	/// <summary>
	/// Gets or sets whether to capture response content.
	/// </summary>
	public bool CaptureResponseContent { get; set; }

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
/// Validates instances of <see cref="GuestLineSettings"/>.
/// </summary>
public class GuestLineSettingsValidator : AbstractValidator<GuestLineSettings>
{
	public static readonly GuestLineSettingsValidator Instance = new();

	public GuestLineSettingsValidator()
	{
		bool ValidateUri(string value)
			=> Uri.TryCreate(value, UriKind.Absolute, out var _);

		RuleFor(s => s.BaseUrl)
			.Custom((value, context) =>
			{
				if (!ValidateUri(value))
				{
					context.AddFailure($"'{value}' is not a valid URI.");
				}
			});

		RuleFor(s => s.ApiKey)
			.NotEmpty()
			.WithMessage("The API key must not be empty.");
	}
}
