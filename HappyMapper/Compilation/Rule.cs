using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using AutoMapper.ConfigurationAPI;
using HappyMapper.Text;

namespace HappyMapper.Compilation
{
    public class Rule
    {
        public TextBuilderRegistry Registry { get; set; }
        public ITextBuilder Builder { get; set; }
        public List<Rule> Childs { get; set; } = new List<Rule>();
        public TextResult Result { get; set; }

        public Rule(TextBuilderRegistry registry, ITextBuilder builder)
        {
            Registry = registry;
            Builder = builder;
        }

        public Rule With(ITextBuilder builder, Action<Rule> setChild = null)
        {
            var rule = new Rule(Registry, builder);

            Childs.Add(rule);
            Registry.AllNodes.Add(rule);

            if (setChild != null) setChild(rule);

            return this;
        }

        public List<TextResult> Build(ImmutableDictionary<TypePair, CodeFile> parentFiles = null)
        {
            var files = (Builder?.CreateCodeFiles(parentFiles)).ToImmutableDictionary();

            var locations = (Builder.GetLocations());

            var results = new List<TextResult>();

            var result = new TextResult() { DetectedLocations = locations, Files = files };
            Result = result;
            
            results.Add(result);

            foreach (var node in Childs)
            {
                results.AddRange(node.Build(files));
            }


            return results;
        }
    }
}