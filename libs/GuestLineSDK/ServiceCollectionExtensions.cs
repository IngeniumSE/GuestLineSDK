// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System.Net.Http.Headers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using GuestLineSDK;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extensions for the <see cref="IServiceCollection"/> type.
/// </summary>
public static class ServiceCollectionExtensions
{
	const string DefaultApiClientName = "GuestLine";

	/// <summary>
	/// Adds Sailthru services to the given services collection.
	/// </summary>
	/// <param name="services">The services collection.</param>
	/// <param name="configure">The configure delegate.</param>
	/// <returns>The services collection.</returns>
	public static IServiceCollection AddGuestLine(
		this IServiceCollection services,
		Action<GuestLineSettings> configure)
	{
		Ensure.IsNotNull(services, nameof(services));
		Ensure.IsNotNull(configure, nameof(configure));

		services.Configure(configure);

		AddCoreServices(services);

		return services;
	}

	/// <summary>
	/// Adds Sailthru services to the given services collection.
	/// </summary>
	/// <param name="services">The services collection.</param>
	/// <param name="settings">The Sailthru settings.</param>
	/// <returns>The services collection.</returns>
	public static IServiceCollection AddGuestLine(
		this IServiceCollection services,
		GuestLineSettings settings)
	{
		Ensure.IsNotNull(services, nameof(services));
		Ensure.IsNotNull(settings, nameof(settings));

		services.AddSingleton(settings.AsOptions());

		AddCoreServices(services);

		return services;
	}

	/// <summary>
	/// Adds Sailthru services to the given services collection.
	/// </summary>
	/// <param name="services">The services collection.</param>
	/// <param name="configuration">The configuration.</param>
	/// <returns>The services collection.</returns>
	public static IServiceCollection AddGuestLine(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		Ensure.IsNotNull(services, nameof(services));
		Ensure.IsNotNull(configuration, nameof(configuration));

		services.Configure<GuestLineSettings>(
			configuration.GetSection(GuestLineSettings.ConfigurationSection));

		AddCoreServices(services);

		return services;
	}

	static void AddCoreServices(IServiceCollection services)
	{
		services.AddSingleton(sp =>
		{
			var settings = sp.GetRequiredService<IOptions<GuestLineSettings>>().Value;

			settings.Validate();

			return settings;
		});

		services.AddScoped<IGuestLineHttpClientFactory, GuestLineHttpClientFactory>();
		services.AddScoped<IGuestLineApiClientFactory, GuestLineApiClientFactory>();
		AddApiClient(
			services,
			DefaultApiClientName,
			(cf, settings) => cf.CreateApiClient(settings, DefaultApiClientName));
	}

	static void AddApiClient<TClient>(
		IServiceCollection services,
		string name,
		Func<IGuestLineApiClientFactory, GuestLineSettings, TClient> factory)
		where TClient : class
	{
		void ConfigureHttpDefaults(HttpClient http)
		{
			http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		services.AddHttpClient(name, ConfigureHttpDefaults);

		services.AddScoped(sp =>
		{
			var settings = sp.GetRequiredService<GuestLineSettings>();
			var clientFactory = sp.GetRequiredService<IGuestLineApiClientFactory>();

			return factory(clientFactory, settings);
		});
	}
}
