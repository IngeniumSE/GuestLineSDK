// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GuestLineSDK;

/// <summary>
/// Provides a base implementation of an API client.
/// </summary>
public abstract class ApiClient
{
	readonly HttpClient _http;
	readonly GuestLineSettings _settings;
	readonly JsonSerializerOptions _serializerOptions = JsonUtility.CreateSerializerOptions();
	readonly JsonSerializerOptions _deserializerOptions = JsonUtility.CreateDeserializerOptions();
	readonly Uri _baseUrl;

	protected ApiClient(HttpClient http, GuestLineSettings settings)
	{
		_http = Ensure.IsNotNull(http, nameof(http));
		_settings = Ensure.IsNotNull(settings, nameof(settings));

		_baseUrl = new Uri(_settings.BaseUrl);
	}

	#region Send and Fetch
	protected internal async Task<GuestLineResponse> SendAsync(
		GuestLineRequest request,
		CancellationToken cancellationToken = default)
	{
		Ensure.IsNotNull(request, nameof(request));
		using var httpReq = CreateHttpRequest(request);
		HttpResponseMessage? httpResp = null;

		try
		{
			httpResp = await _http.SendAsync(httpReq, cancellationToken)
				.ConfigureAwait(false);

			var (transformedResponse, capturedResponseContent) = await TransformResponse(
				httpReq.Method,
				httpReq.RequestUri,
				httpResp)
				.ConfigureAwait(false);

			if (_settings.CaptureResponseContent || !httpResp.IsSuccessStatusCode)
			{
				transformedResponse.ResponseContent = capturedResponseContent;
			}

			return transformedResponse;
		}
		catch (Exception ex)
		{
			var response = new GuestLineResponse(
				httpReq.Method,
				httpReq.RequestUri,
				false,
				(HttpStatusCode)0,
				error: new Error(ex.Message, exception: ex));

			return response;
		}
	}

	protected internal async Task<GuestLineResponse> SendAsync<TRequest>(
		GuestLineRequest<TRequest> request,
		CancellationToken cancellationToken = default)
		where TRequest : notnull
	{
		Ensure.IsNotNull(request, nameof(request));
		var (httpReq, capturedRequestContent) = CreateHttpRequest(request);
		HttpResponseMessage? httpResp = null;

		try
		{
			httpResp = await _http.SendAsync(httpReq, cancellationToken)
				.ConfigureAwait(false);

			var (transformedResponse, capturedResponseContent) = await TransformResponse(
				httpReq.Method,
				httpReq.RequestUri,
				httpResp)
					.ConfigureAwait(false); ;

			if (_settings.CaptureRequestContent)
			{
				transformedResponse.RequestContent = capturedRequestContent;
			}

			if (_settings.CaptureResponseContent || !httpResp.IsSuccessStatusCode)
			{
				transformedResponse.ResponseContent = capturedResponseContent;
			}

			return transformedResponse;
		}
		catch (Exception ex)
		{
			var response = new GuestLineResponse(
				httpReq.Method,
				httpReq.RequestUri,
				false,
				(HttpStatusCode)0,
				error: new Error(ex.Message, exception: ex));

			return response;
		}
	}

	protected internal async Task<GuestLineResponse<TResponse>> FetchAsync<TResponse>(
		GuestLineRequest request,
		CancellationToken cancellationToken = default)
		where TResponse : class
	{
		Ensure.IsNotNull(request, nameof(request));
		var httpReq = CreateHttpRequest(request);
		HttpResponseMessage? httpResp = null;

		try
		{
			httpResp = await _http.SendAsync(httpReq, cancellationToken)
				.ConfigureAwait(false);

			var (transformedResponse, capturedResponseContent) = await TransformResponse<TResponse>(
				httpReq.Method,
				httpReq.RequestUri,
				httpResp)
					.ConfigureAwait(false);

			if (_settings.CaptureResponseContent || !httpResp.IsSuccessStatusCode)
			{
				transformedResponse.ResponseContent = capturedResponseContent;
			}

			return transformedResponse;
		}
		catch (Exception ex)
		{
			var response = new GuestLineResponse<TResponse>(
				httpReq.Method,
				httpReq.RequestUri,
				false,
				(HttpStatusCode)0,
				error: new Error(ex.Message, exception: ex));

			return response;
		}
		finally
		{
			if (httpReq is not null)
			{
				httpReq.Dispose();
			}

			if (httpResp is not null)
			{
				httpResp.Dispose();
			}
		}
	}

