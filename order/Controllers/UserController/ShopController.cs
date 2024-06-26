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
                var satatus = await _shopRepo.CheckShopIsExsit(shopDTOModel.lisense_number, shopDTOModel.latitude, shopDTOModel.logitude);
                if (satatus)
                {
                    return BadRequest(new { data = string.Empty, message = StatusUtils.ALREADY_SHOP_IS_ADDED });
                }
                var shp_id = await _shopRepo.InsertShop(shopDTOModel, userId);
                if (shp_id != null)
                {
                    return BadRequest(new { data = shp_id, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = shp_id, message = StatusUtils.FAILED });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
