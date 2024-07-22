using order.IRepository.ICommonRepositorys;

namespace order.Repository.CommonRepository
{
    public class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetRefreshTokenCookie(IDictionary<string, string> refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Strict,
                Secure = true // Ensure this is true in production
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(refreshToken.FirstOrDefault().Key, refreshToken.FirstOrDefault().Value, cookieOptions);
        }

        public void ClearAllCookies()
        {
            foreach (var cookie in _httpContextAccessor.HttpContext.Request.Cookies)
            {
                ClearSpecificCookie(cookie.Key);
            }
        }

        public void SetRefreshLokinCookie()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Strict,
                Secure = true // Ensure this is true in production
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("loggedIn", "true", cookieOptions);
        }
        public void ClearSpecificCookie(string cookieName)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(-1), // Setting expiration date to the past to delete the cookie
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, "", cookieOptions);
        }
    }
}
