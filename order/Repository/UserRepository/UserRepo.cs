using Dapper;
using order.Context;
using order.IRepository.IUserRepoRepository;
using order.Utils;

namespace order.Repository.UserRepository
{

    public class UserRepo: IUserRepo
    {
        private readonly DapperContext _dapperContext;

        public UserRepo(DapperContext dapperContext, SecurityUtils securityUtils)
        {
            _dapperContext = dapperContext;
        }
        public async Task<int> UpdateUser(string phone, string email, string user_id)
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
