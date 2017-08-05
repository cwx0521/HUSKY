﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Husky.Mail.Abstractions;
using Husky.Mail.Data;
using Husky.Sugar;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using MimeKit.Text;

namespace Husky.Mail
{
	public class MailSender : IMailSender
	{
		public MailSender(IServiceProvider serviceProvider) {
			_db = serviceProvider.GetRequiredService<MailDbContext>();
			_givenSmtp = serviceProvider.GetService<ISmtpProvider>();
		}

		readonly MailDbContext _db;
		readonly ISmtpProvider _givenSmtp;

		public MailDbContext DbContext => _db;

		public async Task Send(MailMessage mailMessage) => await Send(mailMessage, null);

		public async Task Send(string recipient, string subject, string content) {
			if ( string.IsNullOrEmpty(recipient) ) {
				throw new ArgumentNullException(nameof(recipient));
			}
			await Send(new MailMessage {
				Subject = subject,
				Body = content,
				IsHtml = true,
				To = new List<MailAddress> { new MailAddress { Address = recipient } }
			});
		}

		public async Task Send(MailMessage mailMessage, Action<MailSentEventArgs> onCompleted) {
			if ( mailMessage == null ) {
				throw new ArgumentNullException(nameof(mailMessage));
			}

			var smtp = _givenSmtp ?? GetInternalSmtpProvider();
			var mailRecord = CreateMailRecord(mailMessage);

			if ( smtp is MailSmtpProvider internalSmtp ) {
				mailRecord.SmtpId = internalSmtp.Id;
			}

			_db.Add(mailRecord);
			await _db.SaveChangesAsync();

			using ( var client = new SmtpClient() ) {
				client.MessageSent += async (object sender, MessageSentEventArgs e) => {
					mailRecord.IsSuccessful = true;
					await _db.SaveChangesAsync();
					await Task.Run(() => {
						onCompleted?.Invoke(new MailSentEventArgs { MailMessage = mailMessage });
					});
				};
				try {
					await client.ConnectAsync(smtp.Host, smtp.Port, smtp.Ssl);
					await client.AuthenticateAsync(smtp.CredentialName, smtp.Password);
					await client.SendAsync(BuildMimeMessage(smtp, mailMessage));
				}
				catch ( Exception ex ) {
					mailRecord.Exception = ex.Message.Left(200);
					await _db.SaveChangesAsync();
				}
			}
		}

		private MailSmtpProvider GetInternalSmtpProvider() {
			var haveCount = _db.MailSmtpProviders.Count(x => x.IsInUse);
			if ( haveCount == 0 ) {
				throw new Exception("（邮件发送模块）还没有配置任何SMTP服务。".Xslate());
			}
			var skip = new Random().Next(0, haveCount);
			return _db.MailSmtpProviders.Skip(skip).First();
		}

		private MailRecord CreateMailRecord(MailMessage mailMessage) {
			return new MailRecord {
				Subject = mailMessage.Subject,
				Body = mailMessage.Body,
				IsHtml = mailMessage.IsHtml,

				To = string.Join(";", mailMessage.To.Select(x => x.ToString())),
				Cc = string.Join(";", mailMessage.Cc.Select(x => x.ToString())),

				Attachments = mailMessage.Attachments.Select(a => new MailRecordAttachment {
					Name = a.Name,
					ContentStream = ReadStream(a.ContentStream),
					ContentType = a.ContentType
				}).ToList()
			};
		}

		private MimeMessage BuildMimeMessage(ISmtpProvider smtp, MailMessage mailMessage) {
			var mail = new MimeMessage();

			// Subject
			mail.Subject = mailMessage.Subject;
			// From
			mail.From.Add(new MailboxAddress(smtp.SenderDisplayName, smtp.SenderMailAddress ?? smtp.CredentialName));
			// To
			mailMessage.To.ForEach(to => mail.To.Add(new MailboxAddress(to.Name, to.Address)));
			// Cc
			mailMessage.Cc.ForEach(cc => mail.Cc.Add(new MailboxAddress(cc.Name, cc.Address)));

			// Body
			var body = new TextPart(mailMessage.IsHtml ? TextFormat.Html : TextFormat.Text) {
				Text = mailMessage.Body
			};
			if ( mailMessage.Attachments?.Count == 0 ) {
				mail.Body = body;
			}
			// Or: Body + Attachments
			else {
				var multipart = new Multipart("mixed");
				mailMessage.Attachments.ForEach(a => {
					multipart.Add(new MimePart(a.ContentType) {
						ContentObject = new ContentObject(a.ContentStream),
						ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
						ContentTransferEncoding = ContentEncoding.Base64,
						FileName = a.Name
					});
				});
				multipart.Add(body);
				mail.Body = multipart;
			}
			return mail;
		}

		private byte[] ReadStream(Stream stream) {
			var length = stream.Length;
			var bytes = new byte[length];
			stream.Read(bytes, 0, (int)length);
			return bytes;
		}
	}
}