// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using GuestLineSDK.Primitives;

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
	readonly Uri _serviceBaseUrl;
	readonly Uri _bookBaseUrl;

	protected ApiClient(HttpClient http, GuestLineSettings settings)
	{
		_http = Ensure.IsNotNull(http, nameof(http));
		_settings = Ensure.IsNotNull(settings, nameof(settings));

		_serviceBaseUrl = new Uri(GuestLineUriResolver.ResolveServiceBaseUrl(settings));
		_bookBaseUrl = new Uri(GuestLineUriResolver.ResolveBookBaseUrl(settings));
	}

	/// <summary>
	/// Gets the book base URL.
	/// </summary>
	public Uri BookBaseUrl => _bookBaseUrl;

	/// <summary>
	/// Gets the service base URL.
	/// </summary>
	public Uri ServiceBaseUrl => _serviceBaseUrl;

	/// <summary>
	/// Gets the settings.
	/// </summary>
	public GuestLineSettings Settings => _settings;

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
			ex = ex.Demystify();

			var response = new GuestLineResponse(
				httpReq.Method,
				httpReq.RequestUri,
				false,
				(HttpStatusCode)0,
				error: new ErrorResponse
				{
					Error = ex.Message,
					Exception = ex,
					ErrorSource = ErrorSource.SDK
				});

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
			ex = ex.Demystify();

			var response = new GuestLineResponse(
				httpReq.Method,
				httpReq.RequestUri,
				false,
				(HttpStatusCode)0,
				error: new ErrorResponse
				{
					Error = ex.Message,
					Exception = ex,
					ErrorSource = ErrorSource.SDK
				});

			return response;
		}
	}

	protected internal async Task<GuestLineResponse<TResponse>> FetchAsync<TResponse>(
		GuestLineRequest request,
		CancellationToken cancellationToken = default)
		where TResponse : Result<TResponse>
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
			ex = ex.Demystify();

			var response = new GuestLineResponse<TResponse>(
				httpReq.Method,
				httpReq.RequestUri,
				false,
				(HttpStatusCode)0,
				error: new ErrorResponse
				{
					Error = ex.Message,
					Exception = ex,
					ErrorSource = ErrorSource.SDK
				});

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
		where TResponse : Result<TResponse>
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
			ex = ex.Demystify();

			var response = new GuestLineResponse<TResponse>(
				httpReq.Method,
				httpReq.RequestUri,
				false,
				(HttpStatusCode)0,
				error: new ErrorResponse
				{
					Error = ex.Message,
					Exception = ex,
					ErrorSource = ErrorSource.SDK
				});

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
			GuestLineService.Reservation => _bookBaseUrl,
			GuestLineService.ARI => _serviceBaseUrl,

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
		if (response.IsSuccessStatusCode)
		{
			return (new GuestLineResponse(
				method,
				uri,
				response.IsSuccessStatusCode,
				response.StatusCode), null);
		}
		else
		{
			ErrorResponse? error;
			string? stringContent = await response.Content.ReadAsStringAsync()
					.ConfigureAwait(false);
			if (stringContent is { Length: > 0 })
			{
				var result = JsonSerializer.Deserialize<ErrorResponse>(stringContent, _deserializerOptions);

				if (result?.Error is not { Length: > 0 })
				{
					error = new ErrorResponse
					{
						Error = Resources.ApiClient_UnknownResponse,
						Status = result?.Status,
						TrackingId = result?.TrackingId
					};
				}
				else
				{
					error = result;
				}
			}
			else
			{
				error = new ErrorResponse
				{
					Error = Resources.ApiClient_NoErrorMessage
				};
			}

			return (new GuestLineResponse(
				method,
				uri,
				response.IsSuccessStatusCode,
				response.StatusCode,
				error: error
			), stringContent);
		}
	}

	protected internal async Task<(GuestLineResponse<TResponse>, string?)> TransformResponse<TResponse>(
		HttpMethod method,
		Uri uri,
		HttpResponseMessage response,
		CancellationToken cancellationToken = default)
		where TResponse : Result<TResponse>
	{
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
			if (content is not null || stringContent is { Length: > 0 })
			{
				try
				{
					data = stringContent is { Length: > 0 }
						? JsonSerializer.Deserialize<TResponse>(stringContent, _deserializerOptions)
						: await JsonSerializer.DeserializeAsync<TResponse>(content!, _deserializerOptions).ConfigureAwait(false);

					return (new GuestLineResponse<TResponse>(
						method,
						uri,
						response.IsSuccessStatusCode,
						response.StatusCode,
						data: data), stringContent);
				}
				catch (Exception ex)
				{
					ex = ex.Demystify();

					return new(
						new GuestLineResponse<TResponse>(
							method,
							uri,
							response.IsSuccessStatusCode,
							response.StatusCode,
							error: new ErrorResponse
							{
								Error = ex.Message,
								Exception = ex,
								ErrorSource = ErrorSource.SDK
							}
						), stringContent);
				}
			}

			return (new GuestLineResponse<TResponse>(
				method,
				uri,
				false,
				response.StatusCode,
				error: new ErrorResponse
				{
					Error = Resources.ApiClient_UnknownResponse
				}
			), stringContent);
		}
		else
		{
			ErrorResponse? error;
			if (stringContent is { Length: > 0 })
			{
				var result = JsonSerializer.Deserialize<ErrorResponse>(stringContent, _deserializerOptions);

				if (result?.Error is not { Length: > 0 })
				{
					error = new ErrorResponse
					{
						Error = Resources.ApiClient_UnknownResponse,
						Status = result?.Status,
						TrackingId = result?.TrackingId
					};
				}
				else
				{
					error = result;
				}
			}
			else
			{
				error = new ErrorResponse
				{
					Error = Resources.ApiClient_NoErrorMessage
				};
			}

			return (new GuestLineResponse<TResponse>(
				method,
				uri,
				false,
				response.StatusCode,
				error: error
			), stringContent);
		}
	}
	#endregion

	protected internal Lazy<TOperations> Defer<TOperations>(Func<ApiClient, TOperations> factory)
		=> new Lazy<TOperations>(() => factory(this));

	protected internal Uri Root(string resource)
		=> new Uri(resource, UriKind.Relative);
}
