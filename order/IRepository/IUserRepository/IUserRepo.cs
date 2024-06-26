namespace order.IRepository.IUserRepoRepository
{
    public interface IUserRepo
    {
        public Task<int> UpdateUser(string phone, string email, string user_id);
        
    }
}
