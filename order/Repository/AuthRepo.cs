using Dapper;
using Newtonsoft.Json;
using order.Context;
using order.IRepository;
using order.Models;
using order.Utils;
using System.Data;
using System.Text.RegularExpressions;

namespace order.Repository
{
    public class AuthRepo : IAuthRepo
    {
        private readonly DapperContext _dapperContext;
        private readonly IConfiguration _configuration;
        private readonly SecurityUtils _securityUtils;
        public AuthRepo(DapperContext dapperContext, IConfiguration configuration, SecurityUtils securityUtils)
        {
            _dapperContext = dapperContext;
            _configuration = configuration;
            _securityUtils = securityUtils;
        }

        public async Task<(bool, string)> ForgotPassword(string? email, string? phone)
        {
            var getEmailQuery = "select email from tb_user where phone=@phone";
            var getQuery = "select user_name from tb_user where email=@email";
            using(var connection=_dapperContext.CreateConnection())
            {
                var otp = StringUtils.GenerateRandomOTP(4);

                
                var emailService = new CommunicationUtils();
                if (phone != null)
                {
                    var emailOfUser= await connection.QueryAsync<string>(getEmailQuery, new { phone });
                    if (emailOfUser != null)
                    {
                        email = emailOfUser.FirstOrDefault();
                    }
                    else
                    {
                        return (false, StatusUtils.USER_NOT_FOUND);
                    }
                }
                if (email != null)
                {
                    VarificationModel data = new VarificationModel();
                    data.otp = int.Parse(otp);
                    data.currentDateTime = DateTime.Now;
                    data.userDeatils = email.ToString();
                    var jsonData = JsonConvert.SerializeObject(data);
                    var encrypted_otp = _securityUtils.Encrypt(jsonData);

                    var userNmae = await connection.QueryAsync<string>(getEmailQuery, new { phone });
                    var name = userNmae.FirstOrDefault();
                    EmailModel mail = new EmailModel();
                    mail.from_email_password = "kgwo ymcv ravu vltr";
                    mail.from_email = "aniltparameswaran@gmail.com";
                    mail.to_email = email;
                    mail.email_html_body = "<html><body><p> Hi " + name + "</p><p> Your OTP is " + otp +
                        "<br> Don`t share the otp.</p><p><strong> Thanks & Regards,</strong><br><em> " +
                        " Leadwear Team </em></p><p><em> Powered by Leadwear </em></p></body></html>";
                    mail.subject = "Yor Passward";
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

        public async Task<(bool, string)> Login(string? email, string? phone, string password)
        {
            var getQuery = "select password from tb_user where (email=@email OR phone=@phone) and is_delete=0;";

            var getuserId = "select user_id from tb_user where (email=@email OR phone=@phone) and is_delete=0; ";



            using (var connection = _dapperContext.CreateConnection())
            {
                if (connection == null)
                {
                    throw new InvalidOperationException("Database connection cannot be created");
                }
                var encryptPassword = await connection.QueryAsync<string>(getQuery, new { email ,phone});
                if (encryptPassword != null)
                {
                    var encryptPasswordtoDecrpt=encryptPassword.FirstOrDefault();
                    var decryPassword = _securityUtils.Decrypt(encryptPasswordtoDecrpt.ToString());
                    if (password == decryPassword)
                    {
                        var id = await connection.QueryAsync<int>(getuserId, new { email, phone });
                        var userId=id.FirstOrDefault();
                        if( userId > 0)
                        {
                            var tokenUtilities = new SecurityUtils(_configuration);

                            var token = tokenUtilities.GetToken(userId).ToString();
                           
                            if (token != null)
                            {
                                return (true, token);
                                
                            }
                        }
                        return (false, StatusUtils.INVALID_PHONE_AND_EMAIL);

                    }
                    else 
                    {
                        return (false, StatusUtils.INVALID_PASSWORD);
                    }
                }
                
                return (false,StatusUtils.INVALID_PHONE_AND_EMAIL);
            }
        }

        public async Task<(bool, string)> RestPassword(string data, string password)
        {
            var updateQuery = "update tb_user set password=@password where email=@email;" +
                "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
            var email = _securityUtils.Decrypt(data);
            if(email != null)
            {
                using (var connection = _dapperContext.CreateConnection())
                {
                    var encryptPassword = _securityUtils.Encrypt(password);
                    var status = await connection.ExecuteScalarAsync<int>(updateQuery, new { password = encryptPassword, email=email} );
                    if(status > 0)
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
            var decryptedData = _securityUtils.Decrypt(data);
            if (decryptedData != null)
            {
                var model = JsonConvert.DeserializeObject<VarificationModel>(decryptedData);
                var timeDifference = DateTime.Now - model.currentDateTime;
                    if (timeDifference.TotalMinutes < 5)
                {
                    if (model.otp == otp)
                    {
                        return (true, _securityUtils.Encrypt(model.userDeatils));
                    }
                    return (false, StatusUtils.INVALID_OTP);
                }
                return (false, StatusUtils.OPT_EXPIRED);
            }
            return (false, StatusUtils.UNAUTHORIZED_ACCESS);

        }
        
    }
}
