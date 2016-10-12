using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using HappyMapper.Compilation;

namespace HappyMapper.Text
{
    internal interface IFileBuilder
    {
        ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        TextResult Build(ImmutableDictionary<TypePair, CodeFile> parentFiles = null);

        void VisitDelegate(CompiledDelegate @delegate, TypeMap map, Assembly assembly, CodeFile file);
    }
}