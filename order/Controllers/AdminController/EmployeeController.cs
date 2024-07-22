using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using order.DTOModel;
using order.IRepository.ICommonRepositorys;
using order.IRepository.IAdminRepositorys;
using order.Repository.CommonRepository;
using order.Utils;

namespace order.Controllers.AdminController
{
    [Route("api/employee")]
    [ApiController]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepo _employeRepo;
        private readonly ICheckRepo _checkRepo;
        private readonly string adminId = "569806b1-3379-11ef-afb3-00224dae2257";
        public EmployeeController(IEmployeeRepo employeRepo, ICheckRepo checkRepo)
        {
            _employeRepo = employeRepo;
            _checkRepo = checkRepo;
        }
        [HttpPost]

        [Route("employee-registration")]
        public async Task<IActionResult> UserRegistration(EmployeeRegistrationDTOModel model)
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
                if (decryptUserId != adminId)
                {
                    return Unauthorized(new { data = string.Empty, message = StatusUtils.UNAUTHORIZED_ACCESS });
                }
                var (phone_number_exist_user_id, phone_number_message) = await _checkRepo.IsPhoneNumberExist(model.phone);
                if (phone_number_exist_user_id != null)
                {
                    return BadRequest(new { data = string.Empty, message = phone_number_message });
                }
                var (email_exist_user_id, email_message) = await _checkRepo.IsEmailExist(model.email);
                if (email_exist_user_id != null)
                {
                    return BadRequest(new { data = string.Empty, message = email_message });
                }
                var last_inserted_id = await _employeRepo.UserRegistration(model);
                if (last_inserted_id != null)
                {
                    var encryptInserId = SecurityUtils.EncryptString(last_inserted_id);
                    return Ok(new { data = encryptInserId, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = string.Empty, message = StatusUtils.NOT_REGISTERED });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("delete-employee")]
        public async Task<IActionResult> DeleteUser(string user_id, int action)
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
                if (decryptUserId != adminId)
                {
                    return Unauthorized(new { data = string.Empty, message = StatusUtils.UNAUTHORIZED_ACCESS });
                }
                var decrypUpdatetUserId = SecurityUtils.EncryptString(user_id);
                var (delete_status, message) = await _employeRepo.DeleteUser(decrypUpdatetUserId, action);
                if (delete_status)
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

        [Authorize]
        [HttpGet]
        [Route("get-employee-all-details")]
        public async Task<IActionResult> GetUserDetails()
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
                if (decryptUserId != adminId)
                {
                    return Unauthorized(new { data = string.Empty, message = StatusUtils.UNAUTHORIZED_ACCESS });
                }

                var user_details = await _employeRepo.GetAllUserDetails();
                if (user_details == null)
                {
                    return NotFound(new { data = string.Empty, message = "No user found" });
                }

                return Ok(user_details);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("get-employee-details-by-user-id")]
        public async Task<IActionResult> GetUserDetails(string user_id)
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
                if (decryptUserId != adminId)
                {
                    return Unauthorized(new { data = string.Empty, message = StatusUtils.UNAUTHORIZED_ACCESS });
                }
                var decrypUpdatetUserId = SecurityUtils.DecryptString(user_id);
                var user_details = await _employeRepo.GetUserDetailsByUserId(decrypUpdatetUserId);
                if (user_details == null)
                {
                    return NotFound(new { data = string.Empty, message = "User not found" });
                }

                return Ok(user_details);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut]
        [Route("update-employee-by-admin")]
        public async Task<IActionResult> UpdateUserByAdmin(EmployeeUpdateDTOModel model, string user_id)
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
                if (decryptUserId != adminId)
                {
                    return Unauthorized(new { data = string.Empty, message = "Unauthorized" });
                }
                var decrypUpdatetUserId = SecurityUtils.EncryptString(user_id);
                var update_status = await _employeRepo.UpdateEmployee(model, decrypUpdatetUserId);
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
