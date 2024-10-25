namespace order.DTOModel
{
    public class ProductDetailsUpdateDTOModel
    {
 
        public string product_details_id { get; set; }
        public int available_quantity { get; set; }
        public decimal rate { get; set; }
        public int discount { get; set; }
        public string size_range { get; set; }
    }
}
