using System.Numerics;

namespace order.DTOModel
{
    public class EmployeeRegistrationDTOModel
    {
        public string user_name { get; set; }
        public string address { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public int pin { get; set; }
        public string adhaar_no { get; set; }
    }
}
