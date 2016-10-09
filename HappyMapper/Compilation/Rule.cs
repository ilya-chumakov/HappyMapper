using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using AutoMapper.ConfigurationAPI;
using HappyMapper.Text;

namespace HappyMapper.Compilation
{
    public class Rule
    {
        private TextResult _result;
        public TextBuilderRunner Runner { get; set; }
        public ITextBuilder Builder { get; set; }
        public List<Rule> Childs { get; set; } = new List<Rule>();

        public TextResult Result
        {
            get
            {
                if (_result == null) throw new NotSupportedException("Rule wasn't built to retrieve the result!");
                return _result;
            }
        }

        public Rule(TextBuilderRunner runner, ITextBuilder builder)
        {
            Runner = runner;
            Builder = builder;
        }

        public Rule With(ITextBuilder builder, Action<Rule> setChild = null)
        {
            var rule = new Rule(Runner, builder);

            Childs.Add(rule);
            Runner.AllRules.Add(rule);

            if (setChild != null) setChild(rule);

            return this;
        }

        public void Build(ImmutableDictionary<TypePair, CodeFile> parentFiles = null)
        {
            var files = (Builder?.CreateCodeFiles(parentFiles)).ToImmutableDictionary();

            var locations = Builder.GetLocations();

            _result = new TextResult() { DetectedLocations = locations, Files = files };
            
            foreach (var rule in Childs)
            {
                rule.Build(files);
            }
        }
    }
}