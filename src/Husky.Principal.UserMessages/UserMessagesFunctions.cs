﻿using System.Linq;
using System.Threading.Tasks;
using Husky.Principal.UserMessages.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Husky.Principal.UserMessages
{
	public sealed class UserMessagesFunctions
	{
		internal UserMessagesFunctions(IPrincipalUser principal) {
			_me = principal;
			_db = principal.ServiceProvider.GetRequiredService<IUserMessagesDbContext>();
		}

		private readonly IPrincipalUser _me;
		private readonly IUserMessagesDbContext _db;

		public async Task<Result<UserMessage>> NewIncomeMessage(string message) {
			if ( _me.IsAnonymous ) {
				return new Failure<UserMessage>("需要先登录");
			}

			var userMessage = new UserMessage {
				UserId = _me.Id,
				Content = message,
			};

			var validationResult = ValidatorHelper.Validate(userMessage);
			if ( !validationResult.Ok ) {
				return new Failure<UserMessage>(validationResult.Message);
			}

			_db.UserMessage.Add(userMessage);
			await _db.Normalize().SaveChangesAsync();

			return new Success<UserMessage>(userMessage);
		}

		public async Task<Result> MarkRead(params int[] userMessageIdArray) {
			if ( _me.IsAnonymous ) {
				return new Failure("需要先登录");
			}

			var arr = await _db.UserMessage
				.Where(x => x.UserId == _me.Id)
				.Where(x => x.IsRead == false)
				.Where(x => userMessageIdArray.Contains(x.Id))
				.Select(x => x.Id)
				.ToArrayAsync();

			arr.AsParallel().ForAll(id => {
				var row = new UserMessage { Id = id };
				_db.UserMessage.Attach(row);
				row.IsRead = true;
			});
			await _db.Normalize().SaveChangesAsync();

			return new Success();
		}

		public async Task<Result> MarkReadAll() {
			if ( _me.IsAnonymous ) {
				return new Failure("需要先登录");
			}

			var sql = $"update UserMessages" +
				$" set   {nameof(UserMessage.IsRead)}=1" +
				$" where {nameof(UserMessage.IsRead)}=0 and {nameof(UserMessage.UserId)}={{0}}";

			await _db.Normalize().Database.ExecuteSqlRawAsync(sql, _me.Id);
			return new Success();
		}

		public async Task<Result> DeleteMessages(params int[] userMessageIdArray) {
			if ( _me.IsAnonymous ) {
				return new Failure("需要先登录");
			}

			var arr = await _db.UserMessage
				.Where(x => x.UserId == _me.Id)
				.Where(x => x.IsRead)
				.Where(x => userMessageIdArray.Contains(x.Id))
				.Select(x => x.Id)
				.ToArrayAsync();

			arr.AsParallel().ForAll(id => {
				var row = new UserMessage { Id = id };
				_db.UserMessage.Attach(row);
				row.IsDeleted = true;
			});
			await _db.Normalize().SaveChangesAsync();

			return new Success();
		}

		public async Task<Result> DeleteAllReadMessages() {
			if ( _me.IsAnonymous ) {
				return new Failure("需要先登录");
			}

			var sql = $"delete from UserMessages" +
				$" set   {nameof(UserMessage.IsDeleted)}=1" +
				$" where {nameof(UserMessage.IsRead)}=1 and {nameof(UserMessage.UserId)}={{0}}";

			await _db.Normalize().Database.ExecuteSqlRawAsync(sql, _me.Id);
			return new Success();
		}
	}
}
