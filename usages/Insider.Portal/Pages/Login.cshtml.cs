using System.ComponentModel.DataAnnotations;
using Husky.Authentication.Abstractions;
using Husky.Injection;
using Husky.Sugar;
using Husky.Users.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Insider.Portal.Pages
{
	public class LoginModel : PageModel
	{
		public LoginModel(IPrincipal principal) {
			_my = principal;
		}

		readonly IPrincipal _my;

		[Required(ErrorMessage = "����д��¼���������ǿ�ʹ��������ֻ��š�")]
		[RegularExpression(@"^([-0-9a-zA-Z.+_]+@[-0-9a-zA-Z.+_]+\.[a-zA-Z]{2,4})|(1[3578]\d{9})$", ErrorMessage = "��ʽ��Ч������ʹ��������ֻ�����Ϊ�û�����")]
		[Display(Name = "��¼�ʺ�")]
		public string AccountName { get; set; }

		[Required(ErrorMessage = "����д���롣")]
		[StringLength(18, MinimumLength = 8, ErrorMessage = "���볤������{2}-{1}λ֮�䡣")]
		[DataType(DataType.Password)]
		[Display(Name = "����")]
		public string Password { get; set; }

		public void OnGet()
        {
        }

		public async void OnPost() {
			if ( ModelState.IsValid ) {
				var result = await _my.User().SignIn(AccountName, Password);
				if ( result == LoginResult.Success ) {
					Redirect("/");
				}
				ModelState.AddModelError("", result.ToLabel());
			}
		}
	}
}