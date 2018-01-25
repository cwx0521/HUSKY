using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Husky.Authentication.Abstractions;
using Husky.Injection;
using Husky.TwoFactor.Data;
using Insider.Portal.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Insider.Portal.Pages.Account
{
	public class VerifyModel : PageModel
	{
		public VerifyModel(IPrincipal principal) {
			_my = principal;
		}

		readonly IPrincipal _my;

		[BindProperty]
		public VerifyDataModel Data { get; set; }

		[TempData]
		public string AccountName { get; set; }
		[TempData]
		public bool AutoSend { get; set; }

		public void OnGet() {
			Data.AccountName = AccountName;
			Data.AutoSend = AutoSend;
		}

		public async Task<IActionResult> OnPostAsync() {
			if ( ModelState.IsValid ) {
				var result = await _my.TwoFactor().VerifyTwoFactorCode(Data.AccountName, TwoFactorPurpose.Registry, Data.TwoFactorCode, true);
				if ( result.Ok ) {
					return Redirect("/");
				}
				ModelState.AddModelError(nameof(Data.TwoFactorCode), result.Message);
			}
			return Page();
		}
	}

	public class VerifyDataModel
	{
		[Display(Name = "�ʺ�")]
		public string AccountName { get; set; }

		[Required(ErrorMessage = "�����������յ�����֤�롣")]
		[RegularExpression(@"^\d{6}$", ErrorMessage = "��������ȷ����֤�����֣�����6λ��")]
		[Remote(nameof(ApiController.IsTwoFactorCodeValid), "Api", AdditionalFields = nameof(AccountName) + "," + nameof(TwoFactorPurpose), HttpMethod = "POST", ErrorMessage = "��֤�벻��ȷ��")]
		[Display(Name = "��֤��")]
		public string TwoFactorCode { get; set; }

		public bool AutoSend { get; set; }

		public TwoFactorPurpose TwoFactorPurpose => TwoFactorPurpose.Registry;
	}
}