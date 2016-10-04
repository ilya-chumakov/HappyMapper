using System.Reflection;

namespace HappyMapper.Text
{
    public interface IStorageBuilder
    {
        string BuildCode();

        void InitStorage(Assembly assembly);
    }
}