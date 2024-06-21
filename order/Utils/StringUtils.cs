namespace order.Utils
{
    public class StringUtils
    {
        public static string GenerateRandomOTP(int otpLength)

        {
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string otp = string.Empty;

            Random rand = new Random(Guid.NewGuid().GetHashCode());

            while (otp.Length != otpLength)
            {
                otp = string.Empty;

                for (int i = 0; i < otpLength; i++)
                {
                    otp += saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)]; ;
                }

                char first = otp.First();
                if (first == '0')
                    otp = string.Empty;
            }

            return otp;
        }


        public static string GenerateRandomString(int length)

        {
            string[] saAllowedCharacters = { "a","b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l",
                "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
            "A","B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
            string randomString = string.Empty;


            Random random = new Random(Guid.NewGuid().GetHashCode());

            while (randomString.Length != length)
            {
                randomString = string.Empty;
                for (int i = 0; i < length; i++)
                {
                    randomString += saAllowedCharacters[random.Next(0, saAllowedCharacters.Length)]; ;
                }
            }

            return randomString;
        }
        public static string GetPath()
        {
            return "Media/";
        }
        public static string GetFileName(string fileName)
        {

            return "IMG_" + StringUtils.GenerateRandomString(8) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fileName).ToLower(); ;
        }
    }
}
