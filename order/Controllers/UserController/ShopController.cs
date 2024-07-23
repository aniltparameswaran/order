using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.DTOModel;
using order.IRepository.IUserRepoRepository;
using order.Repository;
using order.Utils;

namespace order.Controllers.UserController
{
    [Authorize]
    [Route("api/shop")]
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
                var decryptUserId = SecurityUtils.DecryptString(userId);
                if (userIdClaimed == null || string.IsNullOrEmpty(decryptUserId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                var (shopId,message) = await _shopRepo.CheckShopIsExsit(shopDTOModel.lisense_number, shopDTOModel.latitude, shopDTOModel.logitude);
                if (shopId != null)
                {
                    var decryptshopId = SecurityUtils.EncryptString(shopId);
                    return BadRequest(new { data = shopId, message = message });
                }
                var shp_id = await _shopRepo.InsertShop(shopDTOModel, decryptUserId);
                if (shp_id != null)
                {

                    return Ok(new { data = shp_id, message = StatusUtils.SUCCESS });
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
                var decryptUserId = SecurityUtils.DecryptString(userId);
                if (userIdClaimed == null || string.IsNullOrEmpty(decryptUserId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                var shpopList = await _shopRepo.GetShop(decryptUserId);
                if (shpopList != null)
                {
                    return Ok(new { data = shpopList, message = StatusUtils.SUCCESS });
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
                var decryptUserId = SecurityUtils.DecryptString(userId);
                if (userIdClaimed == null || string.IsNullOrEmpty(decryptUserId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }

                shop_id = SecurityUtils.DecryptString(shop_id);
                var shop = await _shopRepo.GetShopDetailByShopId(shop_id, decryptUserId);
                if (shop != null)
                {
                    return Ok(new { data = shop, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = string.Empty, message = StatusUtils.FAILED });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet]
        [Route("get-current-balance-by-shop-id")]
        public async Task<IActionResult> GetCurrentBalanceByShopId(string shop_id)
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

                shop_id = SecurityUtils.DecryptString(shop_id);
                var balance = await _shopRepo.GetCurrentBalanceByShopId(shop_id);

                return Ok(new { data = balance, message = StatusUtils.SUCCESS });
                
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
