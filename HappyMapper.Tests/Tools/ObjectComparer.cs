using System.Collections.Generic;
using System.Linq;

namespace HappyMapper.Tests.Tools
{
    /// <summary>
    /// Shallow memberwise comparison.
    /// </summary>
    public static class ObjectComparer
    {
        public static CompareResult AreEqual(object src, object dest)
        {
            if (src == null) return new CompareResult("src == null");
            if (dest == null) return new CompareResult("dest == null");

            var srcProperties = src.GetType().GetProperties();
            var destProperties = dest.GetType().GetProperties();

            var errors = new List<string>();

            foreach (var srcProperty in srcProperties)
            {
                var destProperty = destProperties.First(p => p.Name == srcProperty.Name);

                var srcVal = srcProperty.GetValue(src);
                var destVal = destProperty.GetValue(dest);

                if (srcVal == null && destVal == null) continue;

                bool areEqual = srcVal.Equals(destVal);

                if (areEqual) continue;

                if (destProperty.PropertyType.IsClass)
                {
                    var childResult = AreEqual(srcVal, destVal);
                    errors.AddRange(childResult.Errors);
                    continue;
                }
                errors.Add($"{src.GetType().Name}.{srcProperty.Name}: Expected {srcVal} actual {destVal}");
            }

            return new CompareResult(errors);
        }
    }
}