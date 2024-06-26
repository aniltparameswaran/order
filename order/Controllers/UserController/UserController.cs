using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.DTOModel;
using order.IRepository.ICommonRepositorys;
using order.IRepository.IAdminRepositorys;
using order.IRepository.IUserRepoRepository;
using order.Repository.UserRepository;

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
        public async Task<IActionResult> UpdateUserByAdmin(string email,string phone)
        {
            try
            {

                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                if (userId == "569806b1-3379-11ef-afb3-00224dae2257")
                {
                    return Unauthorized(new { data = string.Empty, message = "Unauthorized" });
                }
                var (phone_number_exist_user_id, phone_number_message) = await _checkRepo.IsPhoneNumberExist(phone);
                if (phone_number_exist_user_id != null)
                {
                    if(userId!=phone_number_exist_user_id)
                    {
                        return BadRequest(new { data = string.Empty, message = phone_number_message });
                    }
                   
                }
                var (email_exist_user_id, email_message) = await _checkRepo.IsEmailExist(email);
                if (email_exist_user_id != null)
                {
                    if (userId != email_exist_user_id)
                    {
                        return BadRequest(new { data = string.Empty, message = email_message });
                    }
                   
                }
                var update_status = await _userRepo.UpdateUser(phone, email, userId);
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
