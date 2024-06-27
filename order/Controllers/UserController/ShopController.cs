using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.DTOModel;
using order.IRepository.IUserRepoRepository;
using order.Repository;
using order.Utils;

namespace order.Controllers.UserController
{
    [Route("api/shop")]
    [Authorize]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly IShopRepo _shopRepo;
        public ShopController(IShopRepo shopRepo)
        {
            _shopRepo = shopRepo;
        }
        [HttpPost]
        [Route("add-shop")]
        public async Task<IActionResult> InsertShop(ShopDTOModel shopDTOModel)
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                var (shop_id,message) = await _shopRepo.CheckShopIsExsit(shopDTOModel.lisense_number, shopDTOModel.latitude, shopDTOModel.logitude);
                if (shop_id!=null)
                {
                    return BadRequest(new { data = string.Empty, message = message });
                }
                var shp_id = await _shopRepo.InsertShop(shopDTOModel, userId);
                if (shp_id != null)
                {
                    return BadRequest(new { data = shp_id, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = string.Empty, message = StatusUtils.FAILED });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /*[HttpPost]
        [Route("update-shop")]
        public async Task<IActionResult> UpdateShop(UpdateShopDTOModel shopDTOModel,string shop_id)
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                
                var shp_id = await _shopRepo.UpdateShop(shopDTOModel, userId, shop_id);
                if (shp_id != null)
                {
                    return BadRequest(new { data = shp_id, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = string.Empty, message = StatusUtils.FAILED });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }*/

        [HttpGet]
        [Route("get-shop-list")]
        public async Task<IActionResult> GetShopList()
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                var shpopList = await _shopRepo.GetShop(userId);
                if (shpopList != null)
                {
                    return BadRequest(new { data = shpopList, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = string.Empty, message = StatusUtils.FAILED });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("get-shop-details-by-shop-id")]
        public async Task<IActionResult> GetShopDetailByShopId(string shop_id)
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                var shop = await _shopRepo.GetShopDetailByShopId(shop_id,userId);
                if (shop != null)
                {
                    return BadRequest(new { data = shop, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = string.Empty, message = StatusUtils.FAILED });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
