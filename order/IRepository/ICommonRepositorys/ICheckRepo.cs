namespace order.IRepository.ICommonRepositorys
{
    public interface ICheckRepo
    {
        public Task<(string, string)> IsEmailExist(string email);
        public Task<(string, string)> IsPhoneNumberExist(string phone);
    }
}
