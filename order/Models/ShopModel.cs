namespace order.Models
{
    public class ShopModel
    {
        public string shop_id { get; set; }
        public string shop_name { get; set; }
        public string address { get; set; }
        public long pin { get; set; }
        public string landmark { get; set; }
        public decimal latitude { get; set; }
        public decimal logitude { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string license_number { get; set; }
        public int is_active { get; set; }
        public decimal creadit_amount { get; set; }

    }
}
