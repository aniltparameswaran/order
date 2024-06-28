using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.Context;
using order.DTOModel;
using order.IRepository.IUserRepository;
using order.Utils;

namespace order.Controllers.UserController
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepo _orderRepo;

        public OrderController(IOrderRepo orderRepo)
        {
            _orderRepo = orderRepo;
        }

        [Authorize]
        [HttpPost]
        [Route("add-order")]
        public async Task<IActionResult> InsertOrder(OrderMasterDTOModel orderMasterDTOModel)
        {
            try
            {
                var userIdClaimed = HttpContext.User.FindFirst("user_id");
                var userId = userIdClaimed.Value.ToString();
                if (userIdClaimed == null || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }
                
                var (last_inserted_id,message) = await _orderRepo.InsertOrder(orderMasterDTOModel, userId);
                if (last_inserted_id != null)
                {
                    return Ok(new { data = last_inserted_id, message = message });
                }
                return BadRequest(new { data = string.Empty, message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
    }
}
