using System.Security.Cryptography;
using System.Text;

namespace XeniaTempleBackend.Service.PaymentHelper
{
    public class SIBUPICallBackEncryption

    {

        public static byte[] FromHexString(string input)



        {

            if (string.IsNullOrEmpty(input)) return Array.Empty<byte>();



            if ((input.Length & 1) != 0) throw new ArgumentException("Invalid hex string length", nameof(input));



            byte[] result = new byte[input.Length >> 1];



            for (int index = 0, charIndex = 0; index < result.Length; index++, charIndex += 2)



            {

                char c1 = input[charIndex];



                char c2 = input[charIndex + 1];



                byte n1 = HexChar(c1);



                byte n2 = HexChar(c2);



                result[index] = (byte)(n1 << 4 | n2);

            }



            return result;

        }

        public static byte HexChar(char c)



        {

            if ('0' <= c && c <= '9') return (byte)(c - '0');



            if ('a' <= c && c <= 'f') return (byte)(c - 'a' + 10);



            if ('A' <= c && c <= 'F') return (byte)(c - 'A' + 10);



            throw new ArgumentException($"Invalid hex char: '{c}'", nameof(c));

        }

        public static string Encrypt(string TextToEncrypt, string encryptionKey)

        {

            byte[] plainText = Encoding.UTF8.GetBytes(TextToEncrypt);

            byte[] key = FromHexString(encryptionKey);

            RijndaelManaged algorithm = new RijndaelManaged();

            algorithm.Mode = CipherMode.ECB;

            algorithm.Padding = PaddingMode.PKCS7;

            algorithm.BlockSize = 128;

            algorithm.KeySize = 128;

            algorithm.Key = key;

            string result;

            using (ICryptoTransform encryptor = algorithm.CreateEncryptor())

            {

                using (MemoryStream memoryStream = new MemoryStream())

                {

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))



                    {

                        cryptoStream.Write(plainText, 0, plainText.Length);



                        cryptoStream.FlushFinalBlock();



                        result = Convert.ToBase64String(memoryStream.ToArray());

                    }

                }

            }

            byte[] bytes = Convert.FromBase64String(result);

            string hex = BitConverter.ToString(bytes);

            return (hex.Replace("-", ""));

        }

        public static string Decrypt(string resmsg, string Key)

        {

            byte[] cipherText = FromHexString(resmsg);



            byte[] key = FromHexString(Key);



            RijndaelManaged algorithm = new RijndaelManaged();



            algorithm.Mode = CipherMode.ECB;



            algorithm.Padding = PaddingMode.PKCS7;



            algorithm.BlockSize = 128;



            algorithm.KeySize = 128;



            algorithm.Key = key;



            string resu = null;



            using (ICryptoTransform decryptor = algorithm.CreateDecryptor())



            {

                using (MemoryStream memoryStream = new MemoryStream(cipherText))



                {

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))



                    {

                        using (StreamReader srDecrypt = new StreamReader(cryptoStream))

                        {

                            resu = srDecrypt.ReadToEnd();

                        }

                    }

                }

            }

            return (resu);

        }



        public static string EncryptToHex(string plainText, string key, string clientSecret)
        {

            string iv = clientSecret.Substring(0, 16);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    return BitConverter.ToString(msEncrypt.ToArray()).Replace("-", "");
                }
            }
        }

        public static string DecryptFromHex(string encryptedText, string key, string clientSecret)
        {
            string iv = clientSecret.Substring(0, 16);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        byte[] encryptedBytes = new byte[encryptedText.Length / 2];
                        for (int i = 0; i < encryptedBytes.Length; i++)
                        {
                            encryptedBytes[i] = Convert.ToByte(encryptedText.Substring(i * 2, 2), 16);
                        }

                        csDecrypt.Write(encryptedBytes, 0, encryptedBytes.Length);
                    }

                    return Encoding.UTF8.GetString(msDecrypt.ToArray());
                }
            }
        }

        public static string CreateXClientHash(object encryptedBody, String clientId, String clientSecret)
        {
            string apipath = "/rest/v1/UPI/QR/Code/Generation/Service";
            string apiRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(new { requestMsg = encryptedBody });
            long utcTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string dataToHash = $"{utcTimestamp}{apipath}{clientId}{apiRequestBody}";
            string hashedValue;

            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(clientSecret)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                hashedValue = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            string xClientHash = $"sibt2:{utcTimestamp}:{hashedValue}";
            Console.WriteLine("x-client-hash: " + xClientHash);
            return xClientHash;
        }

        public static string GenerateRandomChars(int length)
        {
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder idBuilder = new StringBuilder();

            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                int randomIndex = random.Next(characters.Length);
                idBuilder.Append(characters[randomIndex]);
            }

            return idBuilder.ToString();
        }

        public static string GenerateGlobalTranID(string consumerName)
        {
            int currentYear = DateTime.UtcNow.Year;
            int currentJulianDate = DateTime.UtcNow.DayOfYear;
            string randomAlphanumeric = GenerateRandomAlphanumeric(9);

            string uniqueTransactionId = $"{consumerName.Substring(0, Math.Min(4, consumerName.Length))}{currentYear}{FormatNumber(currentJulianDate, 3)}{randomAlphanumeric}";

            return uniqueTransactionId;
        }

        private static string GenerateRandomAlphanumeric(int length)
        {
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            char[] resultArray = new char[length];
            for (int i = 0; i < length; i++)
            {
                resultArray[i] = characters[random.Next(characters.Length)];
            }

            return new string(resultArray);
        }

        private static string FormatNumber(int number, int length)
        {
            return number.ToString().PadLeft(length, '0');
        }

    }
}
