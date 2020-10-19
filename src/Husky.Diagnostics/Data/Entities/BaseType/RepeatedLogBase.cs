﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Husky.Diagnostics.Data
{
	public abstract class RepeatedLogBase : LogBase
	{
		public int Repeated { get; set; } = 1;

		[StringLength(32), Column(TypeName = "varchar(32)"), Unique]
		public string Md5Comparison { get; set; } = null!;


		public abstract void ComputeMd5Comparison();
	}
}