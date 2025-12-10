// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System.Diagnostics;
using System.Net;
using System.Text;

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
	Meta? meta = default,
	RateLimiting? rateLimiting = default,
	Error? error = default)
{
	/// <summary>
	/// Gets whether the status code represents a success HTTP status code.
	/// </summary>
	public bool IsSuccess => isSuccess;

	/// <summary>
	/// Gets the error.
	/// </summary>
	public Error? Error => error;

	/// <summary>
	/// Gets th metadata.
	/// </summary>
	public Meta? Meta => meta;

	/// <summary>
	/// Gets the HTTP status code of the response.
	/// </summary>
	public HttpStatusCode StatusCode => statusCode;

	/// <summary>
	/// Gets the rate limiting metdata for the request.
	/// </summary>
	public RateLimiting? RateLimiting => rateLimiting;

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
	/// Provides a string representation for debugging.
	/// </summary>
	/// <returns></returns>
	public virtual string ToDebuggerString()
	{
		var builder = new StringBuilder();
		builder.Append($"{StatusCode}: {RequestMethod} {RequestUri.PathAndQuery}");
		if (Error is not null)
		{
			builder.Append($" - {Error.Message}");
		}

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
	Meta? meta = default,
	RateLimiting? rateLimiting = default,
	Error? error = default) : GuestLineResponse(method, uri, isSuccess, statusCode, meta, rateLimiting, error)
	where TData : class
{
	/// <summary>
	/// Gets the response data.
	/// </summary>
	public TData? Data => data;

	/// <summary>
	/// Gets whether the response has data.
	/// </summary>
	public bool HasData => data is not null;

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
			builder.Append($" - {Error.Message}");
		}

		return builder.ToString();
	}
}

/// <summary>
/// Represents a Trybe error response.
/// </summary>
/// <param name="message">The error message.</param>
/// <param name="errors">The set of additional error messages, these may be field specific.</param>
/// <param name="exception">The exception that was caught.</param>
public class Error(string message, Dictionary<string, string[]>? errors = null, Exception? exception = null)
{
	/// <summary>
	/// Gets the set of additional error messages, these may be field specific.
	/// </summary>
	public Dictionary<string, string[]>? Errors => errors;

	/// <summary>
	/// Gets the exception that was caught.
	/// </summary>
	public Exception? Exception => exception;

	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string Message => message;
}

/// <summary>
/// Represents metadata for a Trybe request.
/// </summary>
public class Meta
{
	/// <summary>
	/// Gets the current page.
	/// </summary>
	public int Page { get; set; }

	/// <summary>
	/// Gets the page size.
	/// </summary>
	public int PageSize { get; set; }

	/// <summary>
	/// Gets or sets the total number of items.
	/// </summary>
	public int TotalItems { get; set; }

	/// <summary>
	/// Gets or sets the total number of pages.
	/// </summary>
	public int TotalPages { get; set; }
}

/// <summary>
/// Represents rate-limiting information for a Trybe request.
/// </summary>
public class RateLimiting
{
	/// <summary>
	/// Gets the remaining number of calls available.
	/// </summary>
	public int Remaining { get; set; }

	/// <summary>
	/// Gets the limit of calls available.
	/// </summary>
	public int Limit { get; set; }
}
