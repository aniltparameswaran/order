using Dapper;
using order.Context;
using order.DTOModel;
using order.Models;

namespace order.IRepository.IAdminRepositorys
{
    public interface IEmployeeRepo
    {
        public Task<string> UserRegistration(EmployeeRegistrationDTOModel model);
        public Task<(bool, string)> DeleteUser(string user_id, int action);
        public Task<UserdetailsByIdModel> GetUserDetailsByUserId(string user_id);
        public Task<IEnumerable<UserdetailsModel>> GetAllUserDetails();
        public Task<int> UpdateEmployee(EmployeeUpdateDTOModel model, string user_id);

    }
}
