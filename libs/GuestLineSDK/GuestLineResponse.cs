// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System.Diagnostics;
using System.Net;
using System.Text;

using GuestLineSDK.Primitives;

namespace GuestLineSDK;

/// <summary>
/// Represents a Trybe response.
/// </summary>
/// <param name="method">The HTTP method requested.</param>
/// <param name="uri">The URI requested.</param>
/// <param name="isSuccess">States whether the status code is a success HTTP status code.</param>
/// <param name="statusCode">The HTTP status code returned.</param>
/// <param name="meta">The paging metadata for the request, if available.</param>
/// <param name="rateLimiting">The rate limiting metadata for the request, if available.</param>
/// <param name="error">The API error, if available.</param>
[DebuggerDisplay("{ToDebuggerString(),nq}")]
public class GuestLineResponse(
	HttpMethod method,
	Uri uri,
	bool isSuccess,
	HttpStatusCode statusCode,
	ErrorResponse? error = default)
{
	/// <summary>
	/// Gets whether the status code represents a success HTTP status code.
	/// </summary>
	public bool IsSuccess => isSuccess;

	/// <summary>
	/// Gets the error.
	/// </summary>
	public ErrorResponse? Error => error;

	/// <summary>
	/// Gets the HTTP status code of the response.
	/// </summary>
	public HttpStatusCode StatusCode => statusCode;

	/// <summary>
	/// Gets or sets the request HTTP method.
	/// </summary>
	public HttpMethod RequestMethod => method;

	/// <summary>
	/// Gets or sets the request URI.
	/// </summary>
	public Uri RequestUri => uri;

	/// <summary>
	/// Gets or sets the request content, when logging is enabled.
	/// </summary>
	public string? RequestContent { get; set; }

	/// <summary>
	/// Gets or sets the response content, when logging is enabled.
	/// </summary>
	public string? ResponseContent { get; set; }

	/// <summary>
	/// Gets the unique identifier associated with the response for tracking purposes.
	/// </summary>
	public virtual string? TrackingId => Error?.TrackingId;

	/// <summary>
	/// Provides a string representation for debugging.
	/// </summary>
	/// <returns></returns>
	public virtual string ToDebuggerString()
	{
		var builder = new StringBuilder();
		builder.Append($"{StatusCode}: {RequestMethod} {RequestUri.PathAndQuery}");
		if (Error is not null)
		{
			builder.Append($" - {Error.Error}");
		}

		string trackingId = TrackingId ?? "(untrackable)";
		builder.Append($" [Tracking ID: {trackingId}]");

		return builder.ToString();
	}
}

/// <summary>
/// Represents a Trybe response with payload data.
/// </summary>
/// <param name="method">The HTTP method requested.</param>
/// <param name="uri">The URI requested.</param>
/// <param name="isSuccess">States whether the status code is a success HTTP status code.</param>
/// <param name="statusCode">The HTTP status code.</param>
/// <param name="data">The API response data, if available.</param>
/// <param name="meta">The paging metadata for the request, if available.</param>
/// <param name="rateLimiting">The rate limiting metadata for the request, if available.</param>
/// <param name="error">The API error, if available.</param>
/// <typeparam name="TData">The data type.</typeparam>
public class GuestLineResponse<TData>(
	HttpMethod method,
	Uri uri,
	bool isSuccess,
	HttpStatusCode statusCode,
	TData? data = default,
	ErrorResponse? error = default) : GuestLineResponse(method, uri, isSuccess, statusCode, error)
	where TData : Result<TData>
{
	/// <summary>
	/// Gets the response data.
	/// </summary>
	public TData? Data => data;

	/// <summary>
	/// Gets whether the response has data.
	/// </summary>
	public bool HasData => data is not null;

	/// <summary>
	/// Gets the tracking identifier associated with the current operation or request.
	/// </summary>
	public override string? TrackingId => Data?.TrackingId ?? base.TrackingId;

	public override string ToDebuggerString()
	{
		var builder = new StringBuilder();
		builder.Append($"{StatusCode}");
		if (HasData)
		{
			builder.Append($" ({Data!.GetType().Name})");
		}
		builder.Append($": {RequestMethod} {RequestUri.PathAndQuery}");
		if (Error is not null)
		{
			builder.Append($" - {Error.Error}");
		}

		string trackingId = TrackingId ?? "(untrackable)";
		builder.Append($" [Tracking ID: {trackingId}]");

		return builder.ToString();
	}
}
