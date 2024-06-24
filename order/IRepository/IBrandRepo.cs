using order.DTOModel;

namespace order.IRepository
{
    public interface IBrandRepo
    {
        public Task<string> InsertBrand(string brand_name);
        public Task<(bool, string)> DeleteBrand(string brand_id, long action);
        public Task<(string, string)> IsBrandExist(string brand_name);
        public Task<int> UpdateBrandName(string brand_name, string brand_id);
    }
}
