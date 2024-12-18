﻿using Microsoft.Extensions.Logging;
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
        private readonly IConfiguration _iconfiguration;
        private readonly static string key= "coKSPYDscPXWKuiZrg2oPj2X6nR3CHx8";
        public SecurityUtils(IConfiguration configuration)
        {

            _iconfiguration = configuration;
        }

        public static string encrypt(string encryptString)
        {
            
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
             });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write, true))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }

            return encryptString;

        }



        public static string EncryptString(string data)
        {
            string res = string.Empty;
            try
            {
                res = encrypt(data);
            }
            catch (Exception)
            {

            }
            return res;
        }

        public static string DecryptString(string data)
        {
            string res = string.Empty;
            try
            {
                res = Decrypt(data);
            }
            catch (Exception)
            {

            }
            return res;
        }




        public static string Decrypt(string cipherText)
        {
           
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static string EncryptModel(string value,string userDeatils)
        {
            string res = string.Empty;
            try
            {
                EncryptvalueModel model = new EncryptvalueModel
                {
                    CurrentDate = DateTime.Now,
                    value = value,
                    userDeatils= userDeatils,

                };
                res = EncryptString(JsonConvert.SerializeObject(model));
            }
            catch (Exception)
            {

            }
            return res;
        }



        public static EncryptvalueModel DecryptModel(string valueModel)
        {
            EncryptvalueModel res = null;
            try
            {
                res = JsonConvert.DeserializeObject<EncryptvalueModel>(DecryptString(valueModel));
            }
            catch (Exception)
            {

            }
            return res;
        }

        public class EncryptvalueModel
        {
            public DateTime CurrentDate { get; set; }

            public string value { get; set; }
            public string? userDeatils { get; set; }
        }
        
    }
}
