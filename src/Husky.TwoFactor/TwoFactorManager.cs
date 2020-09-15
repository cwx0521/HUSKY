﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Husky.AliyunSms;
using Husky.Principal;
using Husky.TwoFactor.Data;
using Microsoft.EntityFrameworkCore;

namespace Husky.TwoFactor
{
	public sealed partial class TwoFactorManager
	{
		public TwoFactorManager(IPrincipalUser principal, TwoFactorDbContext twoFactorDb, AliyunSmsSender aliyunSmsSender /*, IMailSender mailSender */) {
			_me = principal;
			_twoFactorDb = twoFactorDb;
			_aliyunSmsSender = aliyunSmsSender;
			//_mailSender = mailSender;
		}

		private readonly IPrincipalUser _me;
		private readonly TwoFactorDbContext _twoFactorDb;
		private readonly AliyunSmsSender _aliyunSmsSender;
		//private readonly IMailSender _mailSender;

		public async Task<Result> RequestTwoFactorCode(string emailOrMobile, string? templateCode = null, string? signName = null, string? messageTemplateWithCodeAsArg0 = null) {
			if ( emailOrMobile == null ) {
				throw new ArgumentNullException(nameof(emailOrMobile));
			}

			var isEmail = emailOrMobile.IsEmail();
			var isMobile = emailOrMobile.IsMainlandMobile();

			if ( !isEmail && !isMobile ) {
				return new Failure($"无法发送到 '{emailOrMobile}' ");
			}

			if ( isMobile ) {
				var sentWithinMinute = _twoFactorDb.TwoFactorCodes
					.AsNoTracking()
					.Where(x => x.SentTo == emailOrMobile)
					.Where(x => x.CreatedTime > DateTime.Now.AddMinutes(-1))
					.Any();

				if ( sentWithinMinute ) {
					return new Failure("请求过于频繁");
				}
			}

			var code = new TwoFactorCode {
				UserId = _me.Id,
				Code = new Random().Next(0, 1000000).ToString("D6"),
				SentTo = emailOrMobile
			};
			_twoFactorDb.Add(code);
			await _twoFactorDb.SaveChangesAsync();

			if ( isEmail ) {
				//var content = string.Format(messageTemplateWithCodeAsArg0, code.Code);
				//await _mailSender.SendAsync("动态验证码", content, emailOrMobile);
			}
			else if ( isMobile ) {
				var argument = new AliyunSmsArgument {
					SignName = signName,
					TemplateCode = templateCode,
					Parameters = new Dictionary<string, string> {
						{ "code", code.Code }
					}
				};
				await _aliyunSmsSender.SendAsync(argument, emailOrMobile);
			}
			return new Success();
		}

		public async Task<Result> VerifyTwoFactorCode(TwoFactorModel model, bool setIntoUsedAfterVerifying, int withinMinutes = 15) {
			if ( model == null ) {
				throw new ArgumentNullException(nameof(model));
			}

			var record = _twoFactorDb.TwoFactorCodes
				.Where(x => x.IsUsed == false)
				.Where(x => x.CreatedTime > DateTime.Now.AddMinutes(0 - withinMinutes))
				.Where(x => x.SentTo == model.SendTo)
				.Where(x => _me.IsAnonymous || x.UserId == _me.Id)
				.OrderByDescending(x => x.Id)
				.Take(1)
				.FirstOrDefault();

			if ( record == null ) {
				return new Failure("验证码匹配失败");
			}
			if ( record.ErrorTimes > 10 || string.Compare(model.Code, record.Code, true) != 0 ) {
				record.ErrorTimes++;
				await _twoFactorDb.SaveChangesAsync();
				return new Failure("验证码输入错误");
			}
			if ( setIntoUsedAfterVerifying ) {
				record.IsUsed = true;
				await _twoFactorDb.SaveChangesAsync();
			}
			return new Success();
		}
	}
}