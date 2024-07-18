namespace order.IRepository.ICommonRepositorys
{
    public interface ICookieService
    {
        void SetRefreshTokenCookie(IDictionary<string, string> refreshToken);
        void ClearAllCookies();
        void SetRefreshLokinCookie();
        void ClearSpecificCookie(string cookieName);
    }
}
