namespace order.Models
{
    public class GetProductDetailsModel
    {
        public string product_details_id { get; set; }
        public int available_quantity { get; set; }
        public decimal rate { get; set; }
        public int discount { get; set; }
        public string description { get; set; }
        public int is_active { get; set; }
    }

}
