using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using HappyMapper.Text;

namespace HappyMapper.Compilation
{
    public class Tools
    {
        public static Type ToCollectionDelegateType(TypeMap map)
        {
            var singleDelegateType = map.MapDelegateType;

            var srcType = singleDelegateType.GenericTypeArguments[0];
            var destType = singleDelegateType.GenericTypeArguments[1];

            var srcCollType = typeof(ICollection<>).MakeGenericType(srcType);
            var destCollType = typeof(ICollection<>).MakeGenericType(destType);

            return typeof(Func<,,>).MakeGenericType(srcCollType, destCollType, destCollType);
        }

        public static Type ToCollectionUntypedDelegateType(TypeMap map)
        {
            var singleDelegateType = map.MapDelegateType;

            //var srcType = singleDelegateType.GenericTypeArguments[0];
            //var destType = singleDelegateType.GenericTypeArguments[1];

            //var srcCollType = typeof(object);
            //var destCollType = typeof(ICollection<>).MakeGenericType(destType);

            //return typeof(Action<,>).MakeGenericType(typeof(object), typeof(object));
            return typeof(Action<object, object>);
        }

        public static Delegate CreateDelegate(Type mapDelegateType, Assembly assembly, CodeFile codeFile)
        {
            var type = assembly.GetType(codeFile.ClassFullName);

            var @delegate = Delegate.CreateDelegate(mapDelegateType, type, codeFile.MapperMethodName);

            return @delegate;
        }
    }
}