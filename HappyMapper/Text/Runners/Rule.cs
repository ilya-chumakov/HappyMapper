using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using HappyMapper.Compilation;

namespace HappyMapper.Text
{
    internal class Rule
    {
        private TextResult _result;
        public FileBuilderRunner Runner { get; set; }
        public IFileBuilder Builder { get; set; }
        public List<Rule> Childs { get; set; } = new List<Rule>();

        public TextResult Result
        {
            get
            {
                if (_result == null) throw new NotSupportedException("Rule wasn't built to retrieve the result!");
                return _result;
            }
        }

        public Rule(FileBuilderRunner runner, IFileBuilder builder)
        {
            Runner = runner;
            Builder = builder;
        }

        public Rule With(IFileBuilder builder, Action<Rule> setChild = null)
        {
            var rule = new Rule(Runner, builder);

            Childs.Add(rule);
            Runner.AllRules.Add(rule);

            if (setChild != null) setChild(rule);

            return this;
        }

        public void Build(ImmutableDictionary<TypePair, CodeFile> parentFiles = null)
        {
            _result = Builder.Build(parentFiles);

            foreach (var rule in Childs)
            {
                rule.Build(_result.Files);
            }
        }

        public void VisitDelegate(CompiledDelegate @delegate, TypeMap map, Assembly assembly)
        {
            Builder.VisitDelegate(@delegate, map, assembly, Result.Files[map.TypePair]);
        }
    }
}