using Dapper;
using MimeKit;
using order.Context;
using order.DTOModel;
using order.IRepository.IUserRepository;
using order.Utils;
using Twilio.TwiML.Voice;

namespace order.Repository.UserRepository
{
    public class OrderRepo : IOrderRepo
    {
        private readonly DapperContext _dapperContext;

        public OrderRepo(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        

        public async Task<(string,string)> InsertOrder(OrderMasterDTOModel orderMasterDTOModel, string inserted_by)
        {
            var insertOrderMaster = "INSERT INTO tb_order_master(order_master_id,shop_id,total_amount,payed_amount,urgent,total_number_of_item,inserted_by)" +
                "VALUES(@orderMasterUUID,@shop_id,@total_amount,@payed_amount,@urgent,@total_number_of_item,@inserted_by);" +
                "SELECT @orderMasterUUID ";

            var insertOrderDetails = "INSERT INTO tb_order_details(order_details_id,order_master_id,product_details_id,quatity)" +
               "VALUES(@orderDetailsUUID,@order_master_id,@product_details_id,@quatity);" +
               "SELECT @orderDetailsUUID ";

            

            var deleteOrderMaster = "delete from tb_order_master where order_master_id=@order_master_id;";

            var deleteOrderDetails = "delete from tb_order_details where order_master_id=@order_master_id;";


            var deletePayment = "delete from tb_payment where payment_id=@payment_id;";


            using (var connection=_dapperContext.CreateConnection())
            {
                PaymentDTOModel paymentModel =new PaymentDTOModel();
                paymentModel.payment_no = orderMasterDTOModel.payment_no;
                paymentModel.payment_type = orderMasterDTOModel.payment_type;
                paymentModel.shop_id = orderMasterDTOModel.shop_id;
                paymentModel.total_amount = orderMasterDTOModel.total_amount;
                paymentModel.amount = orderMasterDTOModel.payed_amount;
                var paymentId= await InsertPayment(paymentModel, inserted_by);
                if(paymentId == null)
                {
                    return (null, StatusUtils.FAILED);
                }

                var orderMasterUUID= Guid.NewGuid().ToString();
                var orderMasterParameter = new DynamicParameters();
                orderMasterParameter.Add("orderMasterUUID", orderMasterUUID);
                orderMasterParameter.Add("shop_id", orderMasterDTOModel.shop_id);
                orderMasterParameter.Add("total_amount", orderMasterDTOModel.total_amount);
                orderMasterParameter.Add("payed_amount", orderMasterDTOModel.payed_amount);
                orderMasterParameter.Add("urgent", orderMasterDTOModel.urgent);
                orderMasterParameter.Add("total_number_of_item", orderMasterDTOModel.total_number_of_item);
                orderMasterParameter.Add("inserted_by", inserted_by);

                var orderMasterId=await connection.ExecuteScalarAsync<string>(insertOrderMaster, orderMasterParameter);
                if (!string.IsNullOrEmpty(orderMasterId))
                {
                    foreach(var orderDetails in orderMasterDTOModel.orderDetailsDTOModels)
                    {
                        var orderDetailsUUID = Guid.NewGuid().ToString();
                        var orderDetailsParameter = new DynamicParameters();
                        orderDetailsParameter.Add("orderDetailsUUID", orderDetailsUUID);
                        orderDetailsParameter.Add("order_master_id", orderMasterId);
                        orderDetailsParameter.Add("product_details_id", orderDetails.product_details_id);
                        orderDetailsParameter.Add("quatity", orderDetails.quatity);

                        var orderDetailsId = await connection.ExecuteScalarAsync<string>(insertOrderDetails, orderDetailsParameter);
                        if (!string.IsNullOrEmpty(orderDetailsId))
                        {

                            var (status,message) = UpdateSubProductQuatity(orderDetails.product_details_id, orderDetails.quatity).Result;
                            if (!status)
                            {
                                
                                await connection.ExecuteScalarAsync(deleteOrderMaster, new { order_master_id = orderMasterId });
                                await connection.ExecuteScalarAsync(deletePayment, new { payment_id = paymentId });
                                await connection.ExecuteScalarAsync(deleteOrderDetails, new { order_master_id = orderMasterId });

                                return (null, message);

                            }
                            
                            continue;
                        }
                        else
                        {
                            await connection.ExecuteScalarAsync(deletePayment, new { payment_id = paymentId });
                            await connection.ExecuteScalarAsync(deleteOrderMaster, new { order_master_id = orderMasterId });
                             await connection.ExecuteScalarAsync(deleteOrderDetails, new { order_master_id = orderMasterId });
                             
                             return (null, StatusUtils.FAILED);
                        }
                    }
                   
                }
                return  (orderMasterId, StatusUtils.SUCCESS); 
            }
            
        }

        public async  Task<string> InsertPayment(PaymentDTOModel paymentDTOModel, string inserted_by)
        {
            var insertPayment = "INSERT INTO tb_payment(payment_id,shop_id,amount,payment_type,payment_no,inserted_by)" +
                "VALUES(@paymentUUID,@shop_id,@amount,@payment_type,@payment_no,@inserted_by);" +
                "SELECT @paymentUUID ";

            var updateCredit = "update tb_shop_credit set creadit_amount=@creadit_amount,updated_by=@updated_by,updated_date=now() where shop_id=@shop_id;" +
                "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";

            var getCreditAmount = "select creadit_amount from tb_shop_credit where shop_id=@shop_id ;";

            using(var connection=_dapperContext.CreateConnection())
            {
                var paymentUUID = Guid.NewGuid().ToString();
                var paymentParameter = new DynamicParameters();
                paymentParameter.Add("paymentUUID", paymentUUID);
                paymentParameter.Add("shop_id", paymentDTOModel.shop_id);
                paymentParameter.Add("amount", paymentDTOModel.amount);
                paymentParameter.Add("payment_type", paymentDTOModel.payment_type);
                paymentParameter.Add("payment_no", paymentDTOModel.payment_no);
                paymentParameter.Add("inserted_by", inserted_by);

                var paymentId = await connection.ExecuteScalarAsync<string>(insertPayment, paymentParameter);
                if (!string.IsNullOrEmpty(paymentId))
                {

                    var status = await UpdateCredit(paymentDTOModel.total_amount, paymentDTOModel.amount, paymentDTOModel.shop_id, inserted_by);
                    if (status != 0)
                    {
                        return paymentId;
                    }
                    return null;

                }

                return null;

            }
           
        }

        public async Task<string> UpdateAddProductQuatity(string order_master_id)
        {
            var getListOfProductDetailIdQuery = "select product_details_id,quatity from tb_order_details where order_master_id=@order_master_id;";

            var getProductQuantity = "select available_quantity from tb_product_details where product_details_id=@product_details_id;";

            var updateProductQuantity = "Update tb_product_details set available_quantity=@available_quantity where product_details_id=@product_details_id;" +
                "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
            using (var connection = _dapperContext.CreateConnection())
            {
                var productDetails = await connection.QueryAsync(getListOfProductDetailIdQuery, new { order_master_id = order_master_id } );
                var productDetailsList= productDetails.ToList();

                if (productDetailsList.Count() > 0)
                {
                    foreach (var productDetail in productDetailsList)
                    {

                        var availableQuantity = await connection.ExecuteScalarAsync<int>(getProductQuantity,
                            new { product_details_id = productDetail.product_details_id });

                        var currenTQuantity = availableQuantity + productDetail.quatity;

                        
                        var status = await connection.ExecuteScalarAsync<int>(updateProductQuantity,
                                    new { product_details_id = productDetail.product_details_id, available_quantity = currenTQuantity });
                        if (status == 0)
                        {
                            return StatusUtils.FAILED;
                        }
                        
                    }
                    return StatusUtils.SUCCESS;
                }
                return StatusUtils.SUCCESS;

            }
        }

        public async Task<int> UpdateCredit(decimal creadit_amount, decimal payed_amount, string shop_id, string inserted_by)
        {
            var updateCredit = "update tb_shop_credit set creadit_amount=@creadit_amount,updated_by=@updated_by,updated_date=now() where shop_id=@shop_id;" +
               "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";

            var getCreditAmount = "select creadit_amount from tb_shop_credit where shop_id=@shop_id ;";

            using (var connection = _dapperContext.CreateConnection())
            {
                decimal creditAmount = await connection.QuerySingleOrDefaultAsync<decimal>(getCreditAmount, new { shop_id = shop_id });
                decimal totalCreaditAmout = creditAmount + creadit_amount;
                var balanceAmount = totalCreaditAmout - payed_amount;

                var status = await connection.ExecuteScalarAsync<int>(updateCredit, new
                {
                    creadit_amount = balanceAmount,
                    updated_by = inserted_by,
                    shop_id = shop_id

                });

                return status;

            }
        }

        public async Task<(bool,string)> UpdateSubProductQuatity(string product_details_id,int quatity)
        {
            

            var getProductQuantity = "select available_quantity from tb_product_details where product_details_id=@product_details_id;";

            var updateProductQuantity = "Update tb_product_details set available_quantity=@available_quantity where product_details_id=@product_details_id;" +
                "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
            using (var connection = _dapperContext.CreateConnection())
            {
                var availableQuantity = await connection.QueryFirstOrDefaultAsync<int>(getProductQuantity,
                             new { product_details_id = product_details_id });

                var currenTQuantity = availableQuantity - quatity;

                if(currenTQuantity < 0)
                {
                    return (false, StatusUtils.QUANTITY_NOT_AVAILABLE);
                }
                else
                {
                    var status = await connection.ExecuteScalarAsync<int>(updateProductQuantity,
                        new { product_details_id = product_details_id, available_quantity= currenTQuantity });

                    if(status>0)
                    {
                        return (true, StatusUtils.SUCCESS);
                    }
                    return (true, StatusUtils.FAILED);
                }
            }
        }
    }
}
