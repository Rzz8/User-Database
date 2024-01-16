using DotnetAPI.Models;

namespace DotnetAPI.Data
{

    public class UserRepository : IUserRepository
    {
        DataContextEF _entityFramwork;
        public UserRepository(IConfiguration config)
        {
            _entityFramwork = new DataContextEF(config);
        }

        public bool SaveChanges()
        {
            return _entityFramwork.SaveChanges() > 0;
        }

        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramwork.Add(entityToAdd);
            }
        }

        public void RemoveEntity<T>(T entityToRemove)
        {
            if (entityToRemove != null)
            {
                _entityFramwork.Remove(entityToRemove);
            }
        }

        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _entityFramwork.Users.ToList<User>();
            return users;
        }

        public IEnumerable<UserSalary> GetUserSalaries()
        {
            IEnumerable<UserSalary> userSalaries = _entityFramwork.UserSalary.ToList<UserSalary>();
            return userSalaries;
        }

        public IEnumerable<UserJobInfo> GetJobInfo()
        {
            IEnumerable<UserJobInfo> userJobInfos = _entityFramwork.UserJobInfo.ToList();
            return userJobInfos;
        }

        public User GetSingleUser(int userId)
        {
            User? user = _entityFramwork.Users
                .Where(u => u.UserId == userId)
                    .FirstOrDefault<User>();

            if (user != null)
            {
                return user;
            }

            throw new Exception("Failed to get user");
        }

        public UserSalary GetSingleUserSalary(int userId)
        {
            UserSalary? userSalary = _entityFramwork.UserSalary
                .Where(u => u.UserId == userId)
                    .FirstOrDefault<UserSalary>();

            if (userSalary != null)
            {
                return userSalary;
            }
            throw new Exception("Failed to get user salary");
        }
        public UserJobInfo GetSingleUserJobInfo(int userId)
        {
            UserJobInfo? userJobInfo = _entityFramwork.UserJobInfo
                .Where(u => u.UserId == userId)
                    .FirstOrDefault<UserJobInfo>();

            if (userJobInfo != null)
            {
                return userJobInfo;
            }
            throw new Exception("Failed to get user job info");
        }
    }
}
