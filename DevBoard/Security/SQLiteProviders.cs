using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;
using System.Web.Security;

namespace DevBoard.Security
{
    public class SQLiteMembershipProvider : MembershipProvider
    {
        private string _connectionString;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (string.IsNullOrEmpty(name))
                name = "SQLiteMembershipProvider";

            base.Initialize(name, config);

            string connectionStringName = config["connectionStringName"];
            if (string.IsNullOrEmpty(connectionStringName))
                throw new ProviderException("Connection string name not specified");

            var connectionStringSettings = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionStringSettings == null || string.IsNullOrEmpty(connectionStringSettings.ConnectionString))
                throw new ProviderException("Connection string not found");

            _connectionString = connectionStringSettings.ConnectionString;
        }

        public override bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT PasswordHash, PasswordSalt FROM Users WHERE Username = @Username", conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader["PasswordHash"].ToString();
                            string storedSalt = reader["PasswordSalt"].ToString();
                            string computedHash = HashPassword(password, storedSalt);
                            return storedHash == computedHash;
                        }
                    }
                }
            }
            return false;
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (GetUser(username, false) != null)
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }
            if (GetUserNameByEmail(email) != null)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            string salt = GenerateSalt();
            string hash = HashPassword(password, salt);

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("INSERT INTO Users (Username, Email, PasswordHash, PasswordSalt, IsApproved, CreatedDate) VALUES (@Username, @Email, @PasswordHash, @PasswordSalt, @IsApproved, @CreatedDate)", conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@PasswordHash", hash);
                    cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                    cmd.Parameters.AddWithValue("@IsApproved", isApproved ? 1 : 0);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        status = MembershipCreateStatus.Success;
                        return GetUser(username, false);
                    }
                    catch
                    {
                        status = MembershipCreateStatus.ProviderError;
                        return null;
                    }
                }
            }
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Username, Email, CreatedDate, IsApproved FROM Users WHERE Username = @Username", conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new MembershipUser(
                                Name,
                                reader["Username"].ToString(),
                                reader["Username"].ToString(),
                                reader["Email"].ToString(),
                                string.Empty,
                                string.Empty,
                                Convert.ToBoolean(reader["IsApproved"]),
                                false,
                                Convert.ToDateTime(reader["CreatedDate"]),
                                DateTime.MinValue,
                                DateTime.MinValue,
                                DateTime.MinValue,
                                DateTime.MinValue
                            );
                        }
                    }
                }
            }
            return null;
        }

        public override string GetUserNameByEmail(string email)
        {
             using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Username FROM Users WHERE Email = @Email", conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    return cmd.ExecuteScalar() as string;
                }
            }
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combined = Encoding.UTF8.GetBytes(password + salt);
                var hash = sha256.ComputeHash(combined);
                return Convert.ToBase64String(hash);
            }
        }

        private string GenerateSalt()
        {
            var bytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        // Minimal implementation for other overrides
        public override bool EnablePasswordRetrieval => false;
        public override bool EnablePasswordReset => true;
        public override bool RequiresQuestionAndAnswer => false;
        public override string ApplicationName { get; set; } = "DevBoard";
        public override int MaxInvalidPasswordAttempts => 5;
        public override int PasswordAttemptWindow => 10;
        public override bool RequiresUniqueEmail => true;
        public override MembershipPasswordFormat PasswordFormat => MembershipPasswordFormat.Hashed;
        public override int MinRequiredPasswordLength => 6;
        public override int MinRequiredNonAlphanumericCharacters => 0;
        public override string PasswordStrengthRegularExpression => string.Empty;

        public override bool ChangePassword(string username, string oldPassword, string newPassword) => throw new NotImplementedException();
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) => throw new NotImplementedException();
        public override bool DeleteUser(string username, bool deleteAllRelatedData) => throw new NotImplementedException();
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) => throw new NotImplementedException();
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) => throw new NotImplementedException();
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords) => throw new NotImplementedException();
        public override int GetNumberOfUsersOnline() => throw new NotImplementedException();
        public override string GetPassword(string username, string answer) => throw new NotImplementedException();
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline) => throw new NotImplementedException();
        public override string ResetPassword(string username, string answer) => throw new NotImplementedException();
        public override bool UnlockUser(string username) => throw new NotImplementedException();
        public override void UpdateUser(MembershipUser user) => throw new NotImplementedException();
    }

    public class SQLiteRoleProvider : RoleProvider
    {
        private string _connectionString;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            
            if (string.IsNullOrEmpty(name))
                name = "SQLiteRoleProvider";

            base.Initialize(name, config);

            string connectionStringName = config["connectionStringName"];
             if (string.IsNullOrEmpty(connectionStringName))
                throw new ProviderException("Connection string name not specified");

            var connectionStringSettings = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionStringSettings == null || string.IsNullOrEmpty(connectionStringSettings.ConnectionString))
                throw new ProviderException("Connection string not found");

            _connectionString = connectionStringSettings.ConnectionString;
        }

        public override bool IsUserInRole(string username, string roleName)
        {
             using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "SELECT Count(*) FROM UsersInRoles uir " +
                    "JOIN Users u ON uir.UserId = u.Id " +
                    "JOIN Roles r ON uir.RoleId = r.Id " +
                    "WHERE u.Username = @Username AND r.RoleName = @RoleName", conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@RoleName", roleName);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            var roles = new List<string>();
             using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "SELECT r.RoleName FROM Roles r " +
                    "JOIN UsersInRoles uir ON r.Id = uir.RoleId " +
                    "JOIN Users u ON uir.UserId = u.Id " +
                    "WHERE u.Username = @Username", conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(reader["RoleName"].ToString());
                        }
                    }
                }
            }
            return roles.ToArray();
        }

        public override void CreateRole(string roleName)
        {
             using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("INSERT INTO Roles (RoleName) VALUES (@RoleName)", conn))
                {
                    cmd.Parameters.AddWithValue("@RoleName", roleName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public override bool RoleExists(string roleName)
        {
             using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Count(*) FROM Roles WHERE RoleName = @RoleName", conn))
                {
                    cmd.Parameters.AddWithValue("@RoleName", roleName);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
             using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                foreach (string username in usernames)
                {
                    foreach (string roleName in roleNames)
                    {
                        // Get UserId and RoleId
                        long userId;
                        long roleId;

                        using (var cmd = new SQLiteCommand("SELECT Id FROM Users WHERE Username = @Username", conn))
                        {
                            cmd.Parameters.AddWithValue("@Username", username);
                            object result = cmd.ExecuteScalar();
                             if (result == null) continue; // User not found
                            userId = (long)result;
                        }

                        using (var cmd = new SQLiteCommand("SELECT Id FROM Roles WHERE RoleName = @RoleName", conn))
                        {
                            cmd.Parameters.AddWithValue("@RoleName", roleName);
                             object result = cmd.ExecuteScalar();
                            if (result == null) continue; // Role not found
                            roleId = (long)result;
                        }

                        // Insert match
                        using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO UsersInRoles (UserId, RoleId) VALUES (@UserId, @RoleId)", conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@RoleId", roleId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) => throw new NotImplementedException();
        public override string[] GetUsersInRole(string roleName) => throw new NotImplementedException();
        public override string[] GetAllRoles() => throw new NotImplementedException();
        public override string[] FindUsersInRole(string roleName, string usernameToMatch) => throw new NotImplementedException();
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) => throw new NotImplementedException();
        public override string ApplicationName { get; set; } = "DevBoard";
    }
}
