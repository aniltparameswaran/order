using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using order.IRepository.ICommonRepositorys;

namespace order.Utils
{
    public class CustomTokenMiddleware
    { 
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomTokenMiddleware> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CustomTokenMiddleware(RequestDelegate next, ILogger<CustomTokenMiddleware> logger, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
        {
            var accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogInformation("Access token is missing, checking for refresh token.");

                var refreshToken = context.Request.Cookies.FirstOrDefault().Key;
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var cookies = _httpContextAccessor.HttpContext.Request.Cookies;
                    var tokens = tokenService.RefreshTokens(cookies);
                    if (tokens != null)
                    {
                        
                        context.Request.Headers["Authorization"] = "Bearer " + tokens;
                    }
                   
                }
                else
                {
                    _logger.LogWarning("Refresh token is missing.");
                }
            }

            await _next(context);
        }
    }
}