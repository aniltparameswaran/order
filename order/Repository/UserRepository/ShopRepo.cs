using Dapper;
using order.Context;
using order.DTOModel;
using order.Models;
using order.Utils;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;
using System;
using order.IRepository.IUserRepoRepository;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using Twilio.TwiML.Voice;

namespace order.Repository.UserRepository
{
    public class ShopRepo : IShopRepo
    {
        private readonly DapperContext _dapperContext;

        public ShopRepo(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<(string,string)> CheckShopIsExsit(string license_number, decimal latitude, decimal logitude)
        {
            var getLocation = "select shop_id from tb_shop where license_number=@license_number and is_delete=0; ";
            using (var connection = _dapperContext.CreateConnection())
            {
                const double range = 0.0001;
                var locations = await connection.QuerySingleOrDefaultAsync<string>(getLocation, new { license_number }); ;
                if (locations != null)
                {
                    return (locations, StatusUtils.ALREADY_SHOP_IS_ADDED);
                }
                return (null, StatusUtils.SHOP_NOT_EXIST);
            }

        }

        public async Task<decimal> GetCurrentBalanceByShopId(string shop_id)
        {
            var getBalanceByShopId = "select creadit_amount from tb_shop_credit where is_active=1 and shop_id=@shop_id";
            using (var connection = _dapperContext.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<decimal>(getBalanceByShopId, new { shop_id = shop_id });
            }


        }

        public async Task<IEnumerable<ShopNaameModel>> GetShop(string userId)
        {
            var getShop = "select shop_id,shop_name,address from tb_shop where is_delete=0 and ((inserted_by=@userId and updated_by is null) or updated_by=@userId)";

            using(var connection = _dapperContext.CreateConnection())
            {
                var shopList= await connection.QueryAsync<ShopNaameModel>(getShop,new { userId = userId });
                if (shopList != null)
                {

                    var shopDetails = shopList.Select(shop => new ShopNaameModel
                    {
                        shop_id = shop.shop_id != null ? SecurityUtils.EncryptString(shop.shop_id) : null,
                        shop_name = shop.shop_name,
                        address = shop.address
                    }).ToList();
                    return shopDetails;
                }
                return null;
            }
        }

        public async Task<ShopModel> GetShopDetailByShopId(string shop_id, string userId)
        {
            var getShop = "select tb_shop.shop_id,tb_shop.shop_name,tb_shop.address,tb_shop.pin,tb_shop.landmark,tb_shop.latitude,tb_shop.logitude," +
                "tb_shop.phone,tb_shop.email,tb_shop.license_number,tb_shop.is_active,tb_shop_credit.creadit_amount" +
                " from tb_shop " +
                "inner join tb_shop_credit on tb_shop_credit.shop_id=@shop_id " +
                "where tb_shop.is_delete=0 and" +
                " ((tb_shop.inserted_by=@userId and tb_shop.updated_by is null) or tb_shop.updated_by=@userId) and tb_shop.shop_id=@shop_id";
            using (var connection = _dapperContext.CreateConnection())
            {
                var shopList = await connection.QueryAsync<ShopModel>(getShop, new { userId = userId , shop_id = shop_id });
                var shopDeatils = shopList.FirstOrDefault();
                if (shopDeatils != null)
                {
                    shopDeatils.shop_id = shopDeatils.shop_id != null ? SecurityUtils.EncryptString(shopDeatils.shop_id) : null;
                    return shopDeatils;
                }
                return null;
            }


            throw new NotImplementedException();
        }

        public async Task<string> InsertCredit(string shop_id, string inserted_by)
        {
            var insertCredit = "INSERT INTO tb_shop_credit(shop_credit_id,shop_id,inserted_by)VALUES (@creditUUID,@shop_id,@inserted_by);" +
               "SELECT @creditUUID;";

            using (var connection = _dapperContext.CreateConnection())
            {
                var creditUUID = Guid.NewGuid().ToString();
                var creditParameters = new DynamicParameters();
                creditParameters.Add("creditUUID", creditUUID);
                creditParameters.Add("shop_id", shop_id);
                creditParameters.Add("inserted_by", inserted_by);
                return await connection.ExecuteScalarAsync<string>(insertCredit, creditParameters);

                
            }
        }

        public async Task<string> InsertShop(ShopDTOModel shopDTOModel, string inserted_by)
        {
            var insertShop = "INSERT INTO tb_shop" +
                 "(shop_id,shop_name, address,pin,landmark,latitude,logitude, phone, email, license_number,inserted_by)" +
                 "VALUES (@shopUUID,@shop_name,@address,@pin,@landmark,@latitude,@logitude, @phone, @email, @license_number,@inserted_by);" +
                 "SELECT @shopUUID;";

           

            var deleteShop = "delete from tb_shop where shop_id=@last_inserted_id;";
            using (var connection = _dapperContext.CreateConnection())
            {
                var shopUUID = Guid.NewGuid().ToString();
                var shopParameter = new DynamicParameters();
                shopParameter.Add("shopUUID", shopUUID);
                shopParameter.Add("shop_name", shopDTOModel.shop_name);
                shopParameter.Add("address", shopDTOModel.address);
                shopParameter.Add("email", shopDTOModel.email);
                shopParameter.Add("phone", shopDTOModel.phone);
                shopParameter.Add("pin", shopDTOModel.pin);
                shopParameter.Add("landmark", shopDTOModel.landmark);
                shopParameter.Add("latitude", shopDTOModel.latitude);
                shopParameter.Add("logitude", shopDTOModel.logitude);
                shopParameter.Add("license_number", shopDTOModel.license_number);
                shopParameter.Add("inserted_by", inserted_by);


                var last_inserted_id = await connection.ExecuteScalarAsync<string>(insertShop, shopParameter);
                var emailService = new CommunicationUtils();
                if (!string.IsNullOrEmpty(last_inserted_id))
                {

                    var credit_id = await InsertCredit(last_inserted_id, inserted_by);

                    if (!string.IsNullOrEmpty(credit_id))
                    {
                        EmailModel mail = new EmailModel();
                        mail.from_email_password = "kgwo ymcv ravu vltr";
                        mail.from_email = "aniltparameswaran@gmail.com";
                        mail.to_email = shopDTOModel.email;
                        mail.email_html_body = "<html><body><p> Hi " + shopDTOModel.shop_name + "</p><p> Thank you for parchure Ower Product is " +
                            "<br><p><strong> Thanks & Regards,</strong><br><em> " +
                            " Leadwear Team </em></p><p><em> Powered by Leadwear </em></p></body></html>";
                        mail.subject = "Shop";
                        bool status = emailService.SendMail(mail);
                        if (!status)
                        {
                            await connection.ExecuteScalarAsync(deleteShop, new { last_inserted_id = last_inserted_id });
                            return null;
                        }
                        return last_inserted_id;
                    }
                    else
                    {
                        await connection.ExecuteScalarAsync(deleteShop, new { last_inserted_id = last_inserted_id });
                        return null;
                    }
                   


                }
                return null;
            }
        }

        public async Task<int> UpdateShop(UpdateShopDTOModel shopDTOModel, string updated_by, string shop_id)
        {
            try
            {
                var user_update_query = "update tb_shop SET phone=@phone," +
                    "email=@email,updated_date=NOW(),shop_name=@shop_name,address=@address,email=@email,phone=@phone,pin=@pin" +
                    " where user_id=@user_id;" +
                    "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("shop_id", shop_id);
                    parameters.Add("shop_name", shopDTOModel.shop_name);
                    parameters.Add("address", shopDTOModel.address);
                    parameters.Add("email", shopDTOModel.email);
                    parameters.Add("phone", shopDTOModel.phone);
                    parameters.Add("pin", shopDTOModel.pin);
                    parameters.Add("landmark", shopDTOModel.lantmark);
                    parameters.Add("latitude", shopDTOModel.latitude);
                    parameters.Add("logitude", shopDTOModel.logitude);
                    parameters.Add("updated_by", updated_by);

                    var update_user = await connection.ExecuteScalarAsync<int>
                      (user_update_query, parameters);

                    return update_user;

                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while update user");
            }
        }
    }
}
