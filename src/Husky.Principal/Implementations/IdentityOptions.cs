﻿using System;

namespace Husky.Principal.Implementations
{
	public sealed class IdentityOptions
	{
		public IdentityCarrier Carrier { get; set; } = IdentityCarrier.Cookie;
		public string IdKey { get; set; } = "HUSKY_AUTH_IDENTITY";
		public string AnonymousIdKey { get; set; } = "HUSKY_AUTH_ANONYMOUS";
		public string Token { get; set; } = Crypto.SecretToken;
		public bool SessionMode { get; set; } = true;
		public bool DedicateAnonymousIdStorage { get; set; } = true;
		public DateTimeOffset? Expires { get; set; }
		public IIdentityEncyptor Encryptor { get; set; } = new IdentityEncryptor();
	}
}