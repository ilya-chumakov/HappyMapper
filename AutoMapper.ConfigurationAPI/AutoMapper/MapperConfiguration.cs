using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.ConfigurationAPI.Configuration;
using AutoMapper.ConfigurationAPI.Mappers;
using AutoMapper.ConfigurationAPI.QueryableExtensions;
using AutoMapper.ConfigurationAPI.QueryableExtensions.Impl;

namespace AutoMapper.ConfigurationAPI
{
    using UntypedMapperFunc = System.Func<object, object, ResolutionContext, object>;

    public class MapperConfiguration : IConfigurationProvider
    {
        private readonly IEnumerable<IObjectMapper> _mappers;
        public TypeMapRegistry TypeMapRegistry { get; } = new TypeMapRegistry();
        private readonly ConcurrentDictionary<TypePair, TypeMap> _typeMapPlanCache = new ConcurrentDictionary<TypePair, TypeMap>();
        private readonly ConcurrentDictionary<MapRequest, MapperFuncs> _mapPlanCache = new ConcurrentDictionary<MapRequest, MapperFuncs>();
        private readonly ConfigurationValidator _validator;
        private readonly Func<TypePair, TypeMap> _getTypeMap;
        private readonly Func<MapRequest, MapperFuncs> _createMapperFuncs;

        public MapperConfiguration(MapperConfigurationExpression configurationExpression)
            : this(configurationExpression, MapperRegistry.Mappers)
        {
            
        }

        public MapperConfiguration(MapperConfigurationExpression configurationExpression, IEnumerable<IObjectMapper> mappers)
        {
            _mappers = mappers;
            _getTypeMap = GetTypeMap;
            _createMapperFuncs = CreateMapperFuncs;

            _validator = new ConfigurationValidator(this);
            ExpressionBuilder = new ExpressionBuilder(this);

            Configuration = configurationExpression;

            Seal(Configuration);
        }

        public MapperConfiguration(Action<IMapperConfigurationExpression> configure) : this(configure, MapperRegistry.Mappers)
        {
        }

        public MapperConfiguration(Action<IMapperConfigurationExpression> configure, IEnumerable<IObjectMapper> mappers)
            : this(Build(configure), mappers)
        {
        }

        public IExpressionBuilder ExpressionBuilder { get; }

        public Func<Type, object> ServiceCtor { get; private set; }

        public bool AllowNullDestinationValues { get; private set; }

        public bool AllowNullCollections { get; private set; }

        public IConfiguration Configuration { get; }

        public Func<TSource, TDestination, ResolutionContext, TDestination> GetMapperFunc<TSource, TDestination>(TypePair types)
        {
            var key = new TypePair(typeof(TSource), typeof(TDestination));
            var mapRequest = new MapRequest(key, types);
            return (Func<TSource, TDestination, ResolutionContext, TDestination>)GetMapperFunc(mapRequest);
        }

        public Delegate GetMapperFunc(MapRequest mapRequest)
        {
            return _mapPlanCache.GetOrAdd(mapRequest, _createMapperFuncs).Typed;
        }

        public UntypedMapperFunc GetUntypedMapperFunc(MapRequest mapRequest)
        {
            return _mapPlanCache.GetOrAdd(mapRequest, _createMapperFuncs).Untyped;
        }

        private MapperFuncs CreateMapperFuncs(MapRequest mapRequest)
        {
            var typeMap = ResolveTypeMap(mapRequest.RuntimeTypes);
            if (typeMap != null)
            {
                return new MapperFuncs(mapRequest, typeMap);
            }
            var mapperToUse = _mappers.FirstOrDefault(om => om.IsMatch(mapRequest.RuntimeTypes));
            return new MapperFuncs(mapRequest, mapperToUse, this);
        }

        public TypeMap[] GetAllTypeMaps() => TypeMapRegistry.TypeMaps.ToArray();

        public TypeMap FindTypeMapFor(Type sourceType, Type destinationType) => FindTypeMapFor(new TypePair(sourceType, destinationType));

        public TypeMap FindTypeMapFor<TSource, TDestination>() => FindTypeMapFor(new TypePair(typeof(TSource), typeof(TDestination)));

        public TypeMap FindTypeMapFor(TypePair typePair) => TypeMapRegistry.GetTypeMap(typePair);

        public TypeMap ResolveTypeMap(Type sourceType, Type destinationType)
        {
            var typePair = new TypePair(sourceType, destinationType);

            return ResolveTypeMap(typePair);
        }

