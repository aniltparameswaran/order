using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.Context;
using order.DTOModel;
using order.IRepository.IUserRepository;
using order.Models;
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
                var decryptUserId = SecurityUtils.DecryptString(userId);
                if (userIdClaimed == null || string.IsNullOrEmpty(decryptUserId))
                {
                    return Unauthorized(new { data = string.Empty, message = "Token is invalid" });
                }


                orderMasterDTOModel.shop_id = SecurityUtils.DecryptString(orderMasterDTOModel.shop_id);

                List<OrderDetailsDTOModel> itemDeatilsList = orderMasterDTOModel.orderDetailsDTOModels;
                var itemDetails = itemDeatilsList.Select(item => new OrderDetailsDTOModel
                {
                    product_details_id = item.product_details_id != null ? SecurityUtils.DecryptString(item.product_details_id) : null,
                    quantity = item.quantity,
                }).ToList();

                orderMasterDTOModel.orderDetailsDTOModels = itemDetails;
                var (lastInsertedId, message) = await _orderRepo.InsertOrder(orderMasterDTOModel, decryptUserId);

                if (lastInsertedId != null)
                {

                    var encryptInsertedId = SecurityUtils.EncryptString(lastInsertedId);
                    return Ok(new { data = encryptInsertedId, message = message });
                }
                return BadRequest(new { data = string.Empty, message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [Authorize]
        [HttpPost]
        [Route("add-payment")]
        public async Task<IActionResult> InsertPayment(PaymentDTOModel paymentDTOModel)
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


                paymentDTOModel.shop_id = SecurityUtils.DecryptString(paymentDTOModel.shop_id);

                var lastInsertedId = await _orderRepo.InsertPayment(paymentDTOModel, decryptUserId);

                if (lastInsertedId != null)
                {

                    var encryptInsertedId = SecurityUtils.EncryptString(lastInsertedId);
                    return Ok(new { data = encryptInsertedId, message = StatusUtils.SUCCESS });
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
