using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.DTOModel;
using order.IRepository;
using order.Repository;
using order.Utils;

namespace order.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private  readonly IProductRepo _productRepo;
        public ProductController(IProductRepo productRepo)
        {
            _productRepo= productRepo;
        }

        [HttpPost]
        public async Task<IActionResult> InsertProduct(ProductMasterDTOModel productMasterDTOModel)
        {
            try
            {
                /* var userIdClaim = HttpContext.User.FindFirst("user_id");
                 if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                 {
                     return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                 }
                 if (userId != 1)
                 {
                     return Unauthorized(new { data = string.Empty, message = "Unauthorized access" });
                 }
                 var (brand_exist_user_id, brand_message) = await _brandRepo.IsBrandExist(brand_name);
                 if (brand_exist_user_id != 0)
                 {
                     return BadRequest(new { data = string.Empty, message = brand_message });
                 }*/
                if(productMasterDTOModel.ProductDetailsListl.Count >0)
                {
                    var last_inserted_id = await _productRepo.InsertProduct(productMasterDTOModel);
                    if (last_inserted_id != "")
                    {
                        return Ok(new { data = last_inserted_id, message = StatusUtils.SUCCESS });
                    }
                   
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
                /*var userIdClaim = HttpContext.User.FindFirst("user_id");
                 if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                 {
                     return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                 }
                 if (userId != 1)
                 {
                     return Unauthorized(new { data = string.Empty, message = "Unauthorized access" });
                 }*/
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
                /*var userIdClaim = HttpContext.User.FindFirst("user_id");
                 if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                 {
                     return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                 }
                 if (userId != 1)
                 {
                     return Unauthorized(new { data = string.Empty, message = "Unauthorized access" });
                 }*/
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
                /*var userIdClaim = HttpContext.User.FindFirst("user_id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                if (userId != 1)
                {
                    return Unauthorized(new { data = string.Empty, message = "Unauthorized access" });
                }*/
                var (delete_status, message) = await _productRepo.DeleteProductMaster(product_master_id, action);
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

        [HttpDelete]
        [Route("delete-product-details")]
        public async Task<IActionResult> DeleteProductDetail(string product_details_id, int action)
        {
            try
            {
                /*var userIdClaim = HttpContext.User.FindFirst("user_id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                if (userId != 1)
                {
                    return Unauthorized(new { data = string.Empty, message = "Unauthorized access" });
                }*/
                var (delete_status, message) = await _productRepo.DeleteProductDetail(product_details_id, action);
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


        [HttpGet]
        [Route("get-product-master-list")]
        public async Task<IActionResult> GetProductMaster()
        {
            try
            {
                /*var userIdClaim = HttpContext.User.FindFirst("user_id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                if (userId != 1)
                {
                    return Unauthorized(new { data = string.Empty, message = "Unauthorized access" });
                }*/

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
                /*var userIdClaim = HttpContext.User.FindFirst("user_id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                if (userId != 1)
                {
                    return Unauthorized(new { data = string.Empty, message = "Unauthorized access" });
                }*/

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
