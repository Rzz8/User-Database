
using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]

public class UserEFController : ControllerBase
{
    IUserRepository _userRepository;
    IMapper _mapper;

    public UserEFController(IConfiguration config, IUserRepository userRepository)
    {
        _userRepository = userRepository;

        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserToAddDto, User>();
            cfg.CreateMap<UserSalary, UserSalary>();
            cfg.CreateMap<UserJobInfo, UserJobInfo>();
        }));
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _userRepository.GetUsers();
        return users;
    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        return _userRepository.GetSingleUser(userId);
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        // userDb is the user we find from database
        User? userDb = _userRepository.GetSingleUser(user.UserId);

        if (userDb != null)
        {
            userDb.Active = user.Active;
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Gender = user.Gender;
            userDb.Email = user.Email;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Failed to update user");
        }

        throw new Exception("Failed to get user");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        // map the input user to Entityframework User type
        User userDb = _mapper.Map<User>(user);

        _userRepository.AddEntity<User>(userDb);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to add user");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDb = _userRepository.GetSingleUser(userId);

        if (userDb != null)
        {
            _userRepository.RemoveEntity<User>(userDb);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Failed to delete user");
        }
        throw new Exception("Failed to get user");
    }

    [HttpGet("GetUserSalaries")]
    public IEnumerable<UserSalary> GetUserSalaries()
    {
        IEnumerable<UserSalary> userSalaries = _userRepository.GetUserSalaries();
        return userSalaries;
    }

    [HttpGet("GetSingleUserSalary/{userId}")]
    public UserSalary GetSingleUserSalary(int userId)
    {
        UserSalary userSalary = _userRepository.GetSingleUserSalary(userId);
        return userSalary;
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary user)
    {
        _userRepository.AddEntity<UserSalary>(user);

        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to add user salary");
    }

    [HttpPut("UpdateUserSalary")]
    public IActionResult UpdateUserSalary(UserSalary userForUpdate)
    {
        UserSalary? userToUpdate = _userRepository.GetSingleUserSalary(userForUpdate.UserId);

        if (userToUpdate != null)
        {
            userToUpdate.Salary = userForUpdate.Salary;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Failed to update salary");
        }

        throw new Exception("Failed to get user");
    }

    [HttpDelete("DeleteSalary/{userId}")]
    public IActionResult DeleteSalary(int userId)
    {
        UserSalary userToDelete = _userRepository.GetSingleUserSalary(userId);

        if (userToDelete != null)
        {
            _userRepository.RemoveEntity<UserSalary>(userToDelete);

            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Failed to delete user salary");
        }
        throw new Exception("Failed to get user");
    }

    [HttpGet("GetJobInfo")]
    public IEnumerable<UserJobInfo> GetJobInfo()
    {
        IEnumerable<UserJobInfo> userJobInfos = _userRepository.GetJobInfo();
        return userJobInfos;
    }

    [HttpGet("GetSingleJobInfo/{userId}")]
    public UserJobInfo GetSingleJobInfo(int userId)
    {
        UserJobInfo? user = _userRepository.GetSingleUserJobInfo(userId);

        if (user != null)
        {
            return user;
        }
        throw new Exception("Failed to get user");
    }

    [HttpPut("UpdateJobInfo")]
    public IActionResult UpdateJobInfo(UserJobInfo userForUpdate)
    {
        UserJobInfo? userToUpdate = _userRepository.GetSingleUserJobInfo(userForUpdate.UserId);

        if (userToUpdate != null)
        {
            userToUpdate.JobTitle = userForUpdate.JobTitle;
            userToUpdate.Department = userForUpdate.Department;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Failed to update job info");
        }
        throw new Exception("Failed to get user");
    }

    [HttpPost("AddJobInfo")]
    public IActionResult AddJobInfo(UserJobInfo userForAdd)
    {
        _userRepository.AddEntity<UserJobInfo>(userForAdd);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Failed to add user jobinfo");
    }

    [HttpDelete("DeleteJobInfo/{userId}")]
    public IActionResult DeleteJobInfo(int userId)
    {
        UserJobInfo? userToDelete = _userRepository.GetSingleUserJobInfo(userId);

        if (userToDelete != null)
        {
            _userRepository.RemoveEntity<UserJobInfo>(userToDelete);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Failed to delete job info");
        }

        throw new Exception("Failed to get user");

    }
}