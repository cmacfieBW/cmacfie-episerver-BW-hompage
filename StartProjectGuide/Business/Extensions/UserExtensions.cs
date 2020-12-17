using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using EPiServer.Security;
using System.Web.Providers.Entities;

namespace StartProjectGuide.Business.Extensions
{
    public static class UserExtensions
    {
        
        private static class Group
        {
            public static string Administrator = "Administrators";
            public static string Editor = "Editors";
            public static string User = "Users";
        }

        /// <summary>
        /// Checks if the current user has an administrative role
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsAdmin(this IPrincipal user)
        {
            return PrincipalInfo.Current.RoleList.Contains(Group.Administrator);
        }
    }
}