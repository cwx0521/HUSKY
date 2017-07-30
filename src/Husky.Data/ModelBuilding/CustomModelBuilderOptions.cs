﻿namespace Husky.Data.ModelBuilding
{
	public sealed class CustomModelBuilderOptions
	{
		public bool AnnotatedIndices { get; set; } = true;
		public bool AnnotatedDefaultValueSql { get; set; } = true;
		public bool AutoDetectedDefaultValueSql { get; set; } = true;
	}
}
