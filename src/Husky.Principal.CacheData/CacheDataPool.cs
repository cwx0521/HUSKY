﻿using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Husky.Principal
{
	internal class CacheDataPool<T> where T : class, ICacheDataBag
	{
		internal CacheDataPool(IMemoryCache cache) {
			_cache = cache;
		}

		private readonly IMemoryCache _cache;
		private readonly string _cacheKeyOfPool = "Pool_" + typeof(T).FullName;

		internal TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(30);

		internal T? Pick(string key) {
			if ( key == null ) {
				throw new ArgumentNullException(nameof(key));
			}
			var pool = GetPool();
			if ( pool != null ) {
				if ( pool.TryGetValue(key, out var bag) ) {
					if ( bag.ActiveTime.Add(Timeout) > DateTime.Now ) {
						bag.ActiveTime = DateTime.Now;
						return bag;
					}
					Drop(key);
				}
			}
			return default;
		}

		internal void Put(T bag) {
			DropTimeout(Timeout);
			bag.ActiveTime = DateTime.Now;
			EnsureGetPool().AddOrUpdate(bag.Key, bag, (key, _) => bag);
		}

		internal T PickOrCreate(string key, Func<string, T> createBag) {
			if ( key == null ) {
				throw new ArgumentNullException(nameof(key));
			}
			if ( createBag == null ) {
				throw new ArgumentNullException(nameof(createBag));
			}

			var pool = EnsureGetPool();

			if ( pool.TryGetValue(key, out var bag) && bag.ActiveTime.Add(Timeout) > DateTime.Now ) {
				bag.ActiveTime = DateTime.Now;
				return bag;
			}

			var created = createBag(key);
			created.ActiveTime = DateTime.Now;

			pool.AddOrUpdate(created.Key, created, (key, _) => created);
			return created;
		}

		internal void Drop(string key) {
			if ( key == null ) {
				throw new ArgumentNullException(nameof(key));
			}
			GetPool()?.TryRemove(key, out _);
		}

		internal void DropAll() => _cache.Remove(_cacheKeyOfPool);

		internal void DropTimeout(TimeSpan timeout) {
			var pool = GetPool();
			if ( pool != null ) {
				var keys = new string[pool.Count];
				pool.Keys.CopyTo(keys, 0);

				foreach ( var i in keys ) {
					if ( pool.ContainsKey(i) && pool[i].ActiveTime.Add(timeout) < DateTime.Now ) {
						pool.TryRemove(i, out _);
					}
				}
			}
		}

		private ConcurrentDictionary<string, T> GetPool() => _cache.Get<ConcurrentDictionary<string, T>>(_cacheKeyOfPool);

		private ConcurrentDictionary<string, T> EnsureGetPool() {
			return _cache.GetOrCreate(_cacheKeyOfPool, x => {
				x.SetSlidingExpiration(Timeout);
				return new ConcurrentDictionary<string, T>();
			});
		}
	}
}
