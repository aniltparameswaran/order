using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.DTOModel;
using order.IRepository.IUserRepoRepository;
using order.IRepository.IUserRepository;
using order.Utils;

namespace order.Controllers.UserController
{
    [Route("api/item")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemRepo _itemRepo;
        public ItemController(IItemRepo itemRepo)
        {
            _itemRepo = itemRepo;
        }
        [HttpGet]
        [Route("get-item-list")]
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
                var product_master_list = await _itemRepo.GetItem();
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
        [Route("check-quantity-vailable")]
        public async Task<IActionResult> CheckQuantityAvailable(OrderDetailsDTOModel item)
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                var (status,massage) = await _itemRepo.CheckQuantity(item);
                return NotFound(new { data = string.Empty, message = massage });
                

                
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

    }
}
