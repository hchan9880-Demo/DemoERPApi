INSERT INTO Users
(
 Username,
 PasswordHash,
 Role
)
VALUES
(
 'admin',
 'Password123',
 'Admin'
);


/*
SELECT *
FROM Users
WHERE Username=@Username


DELETE FROM Users;
SELECT * FROM Users;
INSERT INTO Users
(
    Username,
    PasswordHash,
    Role
)
VALUES
(
    'admin1',
    'HASHED_PASSWORD_HERE',
    'Admin'
);

INSERT INTO Users
(
    Username,
    PasswordHash,
    Role
)
VALUES
(
    'qauser',
    'HASHED_PASSWORD_HERE',
    'QA'
);


dotnet add package BCrypt.Net-Next
dotnet list package

BCrypt.Net-Next



Generate Password Hash

Temporary console code:

using BCrypt.Net;

Console.WriteLine(
    BCrypt.Net.BCrypt.HashPassword("Password123")
);
*/