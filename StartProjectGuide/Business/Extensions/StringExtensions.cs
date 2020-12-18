using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StartProjectGuide.Business.Extensions
{
    public static class StringExtensions
    {

        /// <summary>
        /// Capitalizes first letter of a string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string CapitalizeFirstLetter(this String s)
        {
            return s.First().ToString().ToUpper() + s.Substring(1);
        }
    }
}