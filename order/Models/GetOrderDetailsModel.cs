namespace order.Models
{
    public class GetOrderDetailsModel
    {

        public string order_details_id { get; set; }
        public string order_master_id { get; set; }

        public string product_details_id { get; set; }
        public int quantity { get; set; }
    }
}
