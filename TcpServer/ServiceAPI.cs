using System;
using System.Linq;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;

namespace TcpServer
{
    class ServiceAPI
    {
        [DllImport("netapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        static extern uint NetUserChangePassword(
            [MarshalAs(UnmanagedType.LPWStr)] string domainname,
            [MarshalAs(UnmanagedType.LPWStr)] string username,
            [MarshalAs(UnmanagedType.LPWStr)] string oldpassword,
            [MarshalAs(UnmanagedType.LPWStr)] string newpassword);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct USER_INFO_1
        {
            public string username;
            public string password;
            public int password_age;
            public int priv;
            public string homedir;
            public string comment;
            public int flag;
            public string scriptpath;
        }

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        static extern int NetUserAdd(
            [MarshalAs(UnmanagedType.LPWStr)] string servername,
            int level,
            ref USER_INFO_1 userinfo,
            out int parm_err);

        public string ChangePassword(string user, string oldpassword, string newpassword, string confirmpassword)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(oldpassword) || string.IsNullOrWhiteSpace(newpassword))
                return $"ERROR occurred! No empty string is valid.";

            if (!newpassword.Equals(confirmpassword))
                return $"ERROR occurred! Two inputted new passwords are different.";

            uint r = NetUserChangePassword("127.0.0.1", user, oldpassword, newpassword);
            if (r != 0)
                return $"ERROR[{r}] occurred! Unable to change password!";

            return $"Password is changed!";
        }

        public string AddUser(string admin, string pass, string user)
        {
            const int UF_DONT_EXPIRE_PASSWD = 0x10000;
            //const int UF_ACCOUNTDISABLE = 0x000002;

            //const int USER_PRIV_GUEST = 0; 
            const int USER_PRIV_USER = 1;
            //const int USER_PRIV_ADMIN = 2;

            string password = _generate();

            if (string.IsNullOrWhiteSpace(user))
                return $"ERROR occurred! No empty string is valid.";

            USER_INFO_1 userInfo1 = new USER_INFO_1()
            {
                username = user,
                password = password,
                password_age = -1,
                priv = USER_PRIV_USER,
                homedir = "",
                comment = "",
                flag = UF_DONT_EXPIRE_PASSWD,
                scriptpath = ""
            };

            using (PrincipalContext _ctx = new PrincipalContext(ContextType.Machine))
            {
                if (!_ctx.ValidateCredentials(admin, pass))
                    return $"Invalid authencation!";

                UserPrincipal _priv = UserPrincipal.FindByIdentity(_ctx, IdentityType.SamAccountName, admin);
                if (!_priv.GetAuthorizationGroups().Any(p => p.ToString() == "Administrators"))
                    return $"Invalid authencation!";
            }

            int output;
            int r = NetUserAdd("127.0.0.1", 1, ref userInfo1, out output);

            if (r != 0)
                return $"ERROR[{r}] occurred! Unable to create user";

            return $"User[{user}/{password}] is created!";
        }

        private string _generate(int length = 8)
        {
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }
    }
}
