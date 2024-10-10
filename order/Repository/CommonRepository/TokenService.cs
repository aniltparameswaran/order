using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using order.DTOModel;
using order.IRepository.ICommonRepositorys;
using order.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Twilio.Http;

namespace order.Repository.CommonRepository
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ICookieService _cookieService;
        private readonly IDictionary<string, string> _refreshTokens = new Dictionary<string, string>();
        public TokenService(IConfiguration configuration, ICookieService cookieService)
        {
            _configuration = configuration;
            _cookieService = cookieService;
        }
        public  string GenerateTokens(string userId)
        {
            ClearAllRefreshTokens();
            _cookieService.ClearAllCookies();

            Console.WriteLine("userId.",userId);
            var accessToken = GenerateAccessToken(userId);
            var refreshToken = GenerateRefreshToken();
             _refreshTokens[refreshToken] = userId;

            _cookieService.SetRefreshTokenCookie(_refreshTokens);

            return  accessToken;

        }

        public void ClearAllRefreshTokens()
        {

            _refreshTokens.Clear();
            Console.WriteLine("All refresh tokens have been cleared.");
        }

        public string RefreshTokens(IRequestCookieCollection cookies)
        {
            
            Console.WriteLine("All refresh tokens have been cleared.", cookies["loggedIn"]);

            var loggedIn=false;

            foreach (var kvp in cookies)
            {
                if(kvp.Key== "loggedIn" && kvp.Value!="")
                {
                    loggedIn=true;
                }
                
            }

            if(!loggedIn)
            {
                _cookieService.ClearAllCookies();
                return null;
            }

            if (cookies.FirstOrDefault().Key!= null)
            {
               
                var user_id = cookies.FirstOrDefault().Value;
                var newAccessToken = GenerateAccessToken(user_id);

                var newRefreshToken = GenerateRefreshToken();
                _refreshTokens[newRefreshToken] = user_id;

                _cookieService.ClearAllCookies();
                _cookieService.SetRefreshTokenCookie(_refreshTokens);

                return newAccessToken ;
            }
            return null;
        }
        public static IRequestCookieCollection ConvertStringToIRequestCookieCollection(string cookieString)
        {
            var cookies = new Dictionary<string, string>();
            var cookiePairs = cookieString.Split(';');

            foreach (var cookiePair in cookiePairs)
            {
                var cookieParts = cookiePair.Split('=');
                if (cookieParts.Length == 2)
                {
                    var key = cookieParts[0].Trim();
                    var value = cookieParts[1].Trim();
                    cookies[key] = value;
                }
            }

            return new CustomRequestCookieCollection(cookies);
        }

        private string GenerateAccessToken(string userId)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim("user_id", userId)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddDays(15), // Shorter expiry for access token
                signingCredentials: signIn
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
        public void ExpireToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken == null)
                throw new ArgumentException("Invalid token");

            var claims = jwtToken.Claims.ToList();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiredToken = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(-1), // Set expiration to past date
                signingCredentials: signIn
            );

            var expiredTokenString = handler.WriteToken(expiredToken);
            Console.WriteLine($"Token expired: {expiredTokenString}");
        }
    }
}
