using System;
using System.Collections.Generic;

namespace AutoMapper.Extended.Net4.SharedTools
{
    //keep it public - the method Add() is used in generated code.
    public static class CollectionExtensions
    {
        public static void Add<T>(this ICollection<T> collection, int count, Func<T> creator)
        {
            for (int i = 0; i < count; i++)
            {
                collection.Add(creator());
            }
        }
    }
}