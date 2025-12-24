// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK;

/// <summary>
/// Represents a request to a Trybe API resource.
/// </summary>
/// <param name="service">The GuestLine service.</param>
/// <param name="method">The HTTP method.</param>
/// <param name="resource">The relative resource.</param>
/// <param name="query">The query string.</param>
public class GuestLineRequest(
	GuestLineService service,
	HttpMethod method,
	PathString resource,
	QueryString? query = null)
{
	readonly GuestLineService _service = service;
	readonly HttpMethod _method = method;
	readonly PathString _resource = resource;
	readonly QueryString? _query = query;

	/// <summary>
	/// Gets the HTTP method for the request.
	/// </summary>
	public HttpMethod Method => _method;

	/// <summary>
	/// Gets the relative resource for the request.
	/// </summary>
	public PathString Resource => _resource;

	/// <summary>
	/// Gets the query string.
	/// </summary>
	public QueryString? Query => _query;

	/// <summary>
	/// Gets the GuestLine service.
	/// </summary>
	public GuestLineService Service => _service;
}

/// <summary>
/// Represents a request to a Trybe API resource.
/// </summary>
/// <param name="service">The GuestLine service.</param>
/// <param name="method">The HTTP method.</param>
/// <param name="resource">The relative resource.</param>
/// <param name="data">The data.</param>
/// <param name="query">The query string.</param>
/// <typeparam name="TData">The data type.</typeparam>
public class GuestLineRequest<TData>(
	GuestLineService service,
	HttpMethod method,
	PathString resource,
	TData data,
	QueryString? query = null) : GuestLineRequest(service, method, resource, query)
	where TData : notnull
{
	readonly TData _data = data;

	/// <summary>
	/// Gets the model for the request.
	/// </summary>
	public TData Data => _data;
}

/// <summary>
/// Represents the possible GuestLine services.
/// </summary>
public enum GuestLineService
{
	/// <summary>
	/// The booking service.
	/// </summary>
	Book,

	/// <summary>
	/// The ARI service.
	/// </summary>
	ARI
}
