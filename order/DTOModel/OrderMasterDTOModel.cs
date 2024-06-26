namespace order.DTOModel
{
    public class OrderMasterDTOModel
    {
        public string shop_id { get; set; }
        public decimal total_amount { get; set; }
        public decimal payed_amount { get; set; }
        public int urgent { get; set; }
        public int total_number_of_item { get; set; }
        public string payment_type { get; set; }
        public string payment_no { get; set; }

        public List<OrderDetailsDTOModel> orderDetailsDTOModels { get; set; }=new List<OrderDetailsDTOModel>();
       
        
    }
}