	protected internal async Task<GuestLineResponse<TResponse>> FetchAsync<TRequest, TResponse>(
		GuestLineRequest<TRequest> request,
		CancellationToken cancellationToken = default)
		where TRequest : notnull
		where TResponse : class
	{
		Ensure.IsNotNull(request, nameof(request));
		var (httpReq, capturedRequestContent) = CreateHttpRequest(request);
		HttpResponseMessage? httpResp = null;

		try
		{
			httpResp = await _http.SendAsync(httpReq, cancellationToken)
				.ConfigureAwait(false);

			var (transformedResponse, capturedResponseContent) = await TransformResponse<TResponse>(
				httpReq.Method,
				httpReq.RequestUri,
				httpResp)
					.ConfigureAwait(false); ;

			if (_settings.CaptureRequestContent)
			{
				transformedResponse.RequestContent = capturedRequestContent;
			}

			if (_settings.CaptureResponseContent || !httpResp.IsSuccessStatusCode)
			{
				transformedResponse.ResponseContent = capturedResponseContent;
			}

			return transformedResponse;
		}
		catch (Exception ex)
		{
			var response = new GuestLineResponse<TResponse>(
				httpReq.Method,
				httpReq.RequestUri,
				false,
				(HttpStatusCode)0,
				error: new Error(ex.Message, exception: ex));

			return response;
		}
		finally
		{
			if (httpReq is not null)
			{
				httpReq.Dispose();
			}

			if (httpResp is not null)
			{
				httpResp.Dispose();
			}
		}
	}
	#endregion

	#region Preprocessing
	protected internal HttpRequestMessage CreateHttpRequest(
		GuestLineRequest request)
	{
		string pathAndQuery = request.Resource.ToUriComponent();
		if (request.Query.HasValue)
		{
			pathAndQuery += request.Query.Value.ToUriComponent();
		}

		var baseUri = request.Service switch
		{
			GuestLineService.Reservation => _baseUrl,

			_ => throw new NotSupportedException(
				string.Format(Resources.ApiClient_UnsupportedService, request.Service)
			)
		};
		var uri = new Uri(baseUri, pathAndQuery);

		var message = new HttpRequestMessage(request.Method, uri);

		message.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_settings.ApiKey}");

