using System;
using System.Text.RegularExpressions;

namespace AutoMapper.Extended.Net4
{
    public static class NamingTools
    {
        /// <summary>
        /// Replace all chars except alphanumeric ASCII
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static string ToAlphanumericOnly(string typeName)
        {
            return Regex.Replace(typeName, @"[^a-zA-Z0-9 -]", string.Empty);
        }

        public static string NewGuid(int? max = null)
        {
            string guid = Guid.NewGuid().ToString().Replace("-", "");

            if (max == null) return guid;

            return guid.Substring(0, max.Value);
        }
    }
}