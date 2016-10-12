using System;
using System.Collections.Generic;

namespace HappyMapper.Text
{
    //https://msdn.microsoft.com/en-us/library/yht2cx7b.aspx
    internal class ExplicitCastChecker
    {
        /// <summary>
        /// Returns true if explicit cast is possible.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool CanCast(Type from, Type to)
        {
            List<Type> list;
            if (Conversions.TryGetValue(from, out list))
            {
                if (list.Contains(to))
                    return true;
            }

            return false;
        }

        static readonly Dictionary<Type, List<Type>> Conversions = new Dictionary<Type, List<Type>>();

        static ExplicitCastChecker()
        {
            Conversions.Add(typeof(sbyte), new List<Type> { typeof(byte), typeof(ushort), typeof(uint), typeof(ulong), typeof(char) });
            Conversions.Add(typeof(byte), new List<Type> { typeof(sbyte), typeof(char) });
            Conversions.Add(typeof(short), new List<Type> { typeof(sbyte), typeof(byte), typeof(ushort), typeof(uint), typeof(ulong), typeof(char) });
            Conversions.Add(typeof(ushort), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(char) });
            Conversions.Add(typeof(int), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(uint), typeof(ulong), typeof(char) });
            Conversions.Add(typeof(uint), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(char) });
            Conversions.Add(typeof(long), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(ulong), typeof(char) });
            Conversions.Add(typeof(ulong), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(char) });
            Conversions.Add(typeof(char), new List<Type> { typeof(sbyte), typeof(byte), typeof(short) });
            Conversions.Add(typeof(float), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(decimal) });
            Conversions.Add(typeof(double), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(float), typeof(decimal) });
            Conversions.Add(typeof(decimal), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(float), typeof(double) });
        }
    }
}