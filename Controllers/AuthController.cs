using System.Net;
using System.Security.Claims;
using DoomTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoomTracker.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("Auth/Login")]
    public IActionResult Login(string returnUrl = "/")
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse", new { returnUrl }) };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("Auth/GoogleResponse")]
    public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (result?.Principal == null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var name = result.Principal.FindFirstValue(ClaimTypes.Name);
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new User
            {
                Email = email,
                Username = name ?? email!.Split('@')[0],
                PasswordHash = "GoogleAuth",
                RegisteredAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
        
        return LocalRedirect(returnUrl);
    }

    [HttpPost("Auth/Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}