using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using OrdinaryMapper.MemberMapping;

namespace OrdinaryMapper
{

    public class MapperConfiguration //: IConfigurationProvider
    {
        public MapperConfigurationExpression Configuration { get; }

        public MapperConfiguration(MapperConfigurationExpression configurationExpression)
        {
            Configuration = configurationExpression;

            Seal(Configuration);
        }

        public Mapper CompileMapper()
        {
            var delegates = Compiler.CompileToAssembly(Configuration, TypeMaps);

            return new Mapper(delegates);
        }

        private readonly TypeMapRegistry _typeMapRegistry = new TypeMapRegistry();
        
        public IDictionary<TypePair, TypeMap> TypeMaps => _typeMapRegistry.TypeMapsDictionary;

        private void Seal(MapperConfigurationExpression configuration)
        {
            configuration.Register(_typeMapRegistry);

            foreach (var typeMap in _typeMapRegistry.TypeMaps)
            {
                typeMap.Seal(_typeMapRegistry, this);
            }
        }

        public MapperConfiguration(Action<MapperConfigurationExpression> configurationExpression) 
            : this(Build(configurationExpression))
        {
        }

        public static MapperConfigurationExpression Build(Action<MapperConfigurationExpression> configure)
        {
            var expr = new MapperConfigurationExpression();
            configure(expr);
            return expr;
        }

    }

    public interface ITypeMapConfiguration
    {
        bool IsOpenGeneric { get; }
        void Configure(MapperConfigurationExpression profile, TypeMap typeMap);
        Type SrcType { get; }
        Type DestType { get; }
        TypePair Types { get; }
    }

    /// <summary>
    /// Profile
    /// </summary>
    public class MapperConfigurationExpression
    {
        private readonly List<ITypeMapConfiguration> _typeMapConfigs = new List<ITypeMapConfiguration>();
        private readonly TypeMapFactory _typeMapFactory = new TypeMapFactory();

        public Func<PropertyInfo, bool> ShouldMapProperty { get; set; } = p => p.IsPublic();

        public Func<FieldInfo, bool> ShouldMapField { get; set; } = f => f.IsPublic();

        public MapperConfigurationExpression()
        {
            _memberConfigurations.Add(new MemberConfigurationConv()
                //.AddMember<NameSplitMember>()
                //.AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, "Get"))
                );
        }

        public void Register(TypeMapRegistry typeMapRegistry)
        {
            //foreach (var config in _typeMapConfigs.Where(c => !c.IsOpenGeneric))
            foreach (var config in _typeMapConfigs.Where(c => !c.IsOpenGeneric))
            {
                BuildTypeMap(typeMapRegistry, config);

                //if (config.ReverseTypeMap != null)
                //{
                //    BuildTypeMap(typeMapRegistry, config.ReverseTypeMap);
                //}
            }
        }


        private void BuildTypeMap(TypeMapRegistry typeMapRegistry, ITypeMapConfiguration config)
        {
            //create full property-to-property maps
            var typeMap = _typeMapFactory.CreateTypeMap(config.SrcType, config.DestType, this);

            //apply selected options
            config.Configure(this, typeMap);

            typeMapRegistry.RegisterTypeMap(typeMap);
        }

        public MappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            var mappingExp = new MappingExpression<TSource, TDestination>();

            _typeMapConfigs.Add(mappingExp);

            return mappingExp;
        }
        private readonly IList<IMemberConfigurationCONV> _memberConfigurations = new List<IMemberConfigurationCONV>();

        public IMemberConfigurationCONV DefaultMemberConfig => _memberConfigurations.First();

        public IEnumerable<IMemberConfigurationCONV> MemberConfigurations => _memberConfigurations;

        public IMemberConfigurationCONV AddMemberConfiguration()
        {
            var condition = new MemberConfigurationConv();
            _memberConfigurations.Add(condition);
            return condition;
        }
    }

    public interface IMemberConfiguration
    {
        void Configure(TypeMap typeMap);
        MemberInfo DestinationMember { get; }
    }

    public class MappingExpression<TSource, TDestination> //: MappingExpression<TSource, TDestination> //, 
        : ITypeMapConfiguration
    {
        private readonly List<IMemberConfiguration> _memberConfigurations = new List<IMemberConfiguration>();

        public MappingExpression()
            : this(typeof(TSource), typeof(TDestination))
        {
        }

        public MappingExpression(Type sourceType, Type destinationType)
        {
            Types = new TypePair(sourceType, destinationType);
            IsOpenGeneric = sourceType.IsGenericTypeDefinition() || destinationType.IsGenericTypeDefinition();
        }

        public MappingExpression<TSource, TDestination> ForMember<TMember>(
            Expression<Func<TDestination, TMember>> destinationMember,
            Action<MemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
        {
            var memberInfo = ReflectionHelper.FindProperty(destinationMember);
            return ForDestinationMember(memberInfo, memberOptions);
        }


        private MappingExpression<TSource, TDestination> ForDestinationMember<TMember>(
            MemberInfo destinationProperty,
            Action<MemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
        {
            //var expression = (MemberConfigurationExpression<TSource, TDestination, TMember>)
            //    CreateMemberConfigurationExpression<TMember>(destinationProperty, typeof(TSource));

            var expression = new MemberConfigurationExpression<TSource, TDestination, TMember>(destinationProperty, typeof(TSource));

            _memberConfigurations.Add(expression);

            memberOptions(expression);

            return this;
        }

        public bool IsOpenGeneric { get; }

        public void Configure(MapperConfigurationExpression profile, TypeMap typeMap)
        {
            typeMap.MapDelegateType = typeof (Action<TSource, TDestination>);

            foreach (var memberConfig in _memberConfigurations)
            {
                memberConfig.Configure(typeMap);
            }
        }

        public Type SrcType => Types.SrcType;
        public Type DestType => Types.DestType;
        public TypePair Types { get; }
    }


    public class MemberConfigurationExpression<TSource, TDestination, TMember> : IMemberConfiguration
    {
        private readonly MemberInfo _destinationMember;
        private readonly Type _sourceType;
        protected List<Action<PropertyMap>> PropertyMapActions { get; } = new List<Action<PropertyMap>>();

        public MemberConfigurationExpression(MemberInfo destinationMember, Type sourceType)
        {
            _destinationMember = destinationMember;
            _sourceType = sourceType;
        }

        public MemberInfo DestinationMember => _destinationMember;
        public void Ignore()
        {
            PropertyMapActions.Add(pm => pm.Ignored = true);
        }

        public void Configure(TypeMap typeMap)
        {
            var destMember = _destinationMember;

            if (destMember.DeclaringType.IsGenericType())
            {
                //destMember = typeMap.DestinationTypeDetails.PublicReadAccessors
                //    .First(m => m.Name == destMember.Name && m.GetMemberType() == destMember.GetMemberType());
                destMember = typeMap.DestType.GetProperties()
                    .First(m => m.Name == destMember.Name && m.GetMemberType() == destMember.GetMemberType());
            }

            var propertyMap = typeMap.FindOrCreatePropertyMapFor(destMember);

            foreach (var action in PropertyMapActions)
            {
                action(propertyMap);
            }
        }
    }
}