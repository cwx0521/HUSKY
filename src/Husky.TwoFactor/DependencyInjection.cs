﻿using Husky.TwoFactor;
using Husky.TwoFactor.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Husky
{
	public static class DependencyInjection
	{
		public static HuskyDI AddDiagnostics(this HuskyDI husky, string nameOfConnectionString = null, bool migrateRequiredDatabase = true) {
			husky.Services
				.AddDbContextPool<TwoFactorDbContext>(nameOfConnectionString, migrateRequiredDatabase)
				.AddScoped<TwoFactorManager>();

			return husky;
		}
	}
}
