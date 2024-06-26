using order.Context;
using order.IRepository;

namespace order.Repository
{
    public class ShopRepo : IShopRepo
    {
        private readonly DapperContext _dapperContext;

        public ShopRepo(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }


    }
}
