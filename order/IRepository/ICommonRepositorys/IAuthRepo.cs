using order.DTOModel;

namespace order.IRepository.ICommonRepositorys
{
    public interface IAuthRepo
    {
        public Task<(bool, string, string)> Login(LoginDTOModel loginDTOModel, int adminOrNot);
        public Task<(bool, string)> ForgotPassword(string userName);
        public Task<(bool, string)> VarificationOtp(string data, int otp);
        public Task<(bool, string)> RestPassword(string data, string password);

    }
}
