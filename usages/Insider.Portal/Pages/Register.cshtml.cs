using System.ComponentModel.DataAnnotations;
using Husky.Authentication.Abstractions;
using Husky.Injection;
using Husky.Sugar;
using Insider.Portal.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Insider.Portal.Pages
{
	public class RegisterModel : PageModel
	{
		public RegisterModel(IPrincipal principal) {
			_my = principal;
		}

		readonly IPrincipal _my;

		const string _typeName = "����";
		public AccountNameType AccountNameType => AccountNameType.Email;

		[Required(ErrorMessage = "������д����������" + _typeName + "��Ϊ�ʺ�����")]
		[EmailAddress(ErrorMessage = "��ʽ��Ч����������" + _typeName + "��Ϊ�ʺ�����")]
		[RegularExpression(StringTest.EmailRegexPattern, ErrorMessage = "��ʽ��Ч����������" + _typeName + "��Ϊ�ʺ�����")]
		[Remote(nameof(ApiController.IsAccountApplicable), "Api", AdditionalFields = nameof(AccountNameType), HttpMethod = "POST", ErrorMessage = "{0}�Ѿ���ע���ˡ�")]
		[Display(Name = _typeName)]
		public string AccountName { get; set; }

		[Required(ErrorMessage = "���������д��")]
		[StringLength(18, MinimumLength = 8, ErrorMessage = "���볤������{2}-{1}λ֮�䡣")]
		[DataType(DataType.Password)]
		[Display(Name = "����")]
		public string Password { get; set; }

		[Required(ErrorMessage = "�ظ�����һ�����롣")]
		[MaxLength(15), Compare(nameof(Password), ErrorMessage = "�����������벻һ�¡�")]
		[DataType(DataType.Password)]
		[Display(Name = "����ȷ��")]
		public string PasswordConfirm { get; set; }

		public void OnGet() {
		}

		public async void OnPost() {
			if ( ModelState.IsValid ) {
				var result = await _my.User().SignUp(AccountNameType, AccountName, Password, verified: false);
				if ( result.Ok ) {
					//return View(nameof(Verify), new RegistryVerifyModel { AccountName = AccountName, AutoSend = true });
				}
				ModelState.AddModelError(nameof(AccountName), result.Message);
			}
		}
	}
}