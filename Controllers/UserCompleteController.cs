using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]

// WeatherForecastController inherit ControllerBase class
public class UserCompleteController : ControllerBase
{
    // Field to hold an instance of DataContextDapper
    private readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;

    // Constructor that takes an IConfiguration parameter
    public UserCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _reusableSql = new ReusableSql(config);
    }

    [HttpGet("GetUsers/{userId}/{isActive}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        // parameters are used to store userId and/or isActive. Initially an empty string
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        string stringParameters = "";
        DynamicParameters sqlParameters = new();

        //Remember to add a comma and space between spUsers_Get and @UserId or @Active!
        if (userId != 0)
        {
            stringParameters += ", @UserId=@UserIdParameter";
            sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
        }

        if (isActive)
        {
            stringParameters += ", @Active=@ActiveParameter";
            sqlParameters.Add("ActiveParameter", isActive, DbType.Boolean);
        }

        /* stringParameters.Substring(1) will take anything from index=1 to the end, 
           remember index starts at 0. So the first "," will not be taken,
           but "," in following parameters (@Active) will be taken if userId != 0.
           Print sql if run in trouble (Console.WriteLine(sql))*/
        if (stringParameters.Length > 0)
        {
            sql += stringParameters.Substring(1);
        }
        IEnumerable<UserComplete> users = _dapper.LoadDataWithParameters<UserComplete>(sql, sqlParameters);
        return users;
    }

    [HttpPut("UpdateInsertUser")]
    public IActionResult UpdateInsertUser(UserComplete user)
    {
        if (_reusableSql.UpdateInsertUser(user))
        {
            return Ok();
        }
        throw new Exception("Failed to update user");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_Delete
                        @UserId = " + userId.ToString();

        Console.WriteLine(sql);
        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }

        throw new Exception("Failed to delete user");
    }

}