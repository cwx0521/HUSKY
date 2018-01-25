﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Husky.AspNetCore
{
	public static class ConnectionStrings
	{
		public static string GetConnectionStringBySeekSequence<T>(this IConfiguration configuration) where T : DbContext {
			var lookFor = new[] { "Dev", "QA", typeof(T).Name.Replace("DbContext", ""), "Husky", "Default" };
			var connstr = lookFor.Select(x => configuration.GetConnectionString(x)).FirstOrDefault(x => !string.IsNullOrEmpty(x));
			if ( string.IsNullOrEmpty(connstr) ) {
				throw new Exception("Didn't find any applicable ConnectionString in appSettings.json configuration, these ConnectionString names are searched: " + string.Join(", ", lookFor));
			}
			return connstr;
		}

		public static string LocalConnectionStringWithIntegratedSecurity(string databaseName) {
			if ( string.IsNullOrWhiteSpace(databaseName) ) {
				throw new ArgumentNullException(nameof(databaseName));
			}
			return $"Data Source=localhost;Initial Catalog={databaseName};Integrated Security=True";
		}

		public static string LocalConnectionStringWithIntegratedSecurity<T>() where T : DbContext {
			return LocalConnectionStringWithIntegratedSecurity(nameof(T));
		}
	}
}
