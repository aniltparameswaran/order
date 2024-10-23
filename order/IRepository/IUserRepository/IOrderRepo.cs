using order.DTOModel;
using order.Models;

namespace order.IRepository.IUserRepository
{
    public interface IOrderRepo
    {
        public Task<(string, string)> InsertOrder(OrderMasterDTOModel orderMasterDTOModel,string inserted_by);
        public Task<string> InsertPayment(PaymentDTOModel paymentDTOModel, string inserted_by);

        public Task<int> UpdateCredit(decimal creadit_amount, decimal payed_amount, string shop_id, string inserted_by,string payment_id);
        public Task<string> UpdateAddProductQuatity(string order_master_id);
        public Task<(bool, string)> UpdateSubProductQuatity(string product_details_id, int quatity);

        public Task<IEnumerable<GetOrderMasterByUserIdModel>> GetOrderByUserId(string user_id);

    }
}
