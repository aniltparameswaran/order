namespace order.DTOModel
{
    public class ProductMasterUpdateDtoModel
    {
        public string brand_id { get; set; }
        public string product_type { get; set; }
        public string sleeve { get; set; }
        public string material { get; set; }

        public List<ProductDetailsUpdateDTOModel> ProductDetailsListl { get; set; } = new List<ProductDetailsUpdateDTOModel>();
    }
}
