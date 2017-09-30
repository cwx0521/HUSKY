using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Husky.Authentication.Abstractions;
using Husky.Injection;
using Husky.Sugar;
using Insider.Portal.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Insider.Portal.Pages.Account
{
	public class RegisterModel : PageModel
	{
		public RegisterModel(IPrincipal principal) {
			_my = principal;
		}

		readonly IPrincipal _my;

		[BindProperty]
		public RegisterAccountModel Account { get; set; }

		[TempData]
		public string AccountName { get; set; }
		[TempData]
		public bool AutoSend { get; set; } = true;

		public void OnGet() {
		}

		public async Task<IActionResult> OnPostAsync() {
			if ( ModelState.IsValid ) {
				var result = await _my.User().SignUp(Account.Type, Account.Name, Account.Password, verified: false);
				if ( result.Ok ) {
					AccountName = Account.Name;
					AutoSend = true;
					return RedirectToPage("Verify");
				}
				ModelState.AddModelError(nameof(Account.Name), result.Message);
			}
			return Page();
		}
	}

	public class RegisterAccountModel
	{
		const string _typeName = "����";
		public AccountNameType Type => AccountNameType.Email;

		[Required(ErrorMessage = "������д����������" + _typeName + "��Ϊ�ʺ�����")]
		[EmailAddress(ErrorMessage = "��ʽ��Ч����������" + _typeName + "��Ϊ�ʺ�����")]
		[RegularExpression(StringTest.EmailRegexPattern, ErrorMessage = "��ʽ��Ч����������" + _typeName + "��Ϊ�ʺ�����")]
		[Remote(nameof(ApiController.IsAccountApplicable), "Api", AdditionalFields = nameof(Type), HttpMethod = "POST", ErrorMessage = "{0}�Ѿ���ע���ˡ�")]
		[Display(Name = _typeName)]
		public string Name { get; set; }

		[Required(ErrorMessage = "���������д��")]
		[StringLength(18, MinimumLength = 8, ErrorMessage = "���볤������{2}-{1}λ֮�䡣")]
		[DataType(DataType.Password)]
		[Display(Name = "����")]
		public string Password { get; set; }

		[Required(ErrorMessage = "�ظ�����һ�����롣")]
		[MaxLength(15), Compare(nameof(Password), ErrorMessage = "�����������벻һ�¡�")]
		[DataType(DataType.Password)]
		[Display(Name = "����ȷ��")]
		public string PasswordRepeat { get; set; }
	}
}