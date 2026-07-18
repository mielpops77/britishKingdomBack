using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IEmailService _emailService; // Ajoutez cette ligne pour injecter le service d'e-mails
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailService emailService, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService; // Initialisez le service d'e-mails
        _configuration = configuration;

    }


    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (model == null)
        {
            return BadRequest(new { message = "Invalid model data." });
        }

        try
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };

            // Ajoutez la logique pour spécifier le UserId ici
            user.Id = model.UserId.ToString();

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // L'utilisateur a été créé avec succès. Vous pouvez ajouter d'autres logiques ici si nécessaire.
                return Ok(new { message = "User registered successfully." });
            }
            else
            {
                // La création de l'utilisateur a échoué. Retournez les erreurs.
                return BadRequest(new { message = "User registration failed.", errors = result.Errors });
            }
        }
        catch (Exception ex)
        {
            // Une exception s'est produite lors de la création de l'utilisateur. Retournez l'erreur.
            return StatusCode(500, new { message = "Internal server error.", error = ex.ToString() });
        }
    }

    public class LoginResponseViewModel
    {
        public string Message { get; set; }
        public string UserId { get; set; } // Ajoutez cette propriété pour contenir l'ID de l'utilisateur
        public string Token { get; set; }
    }


    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; } // Ajoutez cette propriété
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (model == null)
        {
            return BadRequest(new { message = "Invalid model data." });
        }

        try
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // L'utilisateur a été authentifié avec succès.
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var token = GenerateJwtToken(user);
                    return Ok(new LoginResponseViewModel { Message = "User logged in successfully.", UserId = user.Id, Token = token });
                }
                else
                {
                    return BadRequest(new { message = "User not found." });
                }
            }
            else if (result.RequiresTwoFactor)
            {
                // L'authentification à deux facteurs est requise.
                return BadRequest(new { message = "Two-factor authentication is required." });
            }
            else if (result.IsLockedOut)
            {
                // L'utilisateur est actuellement verrouillé en raison d'échecs d'authentification répétés.
                return BadRequest(new { message = "User account is locked out." });
            }
            else
            {
                // L'authentification a échoué. Retournez un message d'erreur approprié.
                return BadRequest(new { message = "Invalid login attempt." });
            }
        }
        catch (Exception ex)
        {
            // Une exception s'est produite lors de l'authentification. Retournez l'erreur.
            return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
        }
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var expiryDays = double.TryParse(_configuration["Jwt:ExpiryDays"], out var d) ? d : 30;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expiryDays),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    public class UserInfoViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
    }



    [Authorize]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            // Récupérez tous les utilisateurs enregistrés dans le système
            var users = await _userManager.Users.ToListAsync();

            // Créez une liste pour stocker les informations des utilisateurs (ID et e-mail)
            var usersInfo = new List<UserInfoViewModel>();

            // Parcourez chaque utilisateur et récupérez son ID et son e-mail
            foreach (var user in users)
            {
                var userInfo = new UserInfoViewModel
                {
                    UserId = user.Id,
                    Email = user.Email
                };
                usersInfo.Add(userInfo);
            }

            // Retournez la liste des informations des utilisateurs avec un message de réussite
            return Ok(new { users = usersInfo });
        }
        catch (Exception ex)
        {
            // Une exception s'est produite lors de la récupération des utilisateurs. Retournez l'erreur.
            return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
        }
    }

    public class ForgotPasswordViewModel
    {
        public string Email { get; set; }
    }





    [AllowAnonymous]
    [HttpPost("forgotpassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
    {
        if (model == null)
        {
            return BadRequest(new { message = "Invalid model data." });
        }

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // L'utilisateur avec cet e-mail n'existe pas
                return BadRequest(new { message = "User not found." });
            }

            // Générez un jeton de réinitialisation de mot de passe
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Créez l'URL de réinitialisation du mot de passe
            var resetUrl = $"http://localhost:65255/resetpassword?email={model.Email}&token={token}";


            // Envoyez l'e-mail de réinitialisation du mot de passe à l'utilisateur
            var emailSent = await _emailService.SendPasswordResetEmail(model.Email, resetUrl);

            if (emailSent)
            {
                // L'e-mail de réinitialisation du mot de passe a été envoyé avec succès
                return Ok(new { message = "Password reset email sent successfully." });
            }
            else
            {
                // Échec de l'envoi de l'e-mail de réinitialisation du mot de passe
                return StatusCode(500, new { message = "Failed to send password reset email." });
            }
        }
        catch (Exception ex)
        {
            // Une exception s'est produite lors de l'envoi de l'e-mail. Retournez l'erreur.
            return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
        }
    }

    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }



    [AllowAnonymous]
    [HttpPost("resetpassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
    {
        if (model == null)
        {
            return BadRequest(new { message = "Invalid model data." });
        }

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // L'utilisateur avec cet e-mail n'existe pas
                return BadRequest(new { message = "User not found." });
            }

            // Réinitialiser le mot de passe pour l'utilisateur avec le token fourni
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (result.Succeeded)
            {
                // Le mot de passe a été réinitialisé avec succès
                return Ok(new { message = "Password reset successfully." });
            }
            else
            {
                // La réinitialisation du mot de passe a échoué. Retournez les erreurs.
                return BadRequest(new { message = "Failed to reset password.", errors = result.Errors });
            }
        }
        catch (Exception ex)
        {
            // Une exception s'est produite lors de la réinitialisation du mot de passe. Retournez l'erreur.
            return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
        }
    }


    [Authorize]
    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { message = "User deleted successfully." });
            }
            else
            {
                return BadRequest(new { message = "Failed to delete user.", errors = result.Errors });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
        }
    }


}


