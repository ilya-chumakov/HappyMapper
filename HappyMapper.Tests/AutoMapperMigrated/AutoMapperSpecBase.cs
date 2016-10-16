using System;
using HappyMapper.AutoMapper.ConfigurationAPI;
using NUnit.Framework;

namespace HappyMapper.Tests.AutoMapperMigrated
{
    public abstract class AutoMapperSpecBase : NonValidatingSpecBase
    {
        [Test]
        public void Should_have_valid_configuration()
        {
            Configuration.AssertConfigurationIsValid();
        }

    }

    public abstract class NonValidatingSpecBase : SpecBase
    {
        private IMapper mapper;

        protected abstract MapperConfiguration Configuration { get; }
        protected IConfigurationProvider ConfigProvider => Configuration;

        protected IMapper Mapper => mapper ?? (mapper = Configuration.CreateMapper());
    }

    public abstract class SpecBaseBase
    {
        public virtual void MainSetup()
        {
            Establish_context();
            Because_of();
        }

        public virtual void MainTeardown()
        {
            Cleanup();
        }

        protected virtual void Establish_context()
        {
        }

        protected virtual void Because_of()
        {
        }

        protected virtual void Cleanup()
        {
        }


        protected TType CreateDependency<TType>()
            where TType : class
        {
            throw new NotImplementedException();
            //return MockRepository.GenerateMock<TType>();
        }

        protected TType CreateStub<TType>() where TType : class
        {
            throw new NotImplementedException();
            //return MockRepository.GenerateStub<TType>();
        }
    }
    public abstract class SpecBase : SpecBaseBase, IDisposable
    {
        protected SpecBase()
        {
            MainSetup();
        }

        public void Dispose()
        {
            MainTeardown();
        }
    }

}

