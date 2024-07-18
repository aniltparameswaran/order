namespace order.DTOModel
{
    public class TokensDto
    {
        public string AccessToken { get; set; }
        public IDictionary<string, string> RefreshToken { get; set; }
    }
}
