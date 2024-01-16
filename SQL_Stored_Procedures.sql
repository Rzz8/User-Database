USE DotNetCourseDatabase 
GO

/***Procedure naming convetion: object + action. So the following example user + get ***/
/***After updating a stored procedure, remember to run the updated stored procedure to take effect***/

/*1. Stored procedure to get user*/
/* EXEC TutorialAppSchema.spUsers_Get @UserId=4, @Active=1*/

ALTER PROCEDURE TutorialAppSchema.spUsers_Get
    @UserId INT = NULL,
    @Active BIT = NULL  /*In SQL, BIT is the data type for boolean*/
    
AS
BEGIN

    /*Drop the temp table if it is already exists */
    DROP TABLE IF EXISTS #AverageDeptSalary

    /*Create a temporary table with average salary based on department */
    SELECT UserJobInfo.Department
        , AVG(UserSalary.Salary) AvgSalary
        /*# is accessible for this query (local temporary table), 
            and ## can be accessed outside of this query (global temporary table)*/
        INTO #AverageDeptSalary
    FROM TutorialAppSchema.Users AS Users 
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
            ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
            ON UserJobInfo.UserId = Users.UserId
        GROUP BY UserJobInfo.Department

    CREATE CLUSTERED INDEX cix_AverageDeptSalary_Department ON #AverageDeptSalary(Department)

    SELECT Users.UserId,
        Users.FirstName,
        Users.LastName,
        Users.Email,
        Users.Gender,
        Users.Active,
        UserSalary.Salary,
        UserJobInfo.Department,
        UserJobInfo.JobTitle,
        AvGSalary.AvgSalary
    FROM TutorialAppSchema.Users AS Users
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
            ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
            ON UserJobInfo.UserId = Users.UserId
        LEFT JOIN #AverageDeptSalary as AvgSalary
            ON AvgSalary.Department = UserJobInfo.Department
    /*ISNULL(expression, replacement_value)*/
    /* expression is the value to be checked, if it is null, then take replacement_value*/
    WHERE Users.UserId = ISNULL(@UserId, Users.UserId) 
        AND ISNULL(Users.Active, 0) = COALESCE(@Active, Users.Active, 0)
END

go

/*2. Stored procedure to add new user or update a user*/
/* AND UserId = @UserId make sure that the acting user (@UserId) from frontend 
   is the same UserId to act on the object in the database (i.e., Users or Posts)*/

CREATE OR ALTER PROCEDURE TutorialAppSchema.spUser_UpdateInsert
	@FirstName nvarchar(50),
	@LastName nvarchar(50),
	@Email nvarchar(50),
	@Gender nvarchar(50),
	@Active bit = 1,
    @UserId int = NULL,
    @Salary decimal(18, 4),  /*Precision (total number of digits): 18; Scale (number of digits to the right of the decimal point): 4*/
    @Department nvarchar(50),
    @JobTitle nvarchar(50)
AS 
BEGIN
    /*Check if a user is null or not in the database*/
    IF NOT EXISTS (SELECT * FROM TutorialAppSchema.Users WHERE UserId=@UserId)
        BEGIN
        IF NOT EXISTS (SELECT * FROM TutorialAppSchema.Users WHERE Email=@Email)
            BEGIN
                /* Declare a variable to store the created UserId */
                DECLARE @OutputUserId INT

                INSERT INTO TutorialAppSchema.Users(
                    [FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active]
                ) VALUES (
                    @FirstName,
                    @LastName,
                    @Email,
                    @Gender,
                    @Active
                )
                
                /*  @@IDENTITY system function is used to retrieve the last identity 
                    value generated in the current session (i.e., UserId). */
                SET @OutputUserId = @@IDENTITY

                INSERT INTO TutorialAppSchema.UserSalary(
                    UserId,
                    Salary
                ) VALUES (
                    @OutputUserId,
                    @Salary
                )

                INSERT INTO TutorialAppSchema.UserJobInfo(
                    UserId,
                    Department,
                    JobTitle
                ) VALUES (
                    @OutputUserId,
                    @Department,
                    @JobTitle
                )
            END
        END
    ELSE
        BEGIN
            UPDATE TutorialAppSchema.Users
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    Gender = @Gender,
                    Active = @Active
                WHERE UserId = @UserId
            
            UPDATE TutorialAppSchema.UserSalary
                SET Salary = @Salary
                WHERE UserId = @UserId

            UPDATE TutorialAppSchema.UserJobInfo
                SET Department = @Department,
                    JobTitle = @JobTitle
                WHERE UserId = @UserId
        END
