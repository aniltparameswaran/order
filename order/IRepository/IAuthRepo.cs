namespace order.IRepository
{
    public interface IAuthRepo
    {
        public Task<(bool, string)> Login(string userName, string password);
        public Task<(bool, string)> ForgotPassword(string userName);
        public Task<(bool, string)> VarificationOtp(string data,int otp);
        public Task<(bool, string)> RestPassword(string data,string password);

    }
}
