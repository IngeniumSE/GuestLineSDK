// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

namespace GuestLineSDK;

class QueryStringBuilder
{
	QueryString _qs = QueryString.Empty;

	public QueryStringBuilder AddParameter(string name, object? value)
	{
		Ensure.IsNotNullOrEmpty(name, nameof(name));

		if (value is not null)
		{
#pragma warning disable CS8604 // Possible null reference argument.
			_qs += QueryString.Create(name, value?.ToString());
#pragma warning restore CS8604 // Possible null reference argument.
		}

		return this;
	}

	public QueryString Build() => _qs;
}
