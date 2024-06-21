using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.IRepository;
using order.Repository;
using order.Utils;

namespace order.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepo _authRepo;
        private readonly SecurityUtils _securityUtils;
        private readonly IUserRepo _userRepo;
        public AuthController(IAuthRepo authRepo, SecurityUtils securityUtils, IUserRepo userRepo)
        {
            _authRepo = authRepo;
            _securityUtils = securityUtils;
            _userRepo = userRepo;
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(string? email, string? phone, string password)
        {
            try
            {
                if(email != null || phone != null)
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        var (email_exist_user_id, email_message) = await _userRepo.IsEmailExist(email);
                        if (email_exist_user_id == 0)
                        {
                            return BadRequest(new { data = string.Empty, message = email_message });
                        }
                    }
                    if (!string.IsNullOrEmpty(phone))
                    {
                        var (phone_number_exist_user_id, phone_number_message) = await _userRepo.IsPhoneNumberExist(phone);
                        if (phone_number_exist_user_id == 0)
                        {
                            return BadRequest(new { data = string.Empty, message = phone_number_message });
                        }
                    }
                    var (status, access_token) = await _authRepo.Login(email, phone, password);
                    if (status)
                    {
                        return Ok(new { data = access_token, message = "Successfully logged in " });
                    }
                    return BadRequest(new { data = string.Empty, message = access_token });
                }
                return BadRequest(new { data = string.Empty, message = StatusUtils.INVALID_PHONE_AND_EMAIL });
            }
               
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string? email, string? phone)
        {
            try
            {
                if (email != null || phone != null)
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        var (email_exist_user_id, email_message) = await _userRepo.IsEmailExist(email);
                        if (email_exist_user_id == 0)
                        {
                            return BadRequest(new { data = string.Empty, message = email_message });
                        }
                    }
                    if (!string.IsNullOrEmpty(phone))
                    {
                        var (phone_number_exist_user_id, phone_number_message) = await _userRepo.IsPhoneNumberExist(phone);
                        if (phone_number_exist_user_id == 0)
                        {
                            return BadRequest(new { data = string.Empty, message = phone_number_message });
                        }
                    }
                    
                    

                    var (status, massage) = await _authRepo.ForgotPassword(email, phone);
                    if (status)
                    {
                        return Ok(new { data = massage, message = "Successfully send mail " });
                    }
                    return BadRequest(new { data = string.Empty, message = massage });
                }
                return BadRequest(new { data = string.Empty, message = StatusUtils.INVALID_PHONE_AND_EMAIL });
            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("varification-otp")]
        public async Task<IActionResult> VarificationOtp(string data, int otp)
        {
            try
            {
                
                    var (status, massage) = await _authRepo.VarificationOtp(data, otp);
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
        public async Task<IActionResult> RestPassword(string data, string password)
        {
            try
            {
                
                var (status,message) = await _authRepo.RestPassword(data, password);
                if (status)
                {
                    return Ok(new { data = string.Empty, message = message });
                }


                return BadRequest(new { data = string.Empty, message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
