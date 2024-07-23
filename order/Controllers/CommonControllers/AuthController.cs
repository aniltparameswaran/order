using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.IRepository.ICommonRepositorys;
using order.IRepository.IAdminRepositorys;
using order.Repository;
using order.Utils;
using order.DTOModel;
using order.Repository.CommonRepository;
using Twilio.Jwt.AccessToken;

namespace order.Controllers.CommonControllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepo _authRepo;
        private readonly SecurityUtils _securityUtils;
        private readonly IEmployeeRepo _userRepo;
        private readonly ITokenService _tokenService;
        private readonly ICookieService _cookieService;
        public AuthController(IAuthRepo authRepo, SecurityUtils securityUtils, IEmployeeRepo userRepo, ITokenService tokenService, ICookieService cookieService)
        {
            _authRepo = authRepo;
            _securityUtils = securityUtils;
            _userRepo = userRepo;
            _tokenService = tokenService;
            _cookieService = cookieService;
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDTOModel loginDTOModel)
        {
            try
            {
                var (status, access_token,message) = await _authRepo.Login(loginDTOModel, 0);
                if (status)
                {
                    return Ok(new { data = access_token, message = "Successfully logged in " });
                }
                return BadRequest(new { data = string.Empty, message = message });

            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("admin-login")]
        public async Task<IActionResult> AdminLogin(LoginDTOModel loginDTOModel)
        {
            try
            {
                var (status, access_token,messsage) = await _authRepo.Login(loginDTOModel, 1);
                if (status)
                {
                    
                    Console.WriteLine("Current Refresh Tokens login:");
                    foreach (var kvp in Request.Cookies)
                    {
                        Console.WriteLine($"Refresh Token: {kvp.Key}, User ID: {kvp.Value}");
                    }
                    _cookieService.SetRefreshLokinCookie();
                    return Ok(new { data = access_token, message = "Successfully logged in " });
                }
                return BadRequest(new { data = string.Empty, message = messsage });

            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            var refreshToken = Request.Cookies;
            if (refreshToken == null)
            {
                return Unauthorized();
            }
            IRequestCookieCollection cookies = Request.Cookies;
            var tokens = _tokenService.RefreshTokens(cookies);
            if (tokens == null)
            {

                return Unauthorized();
            }
            _cookieService.SetRefreshLokinCookie();
            return Ok(new { data = tokens, message = StatusUtils.SUCCESS });

        }
        [HttpGet]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string user_name)
        {
            try
            {

                var (status, massage) = await _authRepo.ForgotPassword(user_name);
                if (status)
                {
                    return Ok(new { data = massage, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = string.Empty, message = massage });

            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("varification-otp")]
        public async Task<IActionResult> VerificationOtp(CheckOtpDTOModel checkOtpDTOModel)
        {
            try
            {

                var (status, massage) = await _authRepo.VerificationOtp(checkOtpDTOModel.data, checkOtpDTOModel.otp);
                if (status)
                {
                    return Ok(new { data = massage, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = string.Empty, message = massage });

            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut]
        [Route("reset-password")]
        public async Task<IActionResult> RestPassword(UpdatePasswordDTOModel updatePasswordDTOModel)
        {
            try
            {

                var (status, message) = await _authRepo.RestPassword(updatePasswordDTOModel.data, updatePasswordDTOModel.password);
                if (status)
                {
                    return Ok(new { data = string.Empty, message });
                }


                return BadRequest(new { data = string.Empty, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // Expire the token
                _tokenService.ExpireToken(token);
            }
            _cookieService.ClearAllCookies();
            _cookieService.ClearSpecificCookie("loggedIn");
            foreach (var kvp in Request.Cookies)
            {
                Console.WriteLine($"Refresh Token: {kvp.Key}, User ID: {kvp.Value}");
            }
            return Ok();
        }


    }
}
