﻿using System;

namespace Husky.Principal.Administration
{
	public interface IPrincipalAdmin
	{
		IPrincipalUser Principal { get; }

		bool IsAdmin { get; }
		bool IsNotAdmin { get; }

		Guid Id { get; }
		string DisplayName { get; }

		string[] Roles { get; }
		long PowerFlags { get; }

		TFlagsEnum Powers<TFlagsEnum>() where TFlagsEnum : Enum;
		bool Allow<TFlagsEnum>(TFlagsEnum power) where TFlagsEnum : Enum;
	}
}
