using Dapper;
using order.Context;
using order.IRepository.ICommonRepositorys;
using order.Utils;

namespace order.Repository.CommonRepository
{
    public class CheckRepo: ICheckRepo
    {
        private readonly DapperContext _dapperContext;

        public CheckRepo(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }
        public async Task<(string, string)> IsEmailExist(string email)
        {
            try
            {
                var query = "select user_id from tb_user where email = @email and is_delete=0;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("email", email);
                    var user_id = await connection.QuerySingleOrDefaultAsync<string>(query, parameters);
                    if (!string.IsNullOrEmpty(user_id))
                    {
                        return (user_id, StatusUtils.EMAIL_ALREADY_EXIST);
                    }
                    return (null, StatusUtils.EMAIL_NOT_EXIST);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while Email checking");
            }
        }
        public async Task<(string, string)> IsPhoneNumberExist(string phone)
        {
            try
            {
                var query = "select user_id from tb_user where phone=@phone and is_delete=0;;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("phone", phone);
                    var user_id = await connection.ExecuteScalarAsync<string>(query, parameters);
                    if (!string.IsNullOrEmpty(user_id))
                    {
                        return (user_id, StatusUtils.PHONE_NUMBER_ALREADY_EXIST);
                    }
                    return (null, StatusUtils.PHONE_NUMBER_NOT_EXIST);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while phone number checking");
            }
        }
    }
}
