using System.Collections.Generic;
using System.Collections.Immutable;
using AutoMapper.ConfigurationAPI;

namespace HappyMapper.Text
{
    public interface ITextBuilder
    {
        ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        Dictionary<TypePair, CodeFile> CreateCodeFiles();

        HashSet<string> GetLocations();
    }
}