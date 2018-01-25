﻿using Husky.AspNetCore.AliyunSms;
using Microsoft.Extensions.DependencyInjection;

namespace Husky.AspNetCore
{
	public static class DI
	{
		public static HuskyDependencyInjectionHub AddMail(this HuskyDependencyInjectionHub husky) {
			husky.ServiceCollection.AddSingleton<AliyunSmsSender>();
			return husky;
		}
	}
}
