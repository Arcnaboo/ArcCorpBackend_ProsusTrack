using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ArcCorpBackend.Services;
using ArcCorpBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace ArcCorpBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {

        private static readonly string ARCCORP_EMAIL_VERIFICATION_TEMPLATE = 
                                @"<!DOCTYPE html>
                                <html xmlns='http://www.w3.org/1999/xhtml'>
                                <head>
                                <meta charset='UTF-8' />
                                <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
                                <title>ArcCorp Email Verification</title>
                                <style>
                                body {{ margin:0; padding:0; background-color:#0b0b0b; font-family:'Open Sans', sans-serif; color:#ffffff; }}
                                .container {{ width: 100%; max-width: 600px; margin: auto; padding: 40px; background-color: #1a1a1a; border-radius: 12px; }}
                                .logo {{ text-align: center; margin-bottom: 30px; }}
                                .logo img {{ width: 150px; height: auto; }}
                                h1 {{ color: #00d8ff; font-size: 36px; text-align: center; margin-bottom: 20px; }}
                                p {{ color: #ffffff; font-size: 18px; line-height: 1.6; text-align: center; }}
                                .code {{ font-size: 48px; font-weight: bold; color: #00d8ff; text-align: center; letter-spacing: 8px; margin: 40px 0; }}
                                footer {{ text-align:center; font-size:14px; color:#aaaaaa; margin-top: 50px; }}
                                a {{ color: #00d8ff; text-decoration: none; font-weight: bold; }}
                                </style>
                                </head>
                                <body>
                                <div class='container'>
                                <div class='logo'>
                                    <img src='https://files.catbox.moe/wuvmy1.png' alt='ArcCorp Logo' />
                                </div>
                                <h1>ArcCorp Verification</h1>
                                <p>Hello {0},</p>
                                <p>Use the code below to verify your account and complete your login:</p>
                                <div class='code'>{1}</div>
                                <p>If you did not request this code, please ignore this email.</p>
                                <footer>
                                    &copy; {2} ArcCorp AI Systems | <a href='https://akgur.com'>akgur.com</a>
                                </footer>
                                </div>
                                </body>
                                </html>";




        /// <summary>
        /// Login endpoint using query string params.
        /// Call with only 'email' to request code, or both 'email' and 'code' to validate.
        /// Example: /api/users/login?email=user@example.com
        ///          /api/users/login?email=user@example.com&code=1234
        /// </summary>
        [HttpGet]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResultModel>> Login(string email, string code = null)
        {
            var result = new LoginResultModel();

            if (string.IsNullOrEmpty(email))
            {
                result.Success = false;
                result.Message = "Error: Email parameter is required to proceed with login.";
                return BadRequest(result);
            }

            if (string.IsNullOrEmpty(code))
            {
                string generatedCode;

                // 1️⃣ Existing user: generate/send new code
                if (UserService.IsExistingUser(email, out generatedCode))
                {
                    var emailHtml = string.Format(ARCCORP_EMAIL_VERIFICATION_TEMPLATE, email, generatedCode, DateTime.UtcNow.Year);
                    await EmailService.SendEmailAsync(email, "ArcCorp Login Verification Code", emailHtml, isHtml: true);

                    result.Success = true;
                    result.Message = $"A new verification code has been sent to existing user {email}. Please check your inbox and spam folder.";
                    return Ok(result);
                }

                // 2️⃣ Otherwise: use pending or new code
                var existingCode = UserService.GetExistingCode(email);
                if (!string.IsNullOrEmpty(existingCode))
                {
                    generatedCode = existingCode;
                }
                else
                {
                    generatedCode = UserService.NewUser(email);
                }

                var fallbackHtml = string.Format(ARCCORP_EMAIL_VERIFICATION_TEMPLATE, email, generatedCode, DateTime.UtcNow.Year);
                await EmailService.SendEmailAsync(email, "ArcCorp Login Verification Code", fallbackHtml, isHtml: true);

                result.Success = true;
                result.Message = $"Verification code sent to {email}. If you don’t see the email, please check your spam or junk folder.";
                return Ok(result);
            }
            else
            {
                // 3️⃣ Code present: validate it
                if (await UserService.ValidateCode(email, code))
                {
                    var jwt = AuthService.GenerateToken(email);

                    result.Success = true;
                    result.Message = "Login successful. Welcome!";
                    result.JwtAuthKey = jwt;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Message = "Invalid verification code. Please check your email and try again.";
                    return Unauthorized(result);
                }
            }
        }

        [HttpGet("test_online")]
        [AllowAnonymous]
        public ActionResult Test_online()
        {
            return Ok("foobar");
        }


    }
}
