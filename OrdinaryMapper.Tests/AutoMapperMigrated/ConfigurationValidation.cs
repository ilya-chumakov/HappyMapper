using AutoMapper;
using AutoMapper.UnitTests;
using NUnit.Framework;
using OrdinaryMapper.Tests.AutoMapperMigrated;

public class When_constructor_does_not_match : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public Dest(int blarg)
        {
            Value = blarg;
        }
        public int Value { get; set; }
    }

    protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());

    [Test]
    public void Should_throw()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(() => Configuration.AssertConfigurationIsValid());
    }
}

public class When_constructor_partially_matches : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public Dest(int value, int blarg)
        {
            Value = blarg;
        }

        public int Value { get; }
    }

    protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());

    [Test]
    public void Should_throw()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(() => Configuration.AssertConfigurationIsValid());
    }
}

public class When_testing_a_dto_with_mismatched_members : NonValidatingSpecBase
{
    public class ModelObject
    {
        public string Foo { get; set; }
        public string Barr { get; set; }
    }

    public class ModelDto
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
    }

    public class ModelObject2
    {
        public string Foo { get; set; }
        public string Barr { get; set; }
    }

    public class ModelDto2
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
        public string Bar1 { get; set; }
        public string Bar2 { get; set; }
        public string Bar3 { get; set; }
        public string Bar4 { get; set; }
    }

    public class ModelObject3
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
        public string Bar1 { get; set; }
        public string Bar2 { get; set; }
        public string Bar3 { get; set; }
        public string Bar4 { get; set; }
    }

    public class ModelDto3
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
    }


    protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>();
        cfg.CreateMap<ModelObject2, ModelDto2>();
        cfg.CreateMap<ModelObject3, ModelDto3>(MemberList.Source);
    });

    [Test]
    public void Should_fail_a_configuration_check()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Configuration.AssertConfigurationIsValid);
    }
}