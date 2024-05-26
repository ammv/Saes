using Saes.GrpcServer.Services.Interfaces;
using System.Security.Cryptography;

namespace Saes.GrpcServer.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private static readonly string _allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~";
        public string GenerateToken()
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());

            return token;
        }

        // <summary>
        /// Генерирует случайную строку указанной длины, которая может использоваться в качестве токена.
        /// </summary>
        /// <param name="length">Желаемая длина строки токена.</param>
        /// <param name="allowedChars">Строка, содержащая символы, которые могут использоваться в токене. 
        /// По умолчанию используются буквы (верхний и нижний регистр), цифры и некоторые специальные символы.</param>
        /// <returns>Случайно сгенерированная строка.</returns>
        public string GenerateToken(int length)
        {
            if (length < 1)
            {
                throw new ArgumentException("Длина токена должна быть больше 0.", nameof(length));
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes);
                var result = new char[length];

                for (int i = 0; i < length; i++)
                {
                    result[i] = _allowedChars[bytes[i] % _allowedChars.Length];
                }

                return new string(result);
            }
        }
    }
}
