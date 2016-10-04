using System.Reflection;

namespace OrdinaryMapper.Text
{
    public interface IStorageBuilder
    {
        string BuildCode();

        void InitStorage(Assembly assembly);
    }
}