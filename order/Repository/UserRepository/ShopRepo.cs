using Dapper;
using order.Context;
using order.DTOModel;
using order.Models;
using order.Utils;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;
using System;
using order.IRepository.IUserRepoRepository;

namespace order.Repository.UserRepository
{
    public class ShopRepo : IShopRepo
    {
        private readonly DapperContext _dapperContext;

        public ShopRepo(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<bool> CheckShopIsExsit(string lisense_number, decimal latitude, decimal logitude)
        {
            var getLocation = "select shop_id from tb_shop where lisense_number=@lisense_number and is_delete=0; ";
            using (var connection = _dapperContext.CreateConnection())
            {
                const double range = 0.0001;
                var locations = await connection.QuerySingleOrDefaultAsync<string>(getLocation, new { lisense_number }); ;
                if (locations != null)
                {
                    return true;
                }
                return false;
            }

        }

        public async Task<string> InsertShop(ShopDTOModel shopDTOModel, string inserted_by)
        {
            var insertShop = "INSERT INTO tb_shop" +
                 "(shop_id,shop_name, address,pin,lantmark,latitude,logitude, phone, email, lisense_number,inserted_by)" +
                 "VALUES (@shopUUID,@shop_name,@address,@pin,@lantmark,@latitude,@logitude, @phone, @email, @lisense_number,@inserted_by);" +
                 "SELECT @shopUUID;";

            var deleteShop = "delete from tb_shop where shop_id=@last_inserted_id";
            using (var connection = _dapperContext.CreateConnection())
            {
                var shopUUID = Guid.NewGuid().ToString();
                var parameter = new DynamicParameters();
                parameter.Add("shopUUID", shopUUID);
                parameter.Add("shop_name", shopDTOModel.shop_name);
                parameter.Add("address", shopDTOModel.address);
                parameter.Add("email", shopDTOModel.email);
                parameter.Add("phone", shopDTOModel.phone);
                parameter.Add("pin", shopDTOModel.pin);
                parameter.Add("lantmark", shopDTOModel.lantmark);
                parameter.Add("latitude", shopDTOModel.latitude);
                parameter.Add("logitude", shopDTOModel.logitude);
                parameter.Add("lisense_number", shopDTOModel.lisense_number);
                parameter.Add("inserted_by", inserted_by);


                var last_inserted_id = await connection.ExecuteScalarAsync<string>(insertShop, parameter);
                var emailService = new CommunicationUtils();
                if (!string.IsNullOrEmpty(last_inserted_id))
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
                    }
                    return last_inserted_id;

                }
                return null;
            }
        }

        public async Task<string> UpdateShop(UpdateShopDTOModel shopDTOModel, string updated_by, string shop_id)
        {
            try
            {
                var user_update_query = "update tb_shop SET phone=@phone," +
                    "email=@email,updated_date=NOW(),shop_name=@shop_name,address=@address,email=@email,phone=@phone,pin=@pin" +
                    " where user_id=@user_id;" +
                    "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
                using (var connection = _dapperContext.CreateConnection())
                {
                   
                    var parameter = new DynamicParameters();
                    parameter.Add("shop_id", shop_id);
                    parameter.Add("shop_name", shopDTOModel.shop_name);
                    parameter.Add("address", shopDTOModel.address);
                    parameter.Add("email", shopDTOModel.email);
                    parameter.Add("phone", shopDTOModel.phone);
                    parameter.Add("pin", shopDTOModel.pin);
                    parameter.Add("lantmark", shopDTOModel.lantmark);
                    parameter.Add("latitude", shopDTOModel.latitude);
                    parameter.Add("logitude", shopDTOModel.logitude);
                    parameter.Add("updated_by", updated_by);
                    

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
