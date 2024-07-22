using order.DTOModel;
using order.Models;

namespace order.IRepository.IUserRepository
{
    public interface IItemRepo
    {
        public Task<IEnumerable<GetProductDetailsModel>> GetItem(string item_code);
        public Task<(bool,string)> CheckQuantity(string product_details_id, int quatity);
       
    }
}
