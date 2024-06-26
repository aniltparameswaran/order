namespace order.DTOModel
{
    public class UpdateShopDTOModel
    {
        public string shop_name { get; set; }
        public string address { get; set; }
        public long pin { get; set; }
        public string lantmark { get; set; }
        public decimal latitude { get; set; }
        public decimal logitude { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
    }
}
