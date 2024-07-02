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
        public async Task<IActionResult> GetProductMaster(string item_code)
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                var itemList = await _itemRepo.GetItem(item_code);
                if (itemList.Count()==0)
                {
                    return NotFound(new { data = string.Empty, message = StatusUtils.QUANTITY_NOT_AVAILABLE });
                }

                return Ok(itemList);
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