        public TypeMap ResolveTypeMap(TypePair typePair)
        {
            var typeMap = _typeMapPlanCache.GetOrAdd(typePair, _getTypeMap);
            if(Configuration.CreateMissingTypeMaps && typeMap != null && typeMap.MapExpression == null)
            {
                lock(typeMap)
                {
                    typeMap.Seal(TypeMapRegistry, this);
                }
            }
            return typeMap;
        }

        private TypeMap GetTypeMap(TypePair initialTypes)
        {
            TypeMap typeMap;
            foreach(var types in initialTypes.GetRelatedTypePairs())
            {
                if(_typeMapPlanCache.TryGetValue(types, out typeMap))
                {
                    return typeMap;
                }
                typeMap = FindTypeMapFor(types);
                if(typeMap != null)
                {
                    return typeMap;
                }
                typeMap = FindClosedGenericTypeMapFor(types, initialTypes);
                if(typeMap != null)
                {
                    return typeMap;
                }
                if(!CoveredByObjectMap(initialTypes))
                {
                    typeMap = FindConventionTypeMapFor(types);
                    if(typeMap != null)
                    {
                        return typeMap;
                    }
                }
            }
            return null;
        }

        public void AssertConfigurationIsValid(TypeMap typeMap)
        {
            _validator.AssertConfigurationIsValid(Enumerable.Repeat(typeMap, 1));
        }

        public void AssertConfigurationIsValid(string profileName)
        {
            _validator.AssertConfigurationIsValid(TypeMapRegistry.TypeMaps.Where(typeMap => typeMap.Profile.ProfileName == profileName));
        }

        public void AssertConfigurationIsValid<TProfile>()
            where TProfile : Profile, new()
        {
            AssertConfigurationIsValid(new TProfile().ProfileName);
        }

        public void AssertConfigurationIsValid()
        {
            _validator.AssertConfigurationIsValid(TypeMapRegistry.TypeMaps.Where(tm => !tm.SourceType.IsGenericTypeDefinition() && !tm.DestinationType.IsGenericTypeDefinition()));
        }

        public IMapper CreateMapper() => new Mapper(this);

        public IMapper CreateMapper(Func<Type, object> serviceCtor) => new Mapper(this, serviceCtor);

        public IEnumerable<IObjectMapper> GetMappers() => _mappers;

        private static MapperConfigurationExpression Build(Action<IMapperConfigurationExpression> configure)
        {
            var expr = new MapperConfigurationExpression();
            configure(expr);
            return expr;
        }

        private void Seal(IConfiguration configuration)
        {
            ServiceCtor = configuration.ServiceCtor;
            AllowNullDestinationValues = configuration.AllowNullDestinationValues;
            AllowNullCollections = configuration.AllowNullCollections;

            var derivedMaps = new List<Tuple<TypePair, TypeMap>>();
            var redirectedTypes = new List<Tuple<TypePair, TypePair>>();

            foreach (var profile in configuration.Profiles.Cast<IProfileConfiguration>())
            {
                profile.Register(TypeMapRegistry);
            }

            foreach (var action in configuration.AllTypeMapActions)
            {
                foreach (var typeMap in TypeMapRegistry.TypeMaps)
                {
                    var expression = new MappingExpression(typeMap.TypePair, typeMap.ConfiguredMemberList);

                    action(typeMap, expression);

                    expression.Configure(typeMap.Profile, typeMap);
                }
            }

            foreach (var action in configuration.AllPropertyMapActions)
            {
                foreach (var typeMap in TypeMapRegistry.TypeMaps)
                {
                    foreach (var propertyMap in typeMap.GetPropertyMaps())
                    {
                        var memberExpression = new MappingExpression.MemberConfigurationExpression(propertyMap.DestMember, typeMap.SourceType);

                        action(propertyMap, memberExpression);

                        memberExpression.Configure(typeMap);
                    }
                }
            }

            foreach (var profile in configuration.Profiles.Cast<IProfileConfiguration>())
            {
                profile.Configure(TypeMapRegistry);
            }

            foreach (var typeMap in TypeMapRegistry.TypeMaps)
            {
                _typeMapPlanCache[typeMap.TypePair] = typeMap;

                if (typeMap.DestinationTypeOverride != null)
                {
                    redirectedTypes.Add(Tuple.Create(typeMap.TypePair, new TypePair(typeMap.SourceType, typeMap.DestinationTypeOverride)));
                }
                if (typeMap.SourceType.IsNullableType())
                {
                    var nonNullableTypes = new TypePair(Nullable.GetUnderlyingType(typeMap.SourceType), typeMap.DestinationType);
                    redirectedTypes.Add(Tuple.Create(nonNullableTypes, typeMap.TypePair));
                }
                derivedMaps.AddRange(GetDerivedTypeMaps(typeMap).Select(derivedMap => Tuple.Create(new TypePair(derivedMap.SourceType, typeMap.DestinationType), derivedMap)));
            }
            foreach (var redirectedType in redirectedTypes)
            {
                var derivedMap = FindTypeMapFor(redirectedType.Item2);
                if (derivedMap != null)
                {
                    _typeMapPlanCache[redirectedType.Item1] = derivedMap;
                }
            }
            foreach (var derivedMap in derivedMaps.Where(derivedMap => !_typeMapPlanCache.ContainsKey(derivedMap.Item1)))
            {
                _typeMapPlanCache[derivedMap.Item1] = derivedMap.Item2;
            }

            foreach (var typeMap in TypeMapRegistry.TypeMaps)
            {
                typeMap.Seal(TypeMapRegistry, this);
            }
        }


