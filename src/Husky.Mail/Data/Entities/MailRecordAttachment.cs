﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Husky.Mail.Data
{
	public partial class MailRecordAttachment
	{
		[Key]
		public int Id { get; set; }

		public int MailId { get; set; }

		[MaxLength(100)]
		public string Name { get; set; } = null!;

		public byte[] ContentStream { get; set; } = null!;

		[MaxLength(32)]
		public string ContentType { get; set; } = null!;

		public DateTime CreatedTime { get; set; } = DateTime.Now;


		// nav props

		public MailRecord Mail { get; set; } = null!;
	}
}
