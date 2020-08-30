﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Husky.Razor
{
	public static partial class HtmlHelperExtensions
	{
		public static IHtmlContent ButtonWithConfirm(this IHtmlHelper helper, string buttonFace, string message = null, object htmlAttributes = null) {
			var id = "_" + Crypto.RandomString();

			var button = new TagBuilder("button");
			button.MergeAttributes(htmlAttributes);
			button.MergeAttribute("type", "button");
			button.MergeAttribute("data-toggle", "modal");
			button.MergeAttribute("data-target", "#" + id);

			var result = new HtmlContentBuilder();
			result.AppendHtml(button.RenderStartTag());
			result.AppendHtml(buttonFace);
			result.AppendHtml(button.RenderEndTag());
			result.AppendHtml(helper.ModalForConfirmation(id, message));
			return result;
		}
	}
}