        private IEnumerable<TypeMap> GetDerivedTypeMaps(TypeMap typeMap)
        {
            foreach (var derivedTypes in typeMap.IncludedDerivedTypes)
            {
                var derivedMap = FindTypeMapFor(derivedTypes);
                if (derivedMap == null)
                {
                    throw QueryMapperHelper.MissingMapException(derivedTypes.SourceType, derivedTypes.DestinationType);
                }
                yield return derivedMap;
                foreach (var derivedTypeMap in GetDerivedTypeMaps(derivedMap))
                {
                    yield return derivedTypeMap;
                }
            }
        }

        private bool CoveredByObjectMap(TypePair typePair)
        {
            return GetMappers().FirstOrDefault(m => m.IsMatch(typePair)) != null;
        }

        private TypeMap FindConventionTypeMapFor(TypePair typePair)
        {
            var typeMap = Configuration.Profiles
                .Cast<IProfileConfiguration>()
                .Select(p => p.ConfigureConventionTypeMap(TypeMapRegistry, typePair))
                .FirstOrDefault(t => t != null);

            if(!Configuration.CreateMissingTypeMaps)
            {
                typeMap?.Seal(TypeMapRegistry, this);
            }

            return typeMap;
        }

        private TypeMap FindClosedGenericTypeMapFor(TypePair typePair, TypePair requestedTypes)
        {
            if (typePair.GetOpenGenericTypePair() == null)
                return null;

            var typeMap = Configuration.Profiles
                .Cast<IProfileConfiguration>()
                .Select(p => p.ConfigureClosedGenericTypeMap(TypeMapRegistry, typePair, requestedTypes))
                .FirstOrDefault(t => t != null);

            typeMap?.Seal(TypeMapRegistry, this);

            return typeMap;
        }

        struct MapperFuncs
        {
            private Lazy<UntypedMapperFunc> _untyped;

            public Delegate Typed { get; }

            public UntypedMapperFunc Untyped => _untyped.Value;

            public MapperFuncs(MapRequest mapRequest, TypeMap typeMap) : this(mapRequest, GenerateTypeMapExpression(mapRequest, typeMap))
            {
            }

            public MapperFuncs(MapRequest mapRequest, IObjectMapper mapperToUse, MapperConfiguration mapperConfiguration) : this(mapRequest, GenerateObjectMapperExpression(mapRequest, mapperToUse, mapperConfiguration))
            {
            }

            public MapperFuncs(MapRequest mapRequest, LambdaExpression typedExpression)
            {
                Typed = typedExpression.Compile();
                _untyped = new Lazy<UntypedMapperFunc>(() => Wrap(mapRequest, typedExpression).Compile());
            }

