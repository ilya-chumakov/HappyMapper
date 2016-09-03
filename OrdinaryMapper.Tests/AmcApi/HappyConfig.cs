using System;
using System.Collections.Generic;
using AutoMapper;
using AutoMapper.Configuration;
using OrdinaryMapper;

namespace OrdinaryMapperAmcApi.Tests
{
    public class HappyConfig
    {
        private MapperConfiguration AutoMapperCfg = null;

        public IDictionary<TypePair, TypeMap> TypeMaps => AutoMapperCfg.TypeMapRegistry.TypeMapsDictionary;
        public MapperConfigurationExpression Configuration => (MapperConfigurationExpression)AutoMapperCfg.Configuration;

        public HappyConfig(Action<IMapperConfigurationExpression> configurationExpression)
        {
            AutoMapperCfg = new MapperConfiguration(configurationExpression);
        }

        public HappyMapper CompileMapper()
        {
            var cfg = AutoMapperCfg.Configuration;

            var delegates = Compiler.CompileToAssembly(null, TypeMaps);

            return new HappyMapper(delegates);
        }
    }
}