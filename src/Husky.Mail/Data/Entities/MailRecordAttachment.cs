﻿using System;
using System.ComponentModel.DataAnnotations;
using Husky.Data.ModelBuilding.Annotations;

namespace Husky.Mail.Data
{
	public partial class MailRecordAttachment
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid MailId { get; set; }

		[MaxLength(100)]
		public string Name { get; set; }

		public byte[] ContentStream { get; set; }

		[MaxLength(32)]
		public string ContentType { get; set; }

		[Index(IsClustered = true, IsUnique = false)]
		public DateTime CreatedTime { get; set; }


		public virtual MailRecord Mail { get; set; }
	}
}