﻿using Dapper;
using MimeKit;
using order.Context;
using order.DTOModel;
using order.IRepository.IUserRepository;

namespace order.Repository.UserRepository
{
    public class OrderRepo : IOrderRepo
    {
        private readonly DapperContext _dapperContext;

        public OrderRepo(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }
        public async Task<string> InsertOrder(OrderMasterDTOModel orderMasterDTOModel, string inserted_by)
        {
            var insertOrderMaster = "INSERT INTO tb_order_master(order_master_id,shop_id,total_amount,payed_amount,urgent,total_number_of_item,inserted_by)" +
                "VALUES(@orderMasterUUID,@shop_id,@total_amount,@payed_amount,@urgent,@total_number_of_item,@inserted_by);" +
                "SELECT @orderMasterUUID ";

            var insertOrderDetails = "INSERT INTO tb_order_details(order_details_id,order_master_id,product_details_id,quatity)" +
               "VALUES(@orderDetailsUUID,@order_master_id,@product_details_id,@quatity);" +
               "SELECT @orderDetailsUUID ";

            

            var deleteOrderMaster = "delete from tb_order_master where order_master_id=@order_master_id;";

            var deleteOrderDetails = "delete from tb_order_details where order_master_id=@order_master_id;";
            using (var connection=_dapperContext.CreateConnection())
            {
                PaymentDTOModel paymentModel =new PaymentDTOModel();
                paymentModel.payment_no = orderMasterDTOModel.payment_no;
                paymentModel.payment_type = orderMasterDTOModel.payment_type;
                paymentModel.shop_id = orderMasterDTOModel.shop_id;
                paymentModel.total_amount = orderMasterDTOModel.total_amount;
                paymentModel.amount = orderMasterDTOModel.payed_amount;
                var payment_id= await InsertPayment(paymentModel, inserted_by);
                if(payment_id == null)
                {
                    return null;
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

                var order_master_id=await connection.ExecuteScalarAsync<string>(insertOrderMaster, orderMasterParameter);
                if (!string.IsNullOrEmpty(order_master_id))
                {
                    foreach(var orderDetails in orderMasterDTOModel.orderDetailsDTOModels)
                    {
                        var orderDetailsUUID = Guid.NewGuid().ToString();
                        var orderDetailsParameter = new DynamicParameters();
                        orderDetailsParameter.Add("orderDetailsUUID", orderDetailsUUID);
                        orderDetailsParameter.Add("order_master_id", order_master_id);
                        orderDetailsParameter.Add("product_details_id", orderDetails.product_details_id);
                        orderDetailsParameter.Add("quatity", orderDetails.quatity);

                        var order_details_id = await connection.ExecuteScalarAsync<string>(insertOrderDetails, orderDetailsParameter);
                        if (!string.IsNullOrEmpty(order_details_id))
                        {
                            continue;
                        }
                        else
                        {
                            await connection.ExecuteScalarAsync(deleteOrderMaster, new { order_master_id = order_master_id });
                            await connection.ExecuteScalarAsync(deleteOrderDetails, new { order_master_id = order_master_id });
                            return null;
                        }
                    }
                   
                }
                return order_master_id;
            }
            ;
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

                var payment_id = await connection.ExecuteScalarAsync<string>(insertPayment, paymentParameter);
                if (!string.IsNullOrEmpty(payment_id))
                {
                    decimal creditAmount = await connection.QuerySingleOrDefaultAsync<decimal>(getCreditAmount, new { shop_id = paymentDTOModel.shop_id });
                    decimal totalCreaditAmout = creditAmount + paymentDTOModel.total_amount;
                    var balanceAmount = totalCreaditAmout - paymentDTOModel.amount;

                    var status = await connection.ExecuteScalarAsync<int>(updateCredit, new
                    {
                        creadit_amount = balanceAmount,
                        updated_by = inserted_by,
                        shop_id = paymentDTOModel.shop_id

                    }) ; 
                    if(status != 0)
                    {
                        return payment_id;
                    }
                    return null;

                }

                return null;



            }
           
        }
    }
}