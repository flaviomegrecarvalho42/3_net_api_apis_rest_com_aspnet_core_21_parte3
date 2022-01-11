using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Alura.ListaLeitura.Seguranca;
using Alura.ListaLeitura.WebApp.HttpClients;
using Alura.ListaLeitura.WebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Alura.ListaLeitura.WebApp.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AuthenticationApiClient _authApiClient;

        public UsuarioController(AuthenticationApiClient authApiClient)
        {
            _authApiClient = authApiClient;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _authApiClient.PostLoginAsync(loginModel);

                if (result.IsSuceeded)
                {
                    //Onde guardar o token?
                    //Através de um cookie de autenticação - link do MS Docs

                    //Primeiro vamos criar os direitos/reinvindicações/claims
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginModel.Login),
                        new Claim("Token", result.Token)
                    };

                    //E guardar esses direitos na identidade principal
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    //Configurar expiração do cookie para um valor menor que a expiração do token
                    AuthenticationProperties authenticationProperties = new AuthenticationProperties
                    {
                        IssuedUtc = DateTime.UtcNow,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(25),
                        IsPersistent = true
                    };

                    //E finalmente autenticar via cookie com essa identidade
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                                  new ClaimsPrincipal(claimsIdentity),
                                                  authenticationProperties);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(String.Empty, "Erro na autenticação");
                return View(loginModel);
            }

            return View(loginModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                await _authApiClient.PostRegisterAsync(registerViewModel);
                return RedirectToAction("Index", "Home");
            }

            return View(registerViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}