using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.DTOModel;
using order.IRepository.IAdminRepositorys;
using order.Repository;
using order.Utils;

namespace order.Controllers.AdminController
{
    [Authorize]
    [Route("api/brand")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IBrandRepo _brandRepo;
        private readonly string adminId = "569806b1-3379-11ef-afb3-00224dae2257";
        public BrandController(IBrandRepo brandRepo)
        {
            _brandRepo = brandRepo;
        }
        [HttpPost]
        [Route("add-brand")]
        public async Task<IActionResult> InsertBrand(string brand_name)
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                if (userId != adminId)
                {
                    return Unauthorized(new { data = string.Empty, message = StatusUtils.UNAUTHORIZED_ACCESS });
                }
                var (brand_exist_user_id, brand_message) = await _brandRepo.IsBrandExist(brand_name);
                if (brand_exist_user_id != null)
                {
                    return BadRequest(new { data = string.Empty, message = brand_message });
                }

                var last_inserted_id = await _brandRepo.InsertBrand(brand_name);
                if (last_inserted_id != "")
                {
                    return Ok(new { data = last_inserted_id, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = string.Empty, message = StatusUtils.NOT_REGISTERED });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("delete-brand")]
        public async Task<IActionResult> DeleteUser(string brand_id, int action)
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                if (userId != adminId)
                {
                    return Unauthorized(new { data = string.Empty, message = StatusUtils.UNAUTHORIZED_ACCESS });
                }
                var (delete_status, message) = await _brandRepo.DeleteBrand(brand_id, action);
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


        [HttpPut]
        [Route("update-brand")]
        public async Task<IActionResult> UpdateUserByUser(string brand_name, string brand_id)
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                if (userId != adminId)
                {
                    return Unauthorized(new { data = string.Empty, message = StatusUtils.UNAUTHORIZED_ACCESS });
                }

                var (brand_exist_user_id, brand_message) = await _brandRepo.IsBrandExist(brand_name);
                if (brand_exist_user_id != null)
                {
                    if (brand_exist_user_id != brand_id)
                    {
                        return BadRequest(new { data = string.Empty, message = brand_message });
                    }
                }


                var update_status = await _brandRepo.UpdateBrandName(brand_name, brand_id);
                if (update_status > 0)
                {
                    return Ok(new { data = string.Empty, message = "Successfully  update brand" });
                }


                return BadRequest(new { data = string.Empty, message = "Fail to update brand" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
