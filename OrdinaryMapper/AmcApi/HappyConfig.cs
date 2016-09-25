﻿using System;
using System.Collections.Generic;
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
            var compiler = new Compiler();

            var delegates = compiler.CompileMapsToAssembly(Configuration, TypeMaps);

            return new HappyMapper(delegates);
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