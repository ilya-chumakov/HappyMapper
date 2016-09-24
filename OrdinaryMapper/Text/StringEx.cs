using System.Text.RegularExpressions;

namespace OrdinaryMapper
{
    public static class StringEx
    {
        /// <summary>
        /// Remove +
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static string NormalizeTypeName(this string typeName)
        {
            return typeName.Replace('+', '.');
        }

        public static string TemplateToCode(this string template, string src, string dest)
        {
            return template.Replace("{0}", src).Replace("{1}", dest);
        }

        public static string RemoveDoubleBraces(this string str)
        {
            return str.Replace("{{", "").Replace("}}", "");
        }
    }

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
    }
}