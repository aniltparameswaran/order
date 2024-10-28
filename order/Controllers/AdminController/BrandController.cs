using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.DTOModel;
using order.IRepository.IAdminRepositorys;
using order.Repository;
using order.Repository.AdminRepository;
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
                var decryptUserId = SecurityUtils.DecryptString(userId);
                if (userIdClaimed == null || string.IsNullOrEmpty(decryptUserId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                if (decryptUserId != adminId)
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
        [Route("delete-brand")]
        public async Task<IActionResult> DeleteUser(string brand_id, int action)
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

                var decryptBrandId = SecurityUtils.EncryptString(brand_id);
                var (delete_status, message) = await _brandRepo.DeleteBrand(decryptBrandId, action);
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
        public async Task<IActionResult> UpdatebrandBybrandId(string brand_name, string brand_id)
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                var decryptUserId = SecurityUtils.DecryptString(userId);
                var decryptBrandId = SecurityUtils.DecryptString(brand_id);
                if (userIdClaimed == null || string.IsNullOrEmpty(decryptUserId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                if (decryptUserId != adminId)
                {
                    return Unauthorized(new { data = string.Empty, message = StatusUtils.UNAUTHORIZED_ACCESS });
                }

                var (brand_exist_user_id, brand_message) = await _brandRepo.IsBrandExist(brand_name);
                if (brand_exist_user_id != null)
                {
                    if (brand_exist_user_id != decryptBrandId)
                    {
                        return BadRequest(new { data = string.Empty, message = brand_message });
                    }
                }

               
                var update_status = await _brandRepo.UpdateBrandName(brand_name, decryptBrandId);
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
        [HttpGet]
        [Route("get-brand-list")]
        public async Task<IActionResult> GetProductMaster()
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

                var brand_list = await _brandRepo.GetBrand();
                if (brand_list == null)
                {
                    return NotFound(new { data = string.Empty, message = "No brand found" });
                }

                return Ok(brand_list);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
