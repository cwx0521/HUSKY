﻿using System;

namespace Husky.WeChatIntegration
{
	public class WeChatAppIdSecret
	{
		public string? AppId { get; set; }
		public string? AppSecret { get; set; }
		public WeChatField? Type { get; set; }

		internal void NotNull() {
			_ = AppId ?? throw new ArgumentNullException(nameof(AppId));
			_ = AppSecret ?? throw new ArgumentNullException(nameof(AppSecret));
		}
	}
}
