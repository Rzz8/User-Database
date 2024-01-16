using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;

namespace DotnetAPI.Helpers
{
    public class ReusableSql
    {
        private readonly DataContextDapper _dapper;
        public ReusableSql(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        public bool UpdateInsertUser(UserComplete user)
        {
            string sql = @"EXEC TutorialAppSchema.spUser_UpdateInsert
                        @FirstName = @FirstNameParameter, 
                        @LastName = @LastNameParameter,
                        @Email = @EmailParameter,
                        @Gender = @GenderParameter, 
                        @Active = @ActiveParameter,
                        @JobTitle = @JobTitleParameter, 
                        @Department = @DepartmentParameter, 
                        @Salary = @SalaryParameter,
                        @UserId = @UserIdParameter";

            DynamicParameters sqlParameters = new();
            sqlParameters.Add("@FirstNameParameter", user.FirstName, System.Data.DbType.String);
            sqlParameters.Add("@LastNameParameter", user.LastName, System.Data.DbType.String);
            sqlParameters.Add("@EmailParameter", user.Email, System.Data.DbType.String);
            sqlParameters.Add("@GenderParameter", user.Gender, System.Data.DbType.String);
            sqlParameters.Add("@ActiveParameter", user.Active, System.Data.DbType.Boolean);
            sqlParameters.Add("@JobTitleParameter", user.JobTitle, System.Data.DbType.String);
            sqlParameters.Add("@DepartmentParameter", user.Department, System.Data.DbType.String);
            sqlParameters.Add("@SalaryParameter", user.Salary, System.Data.DbType.Decimal);
            sqlParameters.Add("@UserIdParameter", user.UserId, DbType.Int32);

            return _dapper.ExecuteSqlWithParameters(sql, sqlParameters);
        }
    }
}