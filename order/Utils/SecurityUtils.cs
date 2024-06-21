using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using order.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace order.Utils
{
    public class SecurityUtils
    {
        private IConfiguration _iconfiguration;
        private readonly string publicKey;
        private readonly string privateKey;
        public SecurityUtils(IConfiguration configuration)
        {
            _iconfiguration = configuration;
            using (var rsa = RSA.Create())
            {
                rsa.KeySize = 2048;
                publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            }
        }
        

        
        public string Encrypt(string data)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
                var encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.OaepSHA256);
                return Convert.ToBase64String(encryptedData);
            }
        }
        public string Decrypt(string encryptedData)
        {
            using (var rsa = RSA.Create())
            {
                rsa.KeySize = 2048;
                

                rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
                var decryptedData = rsa.Decrypt(Convert.FromBase64String(encryptedData), RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decryptedData);
            }
        }
        public string GetToken(int user_id)
        {
            var claims = new[]
            {
                        new Claim(JwtRegisteredClaimNames.Sub,_iconfiguration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat,DateTime.UtcNow.ToString()),
                        new Claim("user_id",user_id.ToString()),

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_iconfiguration["Jwt:Key"]));

            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _iconfiguration["Jwt:Issuer"],
                _iconfiguration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddDays(40),
                signingCredentials: signIn
                );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            return accessToken;
        }
        

        
    }
}
