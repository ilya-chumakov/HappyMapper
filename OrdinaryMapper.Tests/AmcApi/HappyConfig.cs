using System;
using System.Collections.Generic;
using AutoMapper;

namespace OrdinaryMapperAmcApi.Tests
{
    public class HappyConfig
    {
        private MapperConfiguration AutoMapperCfg = null;

        public IDictionary<TypePair, TypeMap> TypeMaps => AutoMapperCfg.TypeMapRegistry.TypeMapsDictionary;

        public HappyConfig(Action<IMapperConfigurationExpression> configurationExpression)
        {
            AutoMapperCfg = new MapperConfiguration(configurationExpression);
        }

        public Mapper CompileMapper()
        {
            var cfg = AutoMapperCfg.Configuration;


            var delegates = OrdinaryMapper.Compiler.CompileToAssembly(null, TypeMaps);

            return new Mapper(delegates);
        }
    }
}