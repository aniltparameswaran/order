using order.DTOModel;

namespace order.IRepository.IUserRepository
{
    public interface IOrderRepo
    {
        public Task<string> InsertOrder(OrderMasterDTOModel orderMasterDTOModel,string inserted_by);
        public Task<string> InsertPayment(PaymentDTOModel paymentDTOModel, string inserted_by);
    }
}
