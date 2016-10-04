using System;
using NUnit.Framework;

namespace HappyMapper.Tests.Tools
{
    public static class TypeExtentions
    {
        public static void ShouldBeThrownBy(this Type exType, Action action)
        {
            Exception e = null;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                e = ex;
            }

            Assert.IsNotNull(e);
            Assert.IsInstanceOf(exType, e);
        }

        public static void ShouldNotBeThrownBy(this Type exception, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (exception.IsInstanceOfType(ex))
                {
                    throw new Exception(string.Format("Expected no exception of type {0} to be thrown.", exception), ex);
                }
            }
        }
    }
}