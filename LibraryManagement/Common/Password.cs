using System;


namespace LibraryManagement.Common {
    public class Password {
        public static string Encode(string password) {
            try {
                byte[] EncDataByte = new byte[password.Length];
                EncDataByte = System.Text.Encoding.UTF8.GetBytes(password);
                string Encrypted = Convert.ToBase64String(EncDataByte);
                return Encrypted;
            } catch (Exception e) {

                throw new Exception("Encode failed " + e.Message);
            }
        }
    }
}