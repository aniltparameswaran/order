using order.DTOModel;

namespace order.IRepository.ICommonRepositorys
{
    public interface ITokenService
    {
        public string GenerateTokens(string userId);
        public string RefreshTokens( IRequestCookieCollection cookies);
        void ExpireToken(string token);
    }
}
