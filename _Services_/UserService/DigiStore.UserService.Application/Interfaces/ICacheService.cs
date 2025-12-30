using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Application.Interfaces;


/// <summary>
/// Caching service using Redis
/// </summary>
public interface ICacheService
{
	/// <summary>
	/// Get cached value
	/// </summary>
	Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

	/// <summary>
	/// Set value in cache
	/// </summary>
	Task SetAsync<T>(
		string key,
		T value,
		TimeSpan? expiration = null,
		CancellationToken ct = default);

	/// <summary>
	/// Remove from cache
	/// </summary>
	Task RemoveAsync(string key, CancellationToken ct = default);

	/// <summary>
	/// Remove by pattern (wildcard)
	/// </summary>
	Task RemoveByPatternAsync(string pattern, CancellationToken ct = default);

	/// <summary>
	/// Check if key exists
	/// </summary>
	Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
