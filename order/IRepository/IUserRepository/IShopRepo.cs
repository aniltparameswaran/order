using order.DTOModel;

namespace order.IRepository.IUserRepoRepository
{
    public interface IShopRepo
    {
        public Task<string> InsertShop(ShopDTOModel shopDTOModel, string inserted_by);
        public Task<string> UpdateShop(UpdateShopDTOModel shopDTOModel, string updated_by, string shop_id);
        public Task<bool> CheckShopIsExsit(string lisense_number, decimal latitude, decimal logitude);
    }
}
