namespace order.DTOModel
{
    public class PaymentDTOModel
    {
      
        public string shop_id { get; set; }
        public decimal total_amount { get; set; }
        public decimal amount { get; set; }
        public string payment_type { get; set; }
        public string payment_no { get; set; }
    }
}
