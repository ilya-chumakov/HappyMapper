using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using HappyMapper.Compilation;

namespace HappyMapper.Text
{
    public interface ITextBuilder
    {
        ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        Dictionary<TypePair, CodeFile> CreateCodeFiles(ImmutableDictionary<TypePair, CodeFile> parentFiles = null);

        HashSet<string> GetLocations();

        void VisitDelegate(CompiledDelegate @delegate, TypeMap mapDelegateType, Assembly assembly, CodeFile file);
    }
}