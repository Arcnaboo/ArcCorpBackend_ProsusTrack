using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ArcCorpBackend.Services;
using ArcCorpBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using ArcCorpBackend.Domain.Repositories;

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

        [HttpGet]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResultModel>> Login(string email, string code = null)
        {
            Log.Information("Login - initiated");

            var result = new LoginResultModel();

            if (string.IsNullOrEmpty(email))
            {
                Log.Information("Login - missing email");
                result.Success = false;
                result.Message = "Error: Email parameter is required to proceed with login.";
                return BadRequest(result);
            }

            if (string.IsNullOrEmpty(code))
            {
                string generatedCode;

                if (UserService.IsExistingUser(email, out generatedCode))
                {
                    var emailHtml = string.Format(ARCCORP_EMAIL_VERIFICATION_TEMPLATE, email, generatedCode, DateTime.UtcNow.Year);
                    await EmailService.SendEmailAsync(email, "ArcCorp Login Verification Code", emailHtml, isHtml: true);

                    result.Success = true;
                    result.Message = $"A new verification code has been sent to existing user {email}. Please check your inbox and spam folder.";
                    Log.Information("Login - verification code sent to existing user");
                    return Ok(result);
                }

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
                Log.Information("Login - verification code sent to new or pending user");
                return Ok(result);
            }
            else
            {
                if (await UserService.ValidateCode(email, code))
                {
                    var repo = new UsersRepository();
                    var user = await repo.GetUserByEmailAsync(email);
                    var jwt = AuthService.GenerateToken(user.UserId.ToString());

                    result.Success = true;
                    result.Message = "Login successful. Welcome!";
                    result.JwtAuthKey = jwt;
                    Log.Information("Login - completed successfully");
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Message = "Invalid verification code. Please check your email and try again.";
                    Log.Information("Login - invalid code");
                    return Unauthorized(result);
                }
            }
        }

        [HttpGet("test_online")]
        [AllowAnonymous]
        public ActionResult Test_online()
        {
            Log.Information("Test_online - initiated");
            Log.Information("Test_online - completed successfully");
            return Ok("foobar");
        }

        [HttpGet("get_user_model")]
        [Authorize]
        public async Task<ActionResult<UserModel>> GetUserModel()
        {
            Log.Information("GetUserModel - initiated");

            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (!AuthService.ValidateToken(token, out string userId))
            {
                Log.Information("GetUserModel - unauthorized access");
                return Unauthorized("Invalid or expired token");
            }

            try
            {
                var userModel = await UserService.GetUserModelById(userId);
                if (userModel == null)
                {
                    Log.Information("GetUserModel - user not found");
                    return Ok("User not found");
                }
                Log.Information("GetUserModel - completed successfully");
                return Ok(userModel);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetUserModel - error occurred");
                return Ok($"Error: {ex.Message}");
            }
        }

        [HttpGet("get_chats")]
        [Authorize]
        public async Task<ActionResult<List<ChatModel>>> GetChats()
        {
            Log.Information("GetChats - initiated");

            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (!AuthService.ValidateToken(token, out string userId))
            {
                Log.Information("GetChats - unauthorized access");
                return Unauthorized("Invalid or expired token");
            }

            try
            {
                var chats = await UserService.GetChatsForUser(userId);
                Log.Information("GetChats - completed successfully");
                return Ok(chats);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetChats - error occurred");
                return Ok($"Error: {ex.Message}");
            }
        }

        [HttpGet("get_messages")]
        [Authorize]
        public async Task<ActionResult<List<MessageModel>>> GetMessages(string chatId)
        {
            Log.Information("GetMessages - initiated");

            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (!AuthService.ValidateToken(token, out string _))
            {
                Log.Information("GetMessages - unauthorized access");
                return Unauthorized("Invalid or expired token");
            }

            try
            {
                var messages = await UserService.GetMessagesForChat(chatId);
                if (messages == null)
                {
                    Log.Information("GetMessages - chat not found");
                    return Ok("Chat not found");
                }
                Log.Information("GetMessages - completed successfully");
                return Ok(messages);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetMessages - error occurred");
                return Ok($"Error: {ex.Message}");
            }
        }

        [HttpPost("new_chat")]
        [Authorize]
        public async Task<ActionResult<ChatResultModel>> NewChat()
        {
            Log.Information("NewChat - initiated");

            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (!AuthService.ValidateToken(token, out string userId))
            {
                Log.Information("NewChat - unauthorized access");
                return Unauthorized("Invalid or expired token");
            }

            try
            {
                var chatModel = await UserService.New_Chat(userId);
                var chatresult = new ChatResultModel
                {
                    ChatModel = chatModel,
                    Success = true,
                    Message = "chat initiated"
                };
                Log.Information("NewChat - completed successfully");
                return Ok(chatresult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "NewChat - error occurred");
                return Ok($"Error: {ex.Message}");
            }
        }

        [HttpPost("prompt")]
        [Authorize]
        public async Task<ActionResult<UniversalIntentResponseModel>> Prompt(UserPromptParamModel promptParam)
        {
            Log.Information("Prompt - initiated");

            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (!AuthService.ValidateToken(token, out string userId))
            {
                Log.Information("Prompt - unauthorized access");
                return Unauthorized("Invalid or expired token");
            }

            if (promptParam == null || string.IsNullOrWhiteSpace(promptParam.ChatId) || string.IsNullOrWhiteSpace(promptParam.UserMessage))
            {
                Log.Information("Prompt - missing chatId or userMessage");
                return Ok("Error: chatId and userMessage must be provided in the request body.");
            }

            try
            {
                // First: query the chat session for intent response
                var response = await ChatService.Query(promptParam.ChatId, promptParam.UserMessage);

                // Second: evaluate prompt for user data preferences
                var repo = new UsersRepository();
                var user = await repo.GetUserByIdAsync(Guid.Parse(userId));
                var userDataService = SynapTronUserDataService.Create(repo);
                var userDataResult = await userDataService.EvaluatePromptAsync(promptParam.UserMessage, user);

                Log.Information("@Prompt - SynapTronUserDataService returned: {UserDataResult}", userDataResult);

                Log.Information("Prompt - completed successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Prompt - error occurred");
                return Ok($"Error: {ex.Message}");
            }
        }
    }
}
