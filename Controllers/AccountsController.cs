using JWTDemo.Server.Models;
using JWTDemo.Server.Service;
using JWTDemo.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace JWTDemo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;

        public AccountsController(UserManager<IdentityUser> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromBody] RegisterModel model)
        {
            var newUser = new IdentityUser { UserName = model.Email, Email = model.Email };

            var result = await _userManager.CreateAsync(newUser, model.Password!);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);

                return Ok(new RegisterResult { Successful = false, Errors = errors });

            }
            await _userManager.AddToRoleAsync(newUser, "User");
            if(newUser.Email!.ToLower().StartsWith("admin"))
            {
                await _userManager.AddToRoleAsync(newUser, "Admin");
                return Ok(new RegisterResult { Successful = true }) ;
            }

            var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var encodeEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodeEmailToken);
            string url = $"{_configuration["AppUrl"]}/api/accounts/confirmEmail?userId={newUser.Id}&token={validEmailToken}";

            var requestDto = new RequestDTO
            {
                To = newUser.Email,
                Subject = "Confirm Email Account",
                Message = $"<p>Welcome to Netcode-Hub Site</p> <p>Please confirm your email account by clicking on this button <a href='{url}'>Click here</a></p>"
            };
            var retunText = await _emailService.SendEmail(requestDto);
            if (retunText.Contains("Mail Sent!"))
            {
                return Ok(new RegisterResult { Successful = true });
            }
            return Ok(new RegisterResult { Successful = true });
        }


        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var decodedToken = WebEncoders.Base64UrlDecode(token);
                string normalToken = Encoding.UTF8.GetString(decodedToken);
                var result = await _userManager.ConfirmEmailAsync(user, normalToken);
                if (result.Succeeded)
                {
                    return Redirect($"{_configuration["AppUrl"]}/login"!);
                }
                return BadRequest();
            }
            return BadRequest();
        }
    }
}
