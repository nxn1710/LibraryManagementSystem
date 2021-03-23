using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LibraryManagement.Commons {
    public class Password {

        public static string key = "nxnldsnqhnaq@#$@";
        public static string Encrypt(String password) {
            if (string.IsNullOrEmpty(password)) return "";
            password += key;
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(passwordBytes);
        }

        public static string Decrypt(string base64Encode) {
            if (string.IsNullOrEmpty(base64Encode)) return "";
            var base64EncodeBytes = Convert.FromBase64String(base64Encode);
            var result = Encoding.UTF8.GetString(base64EncodeBytes);
            result = result.Substring(0, result.Length - key.Length);
            return result;
        }
    }
}