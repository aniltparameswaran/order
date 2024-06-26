using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.DTOModel;
using order.IRepository.IAdminRepositorys;
using order.Repository;
using order.Utils;

namespace order.Controllers.AdminController
{
    [Route("api/product")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepo _productRepo;
        private readonly string adminId="569806b1-3379-11ef-afb3-00224dae2257";
        public ProductController(IProductRepo productRepo)
        {
            _productRepo = productRepo;
        }

        [HttpPost]
        [Route("add-product")]
        public async Task<IActionResult> InsertProduct(ProductMasterDTOModel productMasterDTOModel)
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

                if (productMasterDTOModel.ProductDetailsListl.Count > 0)
                {
                    var (status, data) = await _productRepo.InsertProduct(productMasterDTOModel);
                    if (status)
                    {
                        return Ok(new { data, message = StatusUtils.SUCCESS });
                    }

                    return BadRequest(new { data = string.Empty, message = data });
                }
                return BadRequest(new { data = string.Empty, message = StatusUtils.NOT_REGISTERED });

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut]
        [Route("update-product-master")]
        public async Task<IActionResult> UpdateProductMaster(ProductMasterUpdateDTOModel model, string product_master_id)
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
                var update_status = await _productRepo.UpdateProductMaster(model, product_master_id);
                if (update_status > 0)
                {
                    return Ok(new { data = string.Empty, message = "Successfully  update product master" });
                }
                return BadRequest(new { data = string.Empty, message = "Fail to update product master" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("update-product-details")]
        public async Task<IActionResult> UpdateProductDetail(ProductDetailsUpdateDTOModel model, string product_details_id)
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
                var update_status = await _productRepo.UpdateProductDetail(model, product_details_id);
                if (update_status > 0)
                {
                    return Ok(new { data = string.Empty, message = "Successfully  update product detail" });
                }
                return BadRequest(new { data = string.Empty, message = "Fail to update product detail" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("delete-product-master")]
        public async Task<IActionResult> DeleteProductMaster(string product_master_id, int action)
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
                var (delete_status, message) = await _productRepo.DeleteProductMaster(product_master_id, action);
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

        [HttpDelete]
        [Route("delete-product-details")]
        public async Task<IActionResult> DeleteProductDetail(string product_details_id, int action)
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
                var (delete_status, message) = await _productRepo.DeleteProductDetail(product_details_id, action);
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


        [HttpGet]
        [Route("get-product-master-list")]
        public async Task<IActionResult> GetProductMaster()
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

                var product_master_list = await _productRepo.GetProductMaster();
                if (product_master_list == null)
                {
                    return NotFound(new { data = string.Empty, message = "No user found" });
                }

                return Ok(product_master_list);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("get-product--master-details-by-master-id")]
        public async Task<IActionResult> GetProductDetailByMasterId(string product_master_id)
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

                var product_master_deatils = await _productRepo.GetProductDetailByMasterId(product_master_id);
                if (product_master_deatils == null)
                {
                    return NotFound(new { data = string.Empty, message = "No product found" });
                }

                return Ok(product_master_deatils);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
