using order.DTOModel;
using order.Models;

namespace order.IRepository.IUserRepoRepository
{
    public interface IShopRepo
    {
        public Task<string> InsertShop(ShopDTOModel shopDTOModel, string inserted_by);
        public Task<int> UpdateShop(UpdateShopDTOModel shopDTOModel, string updated_by, string shop_id);
        public Task<(string, string)> CheckShopIsExsit(string lisense_number, decimal latitude, decimal logitude);

        public Task<IEnumerable<ShopNaameModel>> GetShop(string userId);
        public Task<ShopModel> GetShopDetailByShopId(string shop_id,string userId);
    }
}
