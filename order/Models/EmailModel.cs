namespace order.Models
{
    public class EmailModel
    {
        public string email_html_body { get; set; } 
        public string signature_content { get; set; }
        public string from_email { get; set; }
        public string to_email { get; set;}
        public string subject { get; set; }
        public string from_email_password { get; set; }           
    }
}
