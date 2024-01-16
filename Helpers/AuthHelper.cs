using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;

        public AuthHelper(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }
        // Method to generate password hash
        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
            );
        }

        // Method to create a JWT token
        public string CreateToken(int userId)
        {
            // Define claims for the token
            Claim[] claims = new Claim[] {
                new("userId", userId.ToString())
            };

            // Get the token key from configuration
            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            // Create a symmetric security key from the token key
            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    tokenKeyString ?? ""
                )
            );

            // Create signing credentials using the security key
            SigningCredentials credentials = new SigningCredentials(
                tokenKey,
                SecurityAlgorithms.HmacSha512Signature
            );

            // Create a security token descriptor
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            // Create a JWT token
            SecurityToken token = tokenHandler.CreateToken(descriptor);

            // Write the token as a string
            return tokenHandler.WriteToken(token);
        }

        public bool SetPassword(UserForLoginDto userForSetPassword)
        {
            // Initialize a passwordSalt. Initially it is a byte array with 16 zeros. 
            byte[] passwordSalt = new byte[128 / 8];

            // Use RandomNumberGenerator to assign 16 non-zero bytes to the passwordSalt
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);  // fill passwordSalt with non-zero bytes
            }

            // Generate password hash
            byte[] passwordHash = GetPasswordHash(userForSetPassword.Password, passwordSalt);

            // SQL query to add authentication data to the database
            string sqlAddAuth = @" TutorialAppSchema.spRegistration_UpdateInsert
                            @Email = @EmailParam,
                            @PasswordHash = @PasswordHashParam, 
                            @PasswordSalt = @PasswordSaltParam";

            // Add sql dynamic parameters
            DynamicParameters sqlParameters = new();
            sqlParameters.Add("@EmailParam", userForSetPassword.Email, DbType.String);
            sqlParameters.Add("@PasswordHashParam", passwordHash, DbType.Binary);
            sqlParameters.Add("@PasswordSaltParam", passwordSalt, DbType.Binary);

            // Execute SQL query with parameters to add authentication data
            return _dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters);
        }
    }

}

