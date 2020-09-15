﻿using System;

namespace Husky
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class DefaultValueSqlAttribute : Attribute
	{
		public DefaultValueSqlAttribute(string sql) {
			Sql = sql;
		}

		public string Sql { get; set; } = null!;
	}
}
