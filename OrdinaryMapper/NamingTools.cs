using System.Text.RegularExpressions;

namespace OrdinaryMapper
{
    public static class NamingTools
    {
        /// <summary>
        /// Replace all chars except ASCII
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static string ToAlphanumericOnly(string typeName)
        {
            return Regex.Replace(typeName, @"[^a-zA-Z0-9 -]", string.Empty);
        }
    }
}