using Dapper;
using order.Context;
using order.DTOModel;
using order.IRepository.IAdminRepositorys;
using order.IRepository.IUserRepository;
using order.Models;
using order.Utils;

namespace order.Repository.UserRepository
{
    public class ItemRepo : IItemRepo
    {
        private readonly DapperContext _dapperContext;
        

        public ItemRepo(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<(bool, string)> CheckQuantity(OrderDetailsDTOModel item)
        {
            var getQuery = "select count(*) from tb_product_details  where is_delete=0 and is_active=1 and" +
                " available_quantity>@quantity and product_details_id=@product_details_id;";
            using (var connection = _dapperContext.CreateConnection())
            {
                
                    var count = await connection.QuerySingleOrDefaultAsync<int>(getQuery, new { quantity = item.quatity, product_details_id= item.product_details_id });
                    if (count > 0)
                    {
                        return (true, StatusUtils.QUANTITY_AVAILABLE);
                    }
                    else
                    {
                        return (false, StatusUtils.QUANTITY_NOT_AVAILABLE);
                    }
                   
               
                

            }
        }

        public async Task<IEnumerable<GetProductDetailsModel>> GetItem(string item_code)
        {
            var getQuery = "select tb_product_details.product_details_id,tb_product_details.available_quantity,tb_product_details.rate,tb_product_details.discount," +
                "tb_product_details.size_range from tb_product_master " +
                "inner join tb_product_details on tb_product_details.product_master_id=tb_product_master.product_master_id" +
                " where tb_product_details.is_delete=0 and tb_product_details.is_active=1 and tb_product_details.available_quantity>0 and tb_product_master.product_code=@product_code;";
            using (var connection = _dapperContext.CreateConnection())
            {
                var itemList= await connection.QueryAsync<GetProductDetailsModel>(getQuery, new { product_code = item_code });
                return itemList.ToList();
            }
        }
    }
}
