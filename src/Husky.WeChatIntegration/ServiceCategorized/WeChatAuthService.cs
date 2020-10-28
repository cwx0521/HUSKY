﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Husky.WeChatIntegration.ServiceCategorized
{
	public class WeChatAuthService
	{
		public WeChatAuthService(WeChatAppConfig wechatConfig) {
			_wechatConfig = wechatConfig;
		}

		private readonly WeChatAppConfig _wechatConfig;
		private static HttpClient? _httpClient;

		public string CreateWebQrCodeLoginScript(string redirectUrl, string styleSheetUrl) {
			_wechatConfig.RequireOpenPlatformSettings();

			var elementId = "_" + Crypto.RandomString();
			var html = @"<div id='" + elementId + @"'></div>
				<script type='text/javascript' src='https://res.wx.qq.com/connect/zh_CN/htmledition/js/wxLogin.js'></script>
				<script type='text/javascript'>
					(function loadWxLogin() {
						if (typeof WxLogin !== 'function') {			
							setTimeout(loadWxLogin, 50);	
						}
						else {
							var obj = new WxLogin({
								self_redirect: false,
								scope: 'snsapi_login',
								id: '" + elementId + @"',
								appid: '" + _wechatConfig.OpenPlatformAppId + @"',
								redirect_uri: '" + redirectUrl + @"',
								state: '" + Crypto.Encrypt(DateTime.Now.ToString("yyyy-M-d H:mm:ss"), iv: _wechatConfig.OpenPlatformAppId!) + @"',
								href: '" + styleSheetUrl + @"',
								style: ''
							});
						}
					})();
				</script>";
			return html;
		}

		public string CreateMobilePlatformAutoLoginUrl(string redirectUrl, string scope = "snsapi_userinfo") {
			_wechatConfig.RequireMobilePlatformSettings();

			return $"https://open.weixin.qq.com/connect/oauth2/authorize" +
				   $"?appid={_wechatConfig.MobilePlatformAppId}" +
				   $"&redirect_uri={HttpUtility.UrlEncode(redirectUrl)}" +
				   $"&response_type=code" +
				   $"&scope={scope}" +
				   $"&state={Crypto.Encrypt(DateTime.Now.ToString("yyyy-M-d H:mm:ss"), iv: _wechatConfig.MobilePlatformAppId!)}" +
				   $"#wechat_redirect";
		}

		public async Task<WeChatMiniProgramLoginResult> ProceedMiniProgramLoginAsync(string code) {
			_wechatConfig.RequireMiniProgramSettings();

			var url = $"https://api.weixin.qq.com/sns/jscode2session" +
					  $"?appid={_wechatConfig.MiniProgramAppId}" +
					  $"&secret={_wechatConfig.MiniProgramAppSecret}" +
					  $"&js_code={code}" +
					  $"&grant_type=authorization_code";

			try {
				_httpClient ??= new HttpClient();
				var json = await _httpClient.GetStringAsync(url);
				var d = JsonConvert.DeserializeObject<dynamic>(json);

				return new WeChatMiniProgramLoginResult {
					Ok = d.errcode == null || (int)d.errcode == 0,
					Message = d.errmsg,

					OpenId = d.openid,
					UnionId = d.unionid,
					SessionKey = d.session_key
				};
			}
			catch ( Exception e ) {
				return new WeChatMiniProgramLoginResult {
					Ok = false,
					Message = e.Message
				};
			}
		}
	}
}
