﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Husky.Mail;
using Husky.Principal;
using Husky.Sms;
using Husky.TwoFactor.Data;
using Microsoft.EntityFrameworkCore;

namespace Husky.TwoFactor
{
	public sealed partial class TwoFactorManager : ITwoFactorManager
	{
		public TwoFactorManager(ITwoFactorDbContext twoFactorDb, IPrincipalUser principal, ISmsSender? smsSender, IMailSender? mailSender) {
			_twoFactorDb = twoFactorDb;
			_me = principal;
			_smsSender = smsSender;
			_mailSender = mailSender;
		}

		private readonly ITwoFactorDbContext _twoFactorDb;
		private readonly IPrincipalUser _me;
		private readonly ISmsSender? _smsSender;
		private readonly IMailSender? _mailSender;

		public async Task<Result> SendCode(string mobileNumberOrEmailAddress, string? overrideMessageTemplateWithCodeArg0 = null, string? overrideSmsTemplateAlias = null, string? overrideSmsSignName = null) {
			if ( mobileNumberOrEmailAddress == null ) {
				throw new ArgumentNullException(nameof(mobileNumberOrEmailAddress));
			}

			var isEmail = mobileNumberOrEmailAddress.IsEmail();
			var isMobile = mobileNumberOrEmailAddress.IsMainlandMobile();

			if ( !isEmail && !isMobile ) {
				return new Failure($"无法发送到 '{mobileNumberOrEmailAddress}' ");
			}

			if ( isMobile ) {
				var sentWithinMinute = _twoFactorDb.TwoFactorCodes
					.AsNoTracking()
					.Where(x => x.SentTo == mobileNumberOrEmailAddress)
					.Where(x => x.CreatedTime > DateTime.Now.AddMinutes(-1))
					.Any();

				if ( sentWithinMinute ) {
					return new Failure("请求过于频繁");
				}
			}

			var code = new TwoFactorCode {
				UserId = _me.Id,
				Code = new Random().Next(0, 1000000).ToString("D6"),
				SentTo = mobileNumberOrEmailAddress
			};
			_twoFactorDb.TwoFactorCodes.Add(code);
			await _twoFactorDb.Normalize().SaveChangesAsync();

			if ( isEmail ) {
				if ( _mailSender == null ) {
					throw new Exception($"Required to inject service {typeof(MailSender).Assembly.GetName()}");
				}
				var content = string.Format(overrideMessageTemplateWithCodeArg0 ?? "验证码：{0}", code.Code);
				await _mailSender.SendAsync("动态验证码", content, mobileNumberOrEmailAddress);
			}
			else if ( isMobile ) {
				if ( _smsSender == null ) {
					throw new Exception($"Required to inject service {typeof(ISmsSender).Assembly.GetName()}");
				}
				var shortMessage = new SmsBody {
					SignName = overrideSmsSignName,
					Template = overrideMessageTemplateWithCodeArg0,
					TemplateAlias = overrideSmsTemplateAlias,
					Parameters = new Dictionary<string, string> {
						{ "code", code.Code }
					}
				};
				await _smsSender.SendAsync(shortMessage, mobileNumberOrEmailAddress);
			}

			return new Success();
		}

		public async Task<Result> SendCodeThroughSms(string mobileNumber, string? overrideMessageTemplateWithCodeArg0 = null, string? overrideSmsTemplateAlias = null, string? overrideSmsSignName = null) {
			if ( !mobileNumber.IsMainlandMobile() ) {
				return new Failure($"无法发送到 '{mobileNumber}'");
			}
			return await SendCode(mobileNumber, overrideMessageTemplateWithCodeArg0, overrideSmsTemplateAlias, overrideSmsSignName);
		}

		public async Task<Result> SendCodeThroughEmail(string emailAddress, string? messageTemplateWithCodeArg0 = null) {
			if ( !emailAddress.IsEmail() ) {
				return new Failure($"无法发送到 '{emailAddress}'");
			}
			return await SendCode(emailAddress, messageTemplateWithCodeArg0, null, null);
		}

		public async Task<Result> VerifyCode(string sentTo, string code, bool setIntoUsedAfterVerifying, int withinMinutes = 15) {
			if ( sentTo == null ) {
				throw new ArgumentNullException(nameof(sentTo));
			}
			if ( code == null ) {
				throw new ArgumentNullException(nameof(code));
			}

			var record = _twoFactorDb.TwoFactorCodes
				.Where(x => x.IsUsed == false)
				.Where(x => x.CreatedTime > DateTime.Now.AddMinutes(0 - withinMinutes))
				.Where(x => x.SentTo == sentTo)
				.Where(x => _me.IsAnonymous || x.UserId == _me.Id)
				.OrderByDescending(x => x.Id)
				.FirstOrDefault();

			if ( record == null ) {
				return new Failure("验证码匹配失败");
			}
			if ( record.ErrorTimes > 10 || string.Compare(code, record.Code, true) != 0 ) {
				record.ErrorTimes++;
				await _twoFactorDb.Normalize().SaveChangesAsync();
				return new Failure("验证码输入错误");
			}
			if ( setIntoUsedAfterVerifying ) {
				record.IsUsed = true;
				await _twoFactorDb.Normalize().SaveChangesAsync();
			}
			return new Success();
		}

		public async Task<Result> VerifyCode(ITwoFactorModel model, bool setIntoUsedAfterVerifying, int withinMinutes = 15) {
			if ( model == null ) {
				throw new ArgumentNullException(nameof(model));
			}
			return await VerifyCode(model.SendTo, model.Code, setIntoUsedAfterVerifying, withinMinutes);
		}
	}
}