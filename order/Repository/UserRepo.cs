using Dapper;
using order.Context;
using order.DTOModel;
using order.IRepository;
using order.Models;
using order.Utils;
using System.Data;

namespace order.Repository
{
    public class UserRepo :IUserRepo
    {
        private readonly DapperContext _dapperContext;
        private readonly SecurityUtils _securityUtils;

        public UserRepo(DapperContext dapperContext, SecurityUtils securityUtils)
        {
            _dapperContext = dapperContext;
            _securityUtils = securityUtils;
        }
        public async Task<int> UserRegistration(UserRegistrationDTOModel model)
        {
            try
            {

                var user_detail_insert_query = "INSERT INTO tb_user" +
                 "(user_name, address, phone, email, pin,adhaaar_no, password)" +
                 "VALUES (@user_name, @address, @phone, @email, @pin, @adhaaar_no, @password);" +
                 "SELECT LAST_INSERT_ID();";

                using (var connection = _dapperContext.CreateConnection())
                {
                    var password = StringUtils.GenerateRandomString(7);
                    var encrypted_password = SecurityUtils.EncryptString(password+ model.email);
                    var parameter = new DynamicParameters();
                    parameter.Add("user_name", model.user_name);
                    parameter.Add("address", model.address);
                    parameter.Add("email", model.email);
                    parameter.Add("phone", model.phone);
                    parameter.Add("pin", model.pin);
                    parameter.Add("adhaaar_no", model.adhaaar_no);
                    parameter.Add("password", encrypted_password);

                    var last_inserted_id = await connection.ExecuteScalarAsync<int>(user_detail_insert_query, parameter);
                    var emailService = new CommunicationUtils();
                    if (last_inserted_id > 0)
                    {
                        EmailModel mail = new EmailModel();
                        mail.from_email_password = "kgwo ymcv ravu vltr";
                        mail.from_email = "aniltparameswaran@gmail.com";
                        mail.to_email=model.email;
                        mail.email_html_body ="<html><body><p> Hi "+ model.user_name + "</p><p> Your Password is " + password+
                            "<br> Don`t share the Password.</p><p><strong> Thanks & Regards,</strong><br><em> " +
                            " Leadwear Team </em></p><p><em> Powered by Leadwear </em></p></body></html>";
                        mail.subject = "Yor Passward";
                        bool status = emailService.SendMail(mail);
                        return last_inserted_id;
                    }
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while user registration");
            }

        }
        public async Task<(int, string)> IsEmailExist(string email)
        {
            try
            {
                var query = "select user_id from tb_user where email = @email";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("email", email);
                    var user_id = await connection.QuerySingleOrDefaultAsync<int>(query, parameters);
                    if (user_id != 0)
                    {
                        return (user_id, StatusUtils.EMAIL_ALREADY_EXIST);
                    }
                    return (0, StatusUtils.EMAIL_NOT_EXIST);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while Email checking");
            }
        }
        public async Task<(int, string)> IsPhoneNumberExist(string phone)
        {
            try
            {
                var query = "select user_id from tb_user where phone=@phone;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("phone", phone);
                    var user_id = await connection.ExecuteScalarAsync<int>(query, parameters);
                    if (user_id != 0)

                    {
                        return (user_id, StatusUtils.PHONE_NUMBER_ALREADY_EXIST);
                    }
                    return (0, StatusUtils.PHONE_NUMBER_NOT_EXIST);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while phone number checking");
            }
        }

        public async Task<(bool,string)> DeleteUser(int userId, int action)
        {
            try
            {
                bool execute = false;
                var deleteQuery = $"UPDATE tb_user SET updated_date =NOW(), ";

                if (action == 0)
                {
                    execute = true;
                    deleteQuery += "is_active = 0 WHERE user_id = @userId;";
                }
                else if (action == 1)
                {
                    execute = true;
                    deleteQuery += "is_active = 1 WHERE user_id = @userId;";
                }
                else if (action == 2)
                {
                    execute = true;
                    deleteQuery += "is_delete = 1,is_active = 0 WHERE user_id = @userId;";
                }
                if (execute)
                {
                    using (var connection = _dapperContext.CreateConnection())
                    {
                        deleteQuery += "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
                        var parameters = new DynamicParameters();
                        parameters.Add("userId", userId);
                        parameters.Add("updatedBy", userId);
                        var status = await connection.ExecuteAsync(deleteQuery, parameters);
                        if (status > 0)
                        {
                            if(action == 0)
                            {
                                return (true, StatusUtils.IS_ACTIVE_UPDATEDTION_SUCCESS);
                            }
                            else if (action == 1)
                            {
                                return (true, StatusUtils.IS_ACTIVE_UPDATEDTION_SUCCESS);
                            }
                            else if (action == 2)
                            {
                                return (true, StatusUtils.IS_DELETE_UPDATEDTION_SUCCESS);

                            }
                            
                        }
                    }
                }
                return  (false, StatusUtils.UPDATION_FAILED); ;

            }
            catch (Exception ex)
            {

                throw new Exception("Error occurred while deleting user.", ex);
            }
        }
        public async Task<UserdetailsByIdModel> GetUserDetailsByUserId(int user_id)
        {
            try
            {
                var user_details_query = "select user_name,email,phone," +
                    "address,pin,adhaaar_no,is_active  from tb_user where user_id=@user_id && is_delete = 0;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var user_details = await connection.QuerySingleOrDefaultAsync<UserdetailsByIdModel>(user_details_query, new
                    {
                        user_id = user_id
                    });
                    return user_details;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while retrieve user details");
            }
        }
        public async Task<UserdetailsModel> GetAllUserDetails()
        {
            try
            {
                var user_details_query = "select user_id,user_name,email,phone," +
                    "address,pin,adhaaar_no,is_active  from tb_user where is_delete = 0 and user_id!=1;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var user_details = await connection.QuerySingleOrDefaultAsync<UserdetailsModel>(user_details_query);
                    return user_details;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while retrieve user details");
            }
        }
        public async Task<int> UpdateUserByAdmin(UserUpdateDTOModel model, int user_id)
        {
            try
            {
                var user_update_query = "update tb_user SET user_name=@user_name," +
                    "pin=@pin,adhaaar_no=@adhaaar_no,address=@address,updated_date=NOW() where user_id=@user_id;" +
                    "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("user_name", model.user_name);
                    parameters.Add("address", model.address);
                    parameters.Add("user_id", user_id);
                    parameters.Add("pin", model.pin);
                    parameters.Add("adhaaar_no", model.adhaaar_no);

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
        public async Task<int> UpdateUserByUser(string phone, string email, int user_id)
        {
            try
            {
                var user_update_query = "update tb_user SET phone=@phone," +
                    "email=@email,updated_date=NOW() where user_id=@user_id;" +
                    "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("phone", phone);
                    parameters.Add("email", email);
                    parameters.Add("user_id", user_id);

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
