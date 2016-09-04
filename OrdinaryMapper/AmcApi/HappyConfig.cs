using System;
using System.Collections.Generic;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;

namespace OrdinaryMapper.AmcApi
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
            var delegates = Compiler.CompileToAssembly(Configuration, TypeMaps);

            return new HappyMapper(delegates);
        }
    }
}