            private static Expression<UntypedMapperFunc> Wrap(MapRequest mapRequest, LambdaExpression typedExpression)
            {
                var sourceParameter = Expression.Parameter(typeof(object), "source");
                var destinationParameter = Expression.Parameter(typeof(object), "destination");
                var contextParameter = Expression.Parameter(typeof(ResolutionContext), "context");
                var requestedSourceType = mapRequest.RequestedTypes.SourceType;
                var requestedDestinationType = mapRequest.RequestedTypes.DestinationType;

                var destination = requestedDestinationType.IsValueType() ? Expression.Coalesce(destinationParameter, Expression.New(requestedDestinationType)) : (Expression)destinationParameter;
                // Invoking a delegate here
                return Expression.Lambda<UntypedMapperFunc>(
                            ExpressionExtensions.ToType(
                                Expression.Invoke(typedExpression, ExpressionExtensions.ToType(sourceParameter, requestedSourceType), ExpressionExtensions.ToType(destination, requestedDestinationType), contextParameter)
                                , typeof(object)),
                          sourceParameter, destinationParameter, contextParameter);
            }

            private static LambdaExpression GenerateTypeMapExpression(MapRequest mapRequest, TypeMap typeMap)
            {
                var mapExpression = typeMap.MapExpression;
                var typeMapSourceParameter = mapExpression.Parameters[0];
                var typeMapDestinationParameter = mapExpression.Parameters[1];
                var requestedSourceType = mapRequest.RequestedTypes.SourceType;
                var requestedDestinationType = mapRequest.RequestedTypes.DestinationType;

                if (typeMapSourceParameter.Type != requestedSourceType || typeMapDestinationParameter.Type != requestedDestinationType)
                {
                    var requestedSourceParameter = Expression.Parameter(requestedSourceType, "source");
                    var requestedDestinationParameter = Expression.Parameter(requestedDestinationType, "typeMapDestination");
                    var contextParameter = Expression.Parameter(typeof(ResolutionContext), "context");

                    mapExpression = Expression.Lambda(ExpressionExtensions.ToType(Expression.Invoke(typeMap.MapExpression,
                        ExpressionExtensions.ToType(requestedSourceParameter, typeMapSourceParameter.Type),
                        ExpressionExtensions.ToType(requestedDestinationParameter, typeMapDestinationParameter.Type),
                        contextParameter
                        ), mapRequest.RuntimeTypes.DestinationType),
                        requestedSourceParameter, requestedDestinationParameter, contextParameter);
                }

                return mapExpression;
            }

            private static readonly ConstructorInfo ExceptionConstructor = typeof(AutoMapperMappingException).GetConstructors().Single(c => c.GetParameters().Length == 3);

            private static LambdaExpression GenerateObjectMapperExpression(MapRequest mapRequest, IObjectMapper mapperToUse, MapperConfiguration mapperConfiguration)
            {
                var destinationType = mapRequest.RequestedTypes.DestinationType;

                var source = Expression.Parameter(mapRequest.RequestedTypes.SourceType, "source");
                var destination = Expression.Parameter(destinationType, "mapperDestination");
                var context = Expression.Parameter(typeof(ResolutionContext), "context");
                LambdaExpression fullExpression;
                if (mapperToUse == null)
                {
                    var message = Expression.Constant("Missing type map configuration or unsupported mapping.");
                    fullExpression = Expression.Lambda(Expression.Block(Expression.Throw(Expression.New(ExceptionConstructor, message, Expression.Constant(null, typeof(Exception)), Expression.Constant(mapRequest.RequestedTypes))), Expression.Default(destinationType)), source, destination, context);
                }
                else
                {
                    var map = mapperToUse.MapExpression(mapperConfiguration.TypeMapRegistry, mapperConfiguration, null, ExpressionExtensions.ToType(source, mapRequest.RuntimeTypes.SourceType), destination, context);
                    var mapToDestination = Expression.Lambda(ExpressionExtensions.ToType(map, destinationType), source, destination, context);
                    fullExpression = TryCatch(mapToDestination, source, destination, context, mapRequest.RequestedTypes);
                }
                return fullExpression;
            }

            private static LambdaExpression TryCatch(LambdaExpression mapExpression, ParameterExpression source, ParameterExpression destination, ParameterExpression context, TypePair types)
            {
                var exception = Expression.Parameter(typeof(Exception), "ex");

                return Expression.Lambda(Expression.TryCatch(mapExpression.Body,
                    Expression.MakeCatchBlock(typeof(Exception), exception, Expression.Block(
                        Expression.Throw(Expression.New(ExceptionConstructor, Expression.Constant("Error mapping types."), exception, Expression.Constant(types))),
                        Expression.Default(destination.Type)), null)),
                    source, destination, context);
            }
        }
    }
}
