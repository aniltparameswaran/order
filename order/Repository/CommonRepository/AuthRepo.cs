using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using order.Context;
using order.DTOModel;
using order.IRepository.ICommonRepositorys;
using order.Models;
using order.Utils;
using System.Data;
using System.Text.RegularExpressions;
using static order.Utils.SecurityUtils;

namespace order.Repository.CommonRepository
{
    public class AuthRepo : IAuthRepo
    {
        private readonly DapperContext _dapperContext;
        private readonly IConfiguration _configuration;
        private readonly string adminId= "569806b1-3379-11ef-afb3-00224dae2257";
        public AuthRepo(DapperContext dapperContext, IConfiguration configuration)
        {
            _dapperContext = dapperContext;
            _configuration = configuration;
        }
        public bool IsEmail(string input)
        {
            return Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
        public async Task<(bool, string)> ForgotPassword(string userName)
        {
            string query = "SELECT user_id,user_name,email FROM tb_user WHERE ";

            using (var connection = _dapperContext.CreateConnection())
            {
                var otp = StringUtils.GenerateRandomOTP(4);
                var userId = "";
                var parameter = new DynamicParameters();

                if (IsEmail(userName))
                {
                    query += " email = @username";
                    parameter.Add("username", userName, DbType.String);
                }
                else
                {
                    query += " phone = @username";
                    parameter.Add("username", userName, DbType.String);
                }
                var emailService = new CommunicationUtils();

                var userDeatils = await connection.QueryFirstOrDefaultAsync(query, parameter);
                if (userDeatils != null)
                {
                    userId = userDeatils.user_id;
                }
                else
                {
                    return (false, StatusUtils.USER_NOT_FOUND);
                }

                if (userId != null)
                {

                    var encrypted_otp = EncryptModel(otp, userId.ToString());
                    var name = userDeatils.user_name;
                    EmailModel mail = new EmailModel();
                    mail.from_email_password = "kgwo ymcv ravu vltr";
                    mail.from_email = "aniltparameswaran@gmail.com";
                    mail.to_email = userDeatils.email;
                    mail.email_html_body = "<html><body><p> Hi " + name + "</p><p> Your OTP is " + otp +
                        "<br> Don`t share the otp.</p><p><strong> Thanks & Regards,</strong><br><em> " +
                        " Leadwear Team </em></p><p><em> Powered by Leadwear </em></p></body></html>";
                    mail.subject = "Your Passward";
                    bool status = emailService.SendMail(mail);
                    if (status)
                    {
                        return (true, encrypted_otp);
                    }
                    return (false, StatusUtils.FAILED);
                }
                return (false, StatusUtils.INVALID_EMAIL);
            }
            throw new NotImplementedException();
        }

        public async Task<(bool, string)> Login(LoginDTOModel loginDTOModel, int adminOrNot)
        {
            var getQuery = "select password,email,user_id from tb_user where is_delete=0";

            using (var connection = _dapperContext.CreateConnection())
            {
                if (connection == null)
                {
                    throw new InvalidOperationException("Database connection cannot be created");
                }
                var parameter = new DynamicParameters();

                if (IsEmail(loginDTOModel.user_name))
                {
                    getQuery += " and email = @username";
                    parameter.Add("username", loginDTOModel.user_name, DbType.String);
                }
                else
                {
                    getQuery += " and phone = @username";
                    parameter.Add("username", loginDTOModel.user_name, DbType.String);
                }
                var userDetails = await connection.QueryFirstOrDefaultAsync(getQuery, parameter);
                if (userDetails != null)
                {
                    var encryptPasswordtoDecrpt = userDetails.password;
                    var decryPassword = SecurityUtils.DecryptString(encryptPasswordtoDecrpt.ToString());
                    if (loginDTOModel.password + userDetails.email == decryPassword)
                    {
                        var tokenUtilities = new TokenUtil(_configuration);
                        if (adminOrNot == 1)
                        {
                            if (userDetails.user_id == adminId )
                            {
                                var token = tokenUtilities.GetToken(userDetails.user_id);

                                if (token != null)
                                {
                                    return (true, token);

                                }
                                return (false, StatusUtils.FAILED);
                            }
                            return (false, StatusUtils.UNAUTHORIZED_ACCESS);

                        }
                        else if (adminOrNot == 0)
                        {

                            if (userDetails.user_id != "569806b1-3379-11ef-afb3-00224dae2257")
                            {
                                var token = tokenUtilities.GetToken(userDetails.user_id);

                                if (token != null)
                                {
                                    return (true, token);

                                }
                                return (false, StatusUtils.FAILED);
                            }
                            return (false, StatusUtils.UNAUTHORIZED_ACCESS);


                        }
                        return (false, StatusUtils.UNAUTHORIZED_ACCESS);

                    }
                    return (false, StatusUtils.INVALID_PASSWORD);

                }
                else
                {
                    return (false, StatusUtils.INVALID_PHONE_OR_EMAIL);
                }

                return (false, StatusUtils.FAILED);
            }
        }


        public async Task<(bool, string)> RestPassword(string data, string password)
        {
            var updateQuery = "update tb_user set password=@password where user_id=@userId;" +
                "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
            var getEmail = "select email from tb_user where user_id=@userId";
            var user_id = DecryptString(data);
            if (user_id != null)
            {
                using (var connection = _dapperContext.CreateConnection())
                {


                    var email = await connection.QueryAsync<string>(getEmail, new { userId = user_id });
                    var encryptPassword = EncryptString(password + email.FirstOrDefault());
                    var status = await connection.ExecuteScalarAsync<int>(updateQuery, new { password = encryptPassword, userId = user_id });
                    if (status > 0)
                    {
                        return (true, StatusUtils.SUCCESS);
                    }
                    return (false, StatusUtils.UPDATION_FAILED);


                }
            }
            return (false, StatusUtils.UNAUTHORIZED_ACCESS);

        }

        public async Task<(bool, string)> VarificationOtp(string data, int otp)
        {
            EncryptvalueModel decryptedData = DecryptModel(data);
            if (decryptedData != null)
            {

                var timeDifference = DateTime.Now - decryptedData.CurrentDate;
                if (timeDifference.TotalMinutes < 5)
                {
                    if (decryptedData.value == otp.ToString())
                    {
                        return (true, EncryptString(decryptedData.userDeatils));
                    }
                    return (false, StatusUtils.INVALID_OTP);
                }
                return (false, StatusUtils.OPT_EXPIRED);
            }
            return (false, StatusUtils.UNAUTHORIZED_ACCESS);

        }

    }
}
