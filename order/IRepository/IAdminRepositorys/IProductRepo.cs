using order.DTOModel;
using order.Models;

namespace order.IRepository.IAdminRepositorys
{
    public interface IProductRepo
    {
        public Task<(bool, string)> InsertProduct(ProductMasterDTOModel productMasterDTOModel);
        public Task<int> UpdateProductMaster(ProductMasterUpdateDTOModel productMasterUpdateDTOModel, string product_master_id);
        public Task<int> UpdateProductDetail(ProductDetailsUpdateDTOModel productDetailsUpdateDTOModel, string product_details_id);
        public Task<(bool, string)> DeleteProductDetail(string product_master_id, long action);
        public Task<(bool, string)> DeleteProductMaster(string product_details_id, long action);
        public Task<IEnumerable<GetProductMasterModel>> GetProductMaster();
        public Task<GetProductDetailsByMasterId> GetProductDetailByMasterId(string product_master_id);
    }
}
