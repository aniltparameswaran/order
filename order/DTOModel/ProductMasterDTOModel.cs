namespace order.DTOModel
{
    public class ProductMasterDTOModel
    {
       
        public string brand_id { get; set; }
        public string product_type { get; set; }
        public string sleeve { get; set; }
        public List<ProductDetailsDTOModel> ProductDetailsListl { get; set; } = new List<ProductDetailsDTOModel>();

    }
}
