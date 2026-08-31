using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControleFinanceiroWeb.Models;
using ControleFinanceiroWeb.Models.ViewModels;
using ControleFinanceiroWeb.Services.Security;

namespace ControleFinanceiroWeb.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public const string SecurityStampClaim = "cfw:stamp";

        public const string InstanceClaim = "cfw:instance";

        private readonly ISecurityService _securityService;

        public AccountController(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        #region Public Methods

        [HttpGet]
        public async Task<IActionResult> Setup()
        {
            if (await _securityService.IsPinConfiguredAsync())
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new PinSetupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(PinSetupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _securityService.DefinePinAsync(model.Pin);

            if (!result.Success)
            {
                model.ErrorMessage = result.Message;

                return View(model);
            }

            await SignInAsync();

            return RedirectToAction("Index", "Summary");
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl)
        {
            if (!await _securityService.IsPinConfiguredAsync())
            {
                return RedirectToAction(nameof(Setup));
            }

            return View(new PinLoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(PinLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _securityService.ValidatePinAsync(model.Pin);

            if (!result.Success)
            {
                model.ErrorMessage = result.Message;

                return View(model);
            }

            await SignInAsync();

            return RedirectToLocal(model.ReturnUrl);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePin([FromBody] PinChangeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ServiceResult { Success = false, Message = "Informe um PIN de 6 dígitos." });
            }

            var result = await _securityService.ChangePinAsync(model.CurrentPin, model.NewPin);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            await SignInAsync();

            return Ok(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Denied()
        {
            return RedirectToAction(nameof(Login));
        }

        #endregion

        #region Private Methods

        private async Task SignInAsync()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "Casa"),
                new(SecurityStampClaim, await _securityService.GetSecurityStampAsync() ?? string.Empty),
                new(InstanceClaim, AppInstance.Id)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // A session cookie, so closing the browser ends the session and the
            // PIN is asked for again. The household budget is reached from
            // machines other people also use, and a cookie kept on disk would
            // hand everything over to whoever opens the browser next.
            var properties = new AuthenticationProperties
            {
                IsPersistent = false
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                properties);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Summary");
        }

        #endregion
    }
}
