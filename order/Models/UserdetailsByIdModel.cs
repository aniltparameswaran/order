namespace order.Models
{
    public class UserdetailsByIdModel
    {
        public string user_name { get; set; }
        public string address { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public int pin { get; set; }
        public string adhaaar_no { get; set; }
        public int is_active { get; set; }
    }
}
