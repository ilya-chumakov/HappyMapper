using System.Reflection;

namespace HappyMapper.Text
{
    internal interface IStorageBuilder
    {
        string BuildCode();

        void InitStorage(Assembly assembly);
    }
}