END

GO

/*3. Stored procedure to delete a user*/
/* EXEC TutorialAppSchema.spUser_Delete @UserId=1001*/

CREATE OR ALTER PROCEDURE TutorialAppSchema.spUser_Delete

    @UserId int
AS
BEGIN
    DELETE FROM TutorialAppSchema.Users WHERE Users.UserId = @UserId
    DELETE FROM TutorialAppSchema.UserSalary WHERE UserSalary.UserId = @UserId
    DELETE FROM TutorialAppSchema.UserJobInfo WHERE UserJobInfo.UserId = @UserId
END

GO

/*4. Stored procedure to get posts*/
/*"Like" operator is commonly used in search for a specified pattern in a column */
/* EXEC TutorialAppSchema.spPosts_Get @UserId=1003 @SearchValue='Second' */
/* EXEC TutorialAppSchema.spPosts_Get @PostId = 2 */

CREATE OR ALTER PROCEDURE TutorialAppSchema.spPosts_Get
    @UserId int = null,
    @SearchValue nvarchar(max) = null,
    @PostId int = null
AS
BEGIN
    SELECT [Posts].[PostId],
        [Posts].[UserId],
        [Posts].[PostTitle],
        [Posts].[PostContent],
        [Posts].[PostCreated],
        [Posts].[PostUpdated] 
    FROM TutorialAppSchema.Posts AS Posts
        WHERE Posts.UserId = ISNULL(@UserId, Posts.UserId)
            AND Posts.PostId = ISNULL(@PostId, Posts.PostId)
            AND (@SearchValue IS NULL 
                OR Posts.PostContent LIKE '%' + @SearchValue +'%'
                OR Posts.PostTitle LIKE '%' + @SearchValue +'%')
END

GO

/*5. Stored procedure to add posts or update posts */

CREATE OR ALTER PROCEDURE TutorialAppSchema.spPosts_UpdateInsert
    @UserId INT,
    @PostTitle NVARCHAR(255),
    @PostContent NVARCHAR(MAX),
    @PostId INT = NULL /*Can be nullable if we first created a post*/
AS
BEGIN
    IF NOT EXISTS (SELECT * FROM TutorialAppSchema.Posts WHERE PostId = @PostId)
        BEGIN
            INSERT INTO TutorialAppSchema.Posts(
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]
            ) VALUES (
                @UserId,
                @PostTitle,
                @PostContent,
                GETDATE(),
                GETDATE()
            )
        END
    ELSE 
        BEGIN
            UPDATE TutorialAppSchema.Posts
            SET PostTitle = @PostTitle,
                PostContent = @PostContent,
                PostUpdated = GETDATE()
            WHERE Posts.PostId = @PostId
                AND Posts.UserId = @UserId
        END
END

GO

/*6. Stored procedure to delete a post*/

CREATE OR ALTER PROCEDURE TutorialAppSchema.spPost_Delete
    @PostId INT,
    @UserId INT
AS
BEGIN
    DELETE FROM TutorialAppSchema.Posts 
    WHERE PostId = @PostId 
        AND UserId = @UserId
END

GO

/*7. Stored procedure for user registration with inputs of passwordhash and passwordsalt*/

CREATE OR ALTER PROCEDURE TutorialAppSchema.spRegistration_UpdateInsert
    @Email NVARCHAR(50),
    @PasswordHash VARBINARY(MAX),
    @PasswordSalt VARBINARY(MAX) 
AS
BEGIN
    IF NOT EXISTS (SELECT * FROM TutorialAppSchema.auth WHERE Email = @Email)
        BEGIN
            INSERT INTO TutorialAppSchema.auth(
                [Email],
                [PasswordHash],
                [PasswordSalt]
            ) VALUES (
                @Email,
                @PasswordHash,
                @PasswordSalt
            )
        END
    ELSE
        BEGIN
            UPDATE TutorialAppSchema.auth 
            SET PasswordHash = @PasswordHash,
                PasswordSalt = @PasswordSalt
            WHERE Email = @Email
        END
END

GO

/*8. Stored procedure to obtain PasswordHash and PasswordSalt from database*/

CREATE OR ALTER PROCEDURE TutorialAppSchema.spLoginConfirmation_Get
    @Email NVARCHAR(50)
AS
BEGIN
    SELECT [Auth].[PasswordHash], 
           [Auth].[PasswordSalt] 
    FROM TutorialAppSchema.auth AS Auth 
    WHERE Auth.Email = @Email
END

GO
