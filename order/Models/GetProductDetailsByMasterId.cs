using order.DTOModel;

namespace order.Models
{
    public class GetProductDetailsByMasterId
    {
        public string product_master_id { get; set; }
        public string product_code { get; set; }
        public string brand_id { get; set; }
        public string brand_name { get; set; }
        public string product_type { get; set; }
        public string sleeve { get; set; }
        public int is_active { get; set; }
        public List<GetProductDetailsModel> ProductDetailsList { get; set; } = new List<GetProductDetailsModel>();
    }
}
