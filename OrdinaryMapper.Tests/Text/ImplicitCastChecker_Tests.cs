using NUnit.Framework;

namespace OrdinaryMapper.Tests.Text
{
    public class ImplicitCastChecker_Tests
    {
        [Test]
        public void CanCast_IntToByte_ReturnsFalse()
        {
            Assert.IsFalse(ImplicitCastChecker.CanCast(typeof(int), typeof(byte)));
        } 

        [Test]
        public void CanCast_ByteToInt_ReturnsTrue()
        {
            Assert.IsTrue(ImplicitCastChecker.CanCast(typeof(byte), typeof(int)));
        } 
    }
}