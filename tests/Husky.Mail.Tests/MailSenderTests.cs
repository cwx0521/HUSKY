﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Husky.Mail.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Husky.Mail.Tests
{
	[TestClass()]
	public class MailSenderTests
	{
		[TestMethod()]
		public void SendAsyncTest() {
			Crypto.PermanentToken = Crypto.RandomString();

			var _smtp = new MailSmtpProvider {
				Id = Guid.NewGuid(),
				Host = "smtp.live.com",
				Port = 25,
				Ssl = false,
				SenderDisplayName = "Weixing Chen",
				SenderMailAddress = "chenwx521@hotmail.com",
				CredentialName = "",
				Password = "",
				IsInUse = true
			};

			if ( string.IsNullOrEmpty(_smtp.CredentialName) || string.IsNullOrEmpty(_smtp.Password) ) {
				return;
			}

			var dbName = $"UnitTest_{nameof(MailSenderTests)}_{nameof(SendAsyncTest)}";
			var dbBuilder = new DbContextOptionsBuilder<MailDbContext>();
			dbBuilder.UseSqlServer($"Data Source=(localdb)\\MSSQLLocalDB; Initial Catalog={dbName}; Integrated Security=True");

			using var db = new MailDbContext(dbBuilder.Options);
			db.Database.EnsureDeleted();
			db.Database.Migrate();

			//Config CredentialName&Password before running this test

			db.Add(_smtp);
			db.SaveChanges();

			var sender = new MailSender(db);
			var mail = new MailMessage {
				Subject = "Husky.Mail Unit Test",
				Body = "<div style='color:blue'>Greeting</div>",
				IsHtml = true,
				To = new List<MailAddress> {
					new MailAddress { Name = "Weixing", Address = "chenwx521@hotmail.com" }
				},
				Cc = new List<MailAddress> {
					new MailAddress { Name = "Weixing", Address = "5607882@qq.com" }
				},
				Attachments = new List<MailAttachment> {
					new MailAttachment {
						Name = "DummyAttachment.zip",
						ContentType = "application/x-zip-compressed",
						ContentStream = new MemoryStream(Crypto.RandomBytes())
					}
				}
			};

			var i = 0;
			string str = null;
			sender.SendAsync(mail, (arg) => { i++; str = arg.MailMessage.Body; }).Wait();

			var mailRecord = db.MailRecords
				.AsNoTracking()
				.Include(x => x.Smtp)
				.Include(x => x.Attachments)
				.OrderBy(x => x.Id)
				.LastOrDefault();

			Assert.IsNotNull(mailRecord);
			Assert.AreEqual(mailRecord.Subject, mail.Subject);
			Assert.AreEqual(mailRecord.Body, mail.Body);
			Assert.AreEqual(mailRecord.IsHtml, mail.IsHtml);
			Assert.AreEqual(mailRecord.Smtp.Id, _smtp.Id);
			Assert.AreEqual(mailRecord.To, string.Join(";", mail.To.Select(x => x.ToString())));
			Assert.AreEqual(mailRecord.Cc, string.Join(";", mail.Cc.Select(x => x.ToString())));
			Assert.AreEqual(mailRecord.Attachments.Count, mail.Attachments.Count);
			Assert.AreEqual(mailRecord.Attachments.First().Name, mail.Attachments.First().Name);

			Assert.AreEqual(i, 1);
			Assert.AreEqual(str, mail.Body);
			Assert.AreEqual(mailRecord.IsSuccessful, true);
			Assert.IsTrue(string.IsNullOrEmpty(mailRecord.Exception));

			db.Database.EnsureDeleted();
		}
	}
}