using System;
using System.Collections.Generic;

using HappyMapper.AutoMapper.ConfigurationAPI;
using HappyMapper.AutoMapper.ConfigurationAPI.Configuration;
using HappyMapper.Compilation;

namespace HappyMapper
{
    public class HappyConfig
    {
        private readonly MapperConfiguration AutoMapperCfg = null;

        public IDictionary<TypePair, TypeMap> TypeMaps => AutoMapperCfg.TypeMapRegistry.TypeMapsDictionary;
        public MapperConfigurationExpression Configuration => (MapperConfigurationExpression)AutoMapperCfg.Configuration;

        public HappyConfig(Action<IMapperConfigurationExpression> configurationExpression)
        {
            AutoMapperCfg = new MapperConfiguration(configurationExpression); 
        }

        public Mapper CompileMapper()
        {
            var compiler = new Compiler(Configuration, TypeMaps);

            var delegates = compiler.CompileMapsToAssembly();

            return new Mapper(delegates);
        }

        public void AssertConfigurationIsValid()
        {
            try
            {
                AutoMapperCfg.AssertConfigurationIsValid();
            }
            catch (AutoMapperConfigurationException amce)
            {
                throw new HappyMapperConfigurationException(amce);
            }
        }
    }
}