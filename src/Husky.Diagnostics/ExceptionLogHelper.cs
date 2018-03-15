﻿using System;
using System.Threading.Tasks;
using Husky.Diagnostics.Data;
using Husky.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Husky.Diagnostics
{
	public static class ExceptionLogHelper
	{
		public static void Log(this IServiceProvider serviceProvider, Exception e) {
			serviceProvider.LogAsync(e).Wait();
		}

		public static void Log(this Exception e, IServiceProvider serviceProvider) {
			serviceProvider.LogAsync(e).Wait();
		}

		public static async Task LogAsync(this Exception e, IServiceProvider serviceProvider) {
			await serviceProvider.LogAsync(e);
		}

		public static async Task LogAsync(this IServiceProvider serviceProvider, Exception e) {
			try {
				var db = serviceProvider.GetRequiredService<DiagnosticsDbContext>();
				var principal = serviceProvider.GetService<IPrincipalUser>();
				var request = serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.Request;

				var log = new ExceptionLog {
					HttpMethod = request?.Method,
					ExceptionType = e.GetType().FullName,
					Message = e.Message,
					Source = e.Source,
					StackTrace = e.StackTrace,
					Url = request?.GetDisplayUrl(),
					UserName = principal?.DisplayName,
					UserAgent = request?.UserAgent()
				};
				log.ComputeMd5Comparison();

				db.Add(log);
				await db.SaveChangesAsync();
			}
			catch {
			}
		}
	}
}
