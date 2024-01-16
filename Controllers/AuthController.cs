using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        private readonly ReusableSql _reusableSql;
        private readonly IMapper _mapper;

        // Constructor to initialize the AuthController with dependencies
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
            _reusableSql = new ReusableSql(config);
            _mapper = new Mapper(new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<UserForRegistrationDto, UserComplete>();
                }
            ));
        }

        // [AllowAnonymous] allows unauthorized users to use this route
        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            // Check if password and password confirm matches
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                // SQL query to check if the user already exists
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '"
                    + userForRegistration.Email + "'";

                // Load existing users from the database
                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);

                // If user does not exist, proceed with registration
                if (existingUsers.Count() == 0)
                {
                    UserForLoginDto userForSetPassword = new()
                    {
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password
                    };

                    // Execute SQL query with parameters to add authentication data
                    if (_authHelper.SetPassword(userForSetPassword))
                    {
                        UserComplete userComplete = _mapper.Map<UserComplete>(userForRegistration);
                        userComplete.Active = true;

                        // Execute SQL query to add user data
                        if (_reusableSql.UpdateInsertUser(userComplete))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to add user.");
                    }
                    throw new Exception("Failed to register user.");
                }
                throw new Exception("User already exists!");
            }
            throw new Exception("Passwords do not match!");
        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto userToSetPassword)
        {
            if (_authHelper.SetPassword(userToSetPassword))
            {
                return Ok();
            }
            throw new Exception("Failed to update password!");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            // SQL query to retrieve password hash and salt based on the provided email
            string sqlForHashAndSalt = @"EXEC TutorialAppSchema.spLoginConfirmation_Get
                            @Email = @EmailParam";

            DynamicParameters sqlParameters = new();

            sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);

            // Load user data (including passwordHash and passwordSalt) from the database
            UserForLoginConfirmationDto userForLoginConfirmation = _dapper
                .LoadDataSingleWithParameters<UserForLoginConfirmationDto>(sqlForHashAndSalt, sqlParameters);

            // Compute the hash of the provided password using the stored salt
            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForLoginConfirmation.PasswordSalt);

            // Compare the computed hash with the stored hash
            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForLoginConfirmation.PasswordHash[index])
                {
                    // If the hashes don't match, return 401 (Unauthorized) with an error message
                    return StatusCode(401, "Incorrect password!");
                }
            }

            // SQL query to retrieve the user ID based on the provided email
            string userIdSql = @" 
                SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '" +
                        userForLogin.Email + "'";

            // Load the user ID from the database
            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            // Return a success response with a token (authentication token) in a dictionary
            return Ok(new Dictionary<string, string>{
                {"token", _authHelper.CreateToken(userId)}
            });
        }

        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            string userIdSql = @" 
                SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = '" +
                        User.FindFirst("userId")?.Value + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return _authHelper.CreateToken(userId);
        }

    }
}
