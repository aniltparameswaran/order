namespace order.Models
{
    public class GetProductMasterModel
    {
        public string product_master_id { get; set; }
        public string product_code { get; set; }
        public string brand_id { get; set; }
        public string brand_name { get; set; }
        public string product_type { get; set; }
        public string sleeve { get; set; }
        public string material { get; set; }
        public int is_active { get; set; }
    }
}
