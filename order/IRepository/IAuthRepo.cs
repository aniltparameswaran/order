namespace order.IRepository
{
    public interface IAuthRepo
    {
        public Task<(bool, string)> Login(string? email,string? phone, string password);
        public Task<(bool, string)> ForgotPassword(string? email, string? phone);
        public Task<(bool, string)> VarificationOtp(string data,int otp);
        public Task<(bool, string)> RestPassword(string data,string password);

    }
}
