using Dapper;
using order.Context;
using order.DTOModel;
using order.Models;

namespace order.IRepository
{
    public interface IUserRepo
    {
        public Task<string> UserRegistration(UserRegistrationDTOModel model);
        public Task<(string, string)> IsEmailExist(string email);
        public Task<(string, string)> IsPhoneNumberExist(string phone);
        public Task<(bool, string)> DeleteUser(string user_id, int action);
        public Task<UserdetailsByIdModel> GetUserDetailsByUserId(string user_id);
        public Task<UserdetailsModel> GetAllUserDetails();
        public Task<int> UpdateUserByAdmin(UserUpdateDTOModel model, string user_id);
        public Task<int> UpdateUserByUser(string phone,string email, string user_id);
        

    }
}
