
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Ders_Sinav_Programi.Helpers;
using Ders_Sinav_Programi.Models;
using System.Security.Claims;

namespace Ders_Sinav_Programi.Controllers
{
    public class AccountController : Controller
    {
        private readonly DbHelper _db;

        public AccountController(DbHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var conn = _db.GetConnection();
            conn.Open();

            var query = @"
                SELECT k.Kullanici_id, k.Email, k.Sifre_hash, k.Aktif_mi, r.Rol_adi
                FROM Kullanici k
                INNER JOIN Roller r ON k.Rol_id = r.Rol_id
                WHERE k.Email = @Email";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", model.Email);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                TempData["LoginError"] = "E-posta veya şifre hatalı.";
                return View(model);
            }

            bool aktifMi = reader.GetBoolean(reader.GetOrdinal("Aktif_mi"));
            if (!aktifMi)
            {
                TempData["LoginError"] = "Hesabınız aktif değil. Lütfen yönetici ile iletişime geçin.";
                return View(model);
            }

            string sifreHash = reader.GetString(reader.GetOrdinal("Sifre_hash"));
            string rolAdi = reader.GetString(reader.GetOrdinal("Rol_adi"));
            int kullaniciId = reader.GetInt32(reader.GetOrdinal("Kullanici_id"));

            
            bool sifreDogruMu = BCrypt.Net.BCrypt.Verify(model.Password, sifreHash);

            if (!sifreDogruMu)
            {
                TempData["LoginError"] = "E-posta veya şifre hatalı.";
                return View(model);
            }

            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Role, rolAdi),
                new Claim(ClaimTypes.NameIdentifier, kullaniciId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = model.RememberMe }
            );

            return RedirectToAction("Index", "Musaitlik");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
