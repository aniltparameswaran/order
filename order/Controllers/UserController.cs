﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using order.DTOModel;
using order.IRepository;
using order.Utils;

namespace order.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepo _userRepo;
        public UserController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }
        [HttpPost]
        public async Task<IActionResult> UserRegistration(UserRegistrationDTOModel model)
        {
            try
            {

                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                var (phone_number_exist_user_id, phone_number_message) = await _userRepo.IsPhoneNumberExist(model.phone);
                if (phone_number_exist_user_id != null)
                {
                    return BadRequest(new { data = string.Empty, message = phone_number_message });
                }
                var request_status = await _userRepo.UserRegistration(model);
                if (request_status !=null)
                {
                    return Ok(new { data = string.Empty, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = string.Empty, message = StatusUtils.NOT_REGISTERED });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(string user_id, int action)
        {
            try
            {

                var (delete_status, message) = await _userRepo.DeleteUser(user_id, action);
                if (delete_status)
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

        [Authorize]
        [HttpGet]
        [Route("get-user-all-details")]
        public async Task<IActionResult> GetUserDetails()
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                if (userId != "569806b1-3379-11ef-afb3-00224dae2257")
                {
                    return Unauthorized(new { data = string.Empty, message = StatusUtils.UNAUTHORIZED_ACCESS });
                }

                var user_details = await _userRepo.GetAllUserDetails();
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
        [Route("get-user-details-by-user-id")]
        public async Task<IActionResult> GetUserDetails(string user_id)
        {
            try
            {
                 var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
               


                var user_details = await _userRepo.GetUserDetailsByUserId(user_id);
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
        [Route("update-user-by-admin")]
        public async Task<IActionResult> UpdateUserByAdmin(UserUpdateDTOModel model, string user_id)
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
                var update_status = await _userRepo.UpdateUserByAdmin(model, user_id);
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

        [Authorize]
        [HttpPut]
        [Route("update-user-by-user")]
        public async Task<IActionResult> UpdateUserByUser(string phone, string email)
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }

                var (phone_number_exist_user_id, phone_number_message) = await _userRepo.IsPhoneNumberExist(phone);
                if (phone_number_exist_user_id != null)
                {
                    if (phone_number_exist_user_id != userId)
                    {
                        return BadRequest(new { data = string.Empty, message = phone_number_message });
                    }
                }

                var (email_exist_user_id, email_message) = await _userRepo.IsEmailExist(email);
                if (email_exist_user_id != null)
                {
                    if (email_exist_user_id != userId)
                    {
                        return BadRequest(new { data = string.Empty, message = email_message });
                    }
                }
                var update_status = await _userRepo.UpdateUserByUser(phone, email, userId);
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