		return message;
	}

	protected internal (HttpRequestMessage, string?) CreateHttpRequest<TRequest>(
		GuestLineRequest<TRequest> request)
		where TRequest : notnull
	{
		var message = CreateHttpRequest((GuestLineRequest)request);
		string? capturedContent = null;

		if (_settings.CaptureRequestContent)
		{
			capturedContent = JsonSerializer.Serialize(request.Data, _serializerOptions);
			message.Content = new StringContent(capturedContent, Encoding.UTF8, "application/json");
		}
		else
		{
			message.Content = JsonContent.Create(
				inputValue: request.Data, options: _serializerOptions);
		}

		return (message, capturedContent);
	}
	#endregion

	#region Postprocessing
	protected internal async Task<(GuestLineResponse, string?)> TransformResponse(
		HttpMethod method,
		Uri uri,
		HttpResponseMessage response,
		CancellationToken cancellationToken = default)
	{
		var rateLimiting = GetRateLimiting(response);

		if (response.IsSuccessStatusCode)
		{
			return (new GuestLineResponse(
				method,
				uri,
				response.IsSuccessStatusCode,
				response.StatusCode,
				rateLimiting: rateLimiting), null);
		}
		else
		{
			Error error;
			string? stringContent = await response.Content.ReadAsStringAsync()
					.ConfigureAwait(false);
			if (stringContent is { Length: >0 })
			{
				var result = JsonSerializer.Deserialize<ErrorContainer>(stringContent, _deserializerOptions);

				if (result?.Error is not { Length: > 0 })
				{
					error = new(Resources.ApiClient_UnknownResponse, result?.ToErrorDictionary());
				}
				else
				{
					error = new(BuildErrorMessage(result!), result?.ToErrorDictionary());
				}
			}
			else
			{
				error = new Error(Resources.ApiClient_NoErrorMessage);
			}

			return (new GuestLineResponse(
				method,
				uri,
				response.IsSuccessStatusCode,
				response.StatusCode,
				rateLimiting: rateLimiting,
				error: error
			), stringContent);
		}
	}

	protected internal async Task<(GuestLineResponse<TResponse>, string?)> TransformResponse<TResponse>(
		HttpMethod method,
		Uri uri,
		HttpResponseMessage response,
		CancellationToken cancellationToken = default)
		where TResponse : class
	{
		var rateLimiting = GetRateLimiting(response);

		Stream? content = null;
		string? stringContent = null;
		if (response.Content is not null)
		{
			if (_settings.CaptureResponseContent || !response.IsSuccessStatusCode)
			{
				stringContent = await response.Content.ReadAsStringAsync()
					.ConfigureAwait(false);
			}
			else
			{
				content = await response.Content.ReadAsStreamAsync()
				.ConfigureAwait(false);
			}
		}

		if (response.IsSuccessStatusCode)
		{
			TResponse? data = default;
			Meta? meta = default;
			if (content is not null || stringContent is { Length: >0 })
			{
				try
				{
					data = stringContent is { Length: > 0 }
						? JsonSerializer.Deserialize<TResponse>(stringContent, _deserializerOptions)
						: await JsonSerializer.DeserializeAsync<TResponse>(content!, _deserializerOptions).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					return new(
						new GuestLineResponse<TResponse>(
							method,
							uri,
							response.IsSuccessStatusCode,
							response.StatusCode,
							rateLimiting: rateLimiting,
							error: new Error(ex.Message, exception: ex)
						), stringContent);
				}
			}

			return (new GuestLineResponse<TResponse>(
				method,
				uri,
				response.IsSuccessStatusCode,
				response.StatusCode,
				data: data,
				meta: meta,
				rateLimiting: rateLimiting
			), stringContent);
		}
		else
		{
			Error error;
			if (stringContent is { Length: >0 })
			{
				try
				{
					var result = JsonSerializer.Deserialize<ErrorContainer>(stringContent, _deserializerOptions);

					if (result?.Error is not { Length: > 0 })
					{
						error = new(Resources.ApiClient_UnknownResponse, result?.ToErrorDictionary());
					}
					else
					{
						error = new(BuildErrorMessage(result!), result?.ToErrorDictionary());
					}
				}
				catch (Exception ex)
				{
					error = new Error(ex.Message, exception: ex);
				}
			}
			else
			{
				error = new Error(Resources.ApiClient_NoErrorMessage);
			}

			return (new GuestLineResponse<TResponse>(
				method,
				uri,
				response.IsSuccessStatusCode,
				response.StatusCode,
				rateLimiting: rateLimiting,
				error: error
			), stringContent);
		}
	}

	RateLimiting? GetRateLimiting(HttpResponseMessage response)
	{
		var headers = response.Headers;

		return int.TryParse(GetHeader("X-Ratelimit-Remaining", headers), out int remaining)
			&& int.TryParse(GetHeader("X-Ratelimit-Limit", headers), out int limit)
			? new RateLimiting { Limit = limit, Remaining = remaining } : null;
	}

	string BuildErrorMessage(ErrorContainer container)
	{
		var builder = new StringBuilder();
		if (container.ErrorCode is { Length: > 0 })
		{
			builder.Append(container.ErrorCode);
			builder.Append(": ");
		}

		if (container.Error is { Length: > 0 })
		{
			builder.Append(container.Error);
			builder.Append("; ");
		}

		if (container.ErrorDetails is { Length: > 0 })
		{
			builder.Append(string.Join("; ", container.ErrorDetails.Select(ed => ed.Message).Where(m => m is { Length: > 0 })));
		}

		return builder.ToString().TrimEnd(' ', ';', ':') is { Length: > 0 } msg
			? msg
			: Resources.ApiClient_UnknownResponse;
	}

	string? GetHeader(string name, HttpHeaders headers)
		=> headers.TryGetValues(name, out var values)
		? values.First()
		: null;

	class ErrorContainer
	{
		[JsonPropertyName("error")]
		public string? Error { get; set; }

		[JsonPropertyName("errorCode")]
		public string? ErrorCode { get; set; }

		[JsonPropertyName("errorDetails")]
		public ErrorDetails[]? ErrorDetails { get; set; }

		[JsonPropertyName("bookingId")]
		public int? BookingId { get; set; }

		public Dictionary<string, string[]>? ToErrorDictionary()
		{
			if (ErrorDetails is null)
			{
				return null;
			}

			var dict = new Dictionary<string, string[]>();
			dict.Add(ErrorCode ?? "error", ErrorDetails.Select(ed => ed.Message ?? string.Empty).ToArray());

			return dict;
		}
	}

	public class ErrorDetails
	{
		[JsonPropertyName("message")]
		public string? Message { get; set; }
	}
	#endregion

	protected internal Lazy<TOperations> Defer<TOperations>(Func<ApiClient, TOperations> factory)
		=> new Lazy<TOperations>(() => factory(this));

	protected internal Uri Root(string resource)
		=> new Uri(resource, UriKind.Relative);
}
