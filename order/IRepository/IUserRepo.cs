using Dapper;
using order.Context;
using order.DTOModel;
using order.Models;

namespace order.IRepository
{
    public interface IUserRepo
    {
        public Task<int> UserRegistration(UserRegistrationDTOModel model);
        public Task<(int, string)> IsEmailExist(string email);
        public Task<(int, string)> IsPhoneNumberExist(string phone);
        public Task<(bool, string)> DeleteUser(int user_id, int action);
        public Task<UserdetailsByIdModel> GetUserDetailsByUserId(int user_id);
        public Task<UserdetailsModel> GetAllUserDetails();
        public Task<int> UpdateUserByAdmin(UserUpdateDTOModel model, int user_id);
        public Task<int> UpdateUserByUser(string phone,string email, int user_id);
        

    }
}
