using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.DTOModel;
using order.IRepository.ICommonRepositorys;
using order.IRepository.IAdminRepositorys;
using order.IRepository.IUserRepoRepository;
using order.Repository.UserRepository;
using order.Utils;

namespace order.Controllers.UserController
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepo _userRepo;
        private readonly ICheckRepo _checkRepo;
        public UserController(IUserRepo userRepo, ICheckRepo checkRepo)
        {
            _userRepo = userRepo;
            _checkRepo = checkRepo;
        }
        
        [HttpPut]
        [Route("update-user")]
        public async Task<IActionResult> UpdateUser(string email,string phone)
        {
            try
            {

                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                var decryptUserId = SecurityUtils.DecryptString(userId);
                if (userIdClaimed == null || string.IsNullOrEmpty(decryptUserId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
               
                var (phone_number_exist_user_id, phone_number_message) = await _checkRepo.IsPhoneNumberExist(phone);
                if (phone_number_exist_user_id != null)
                {
                    if(decryptUserId != phone_number_exist_user_id)
                    {
                        return BadRequest(new { data = string.Empty, message = phone_number_message });
                    }
                   
                }
                var (email_exist_user_id, email_message) = await _checkRepo.IsEmailExist(email);
                if (email_exist_user_id != null)
                {
                    if (decryptUserId != email_exist_user_id)
                    {
                        return BadRequest(new { data = string.Empty, message = email_message });
                    }
                   
                }
                var update_status = await _userRepo.UpdateUser(phone, email, decryptUserId);
                if (update_status > 0)
                {
                    return Ok(new { data = string.Empty, message = "Successfully  update user" });
                }
                return BadRequest(new { data = string.Empty, message = "Fail to update user" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
