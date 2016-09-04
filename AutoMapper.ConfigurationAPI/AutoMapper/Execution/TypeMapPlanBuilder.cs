using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.ConfigurationAPI.Configuration;
using AutoMapper.ConfigurationAPI.Mappers;

namespace AutoMapper.ConfigurationAPI.Execution
{
    public class TypeMapPlanBuilder
    {
        private static readonly Expression<Func<IRuntimeMapper, ResolutionContext>> CreateContext = mapper => new ResolutionContext(mapper.DefaultContext.Options, mapper);
        private static readonly Expression<Func<AutoMapperMappingException>> CtorExpression = () => new AutoMapperMappingException(null, null, default(TypePair), null, null);
        private static readonly Expression<Action<ResolutionContext>> IncTypeDepthInfo = ctxt => ctxt.IncrementTypeDepth(default(TypePair));
        private static readonly Expression<Action<ResolutionContext>> DecTypeDepthInfo = ctxt => ctxt.DecrementTypeDepth(default(TypePair));
        private static readonly Expression<Func<ResolutionContext, int>> GetTypeDepthInfo = ctxt => ctxt.GetTypeDepth(default(TypePair));

        private readonly IConfigurationProvider _configurationProvider;
        private readonly TypeMap _typeMap;
        private readonly TypeMapRegistry _typeMapRegistry;
        private readonly ParameterExpression _source;
        private readonly ParameterExpression _initialDestination;
        private readonly ParameterExpression _context;
        private readonly ParameterExpression _destination;

        public ParameterExpression Source => _source;
        public ParameterExpression Context => _context;

        public TypeMapPlanBuilder(IConfigurationProvider configurationProvider, TypeMapRegistry typeMapRegistry, TypeMap typeMap)
        {
            _configurationProvider = configurationProvider;
            _typeMapRegistry = typeMapRegistry;
            _typeMap = typeMap;
            _source = Expression.Parameter(typeMap.SourceType, "src");
            _initialDestination = Expression.Parameter(typeMap.DestinationTypeToUse, "dest");
            _context = Expression.Parameter(typeof(ResolutionContext), "ctxt");
            _destination = Expression.Variable(_initialDestination.Type, "typeMapDestination");
        }

        public LambdaExpression CreateMapperLambda()
        {
            if(_typeMap.SourceType.IsGenericTypeDefinition() || _typeMap.DestinationTypeToUse.IsGenericTypeDefinition())
            {
                return null;
            }
            var customExpression = TypeConverterMapper() ?? _typeMap.Substitution ?? _typeMap.CustomMapper ?? _typeMap.CustomProjection;
            if(customExpression != null)
            {
                return Expression.Lambda(customExpression.ReplaceParameters(_source, _initialDestination, _context), _source, _initialDestination, _context);
            }
            bool constructorMapping;

            var destinationFunc = CreateDestinationFunc(out constructorMapping);

            var assignmentFunc = CreateAssignmentFunc(destinationFunc, constructorMapping);

            var mapperFunc = CreateMapperFunc(assignmentFunc);

            var checkContext = CheckContext(_typeMap, _context);
            var lambaBody = (checkContext != null) ? new[] { checkContext, mapperFunc } : new[] { mapperFunc };

            return Expression.Lambda(Expression.Block(new[] { _destination }, lambaBody), _source, _initialDestination, _context);
        }

        private LambdaExpression TypeConverterMapper()
        {
            if(_typeMap.TypeConverterType == null)
            {
                return null;
            }
            Type type;
            if(_typeMap.TypeConverterType.IsGenericTypeDefinition())
            {
                var genericTypeParam = _typeMap.SourceType.IsGenericType()
                    ? _typeMap.SourceType.GetTypeInfo().GenericTypeArguments[0]
                    : _typeMap.DestinationTypeToUse.GetTypeInfo().GenericTypeArguments[0];
                type = _typeMap.TypeConverterType.MakeGenericType(genericTypeParam);
            }
            else
            {
                type = _typeMap.TypeConverterType;
            }
            // (src, dest, ctxt) => ((ITypeConverter<TSource, TDest>)ctxt.Options.CreateInstance<TypeConverterType>()).ToType(src, ctxt);
            var converterInterfaceType = typeof(ITypeConverter<,>).MakeGenericType(_typeMap.SourceType, _typeMap.DestinationTypeToUse);
            return Expression.Lambda(
                Expression.Call(
                    ExpressionExtensions.ToType(
                        Expression.Call(
                            Expression.MakeMemberAccess(_context, typeof(ResolutionContext).GetDeclaredProperty("Options")),
                            typeof(IMappingOperationOptions).GetDeclaredMethod("CreateInstance")
                                .MakeGenericMethod(type)
                            ),
                        converterInterfaceType),
                    converterInterfaceType.GetDeclaredMethod("Convert"),
                    _source, _initialDestination, _context
                    ),
                _source, _initialDestination, _context);
        }

        public static ConditionalExpression CheckContext(TypeMap typeMap, Expression context)
        {
            if(typeMap.MaxDepth > 0 || typeMap.PreserveReferences)
            {
                var mapper = Expression.Property(context, "Mapper");
                return Expression.IfThen(Expression.Property(context, "IsDefault"), Expression.Assign(context, Expression.Invoke(CreateContext, mapper)));
            }
            return null;
        }

        private Expression CreateDestinationFunc(out bool constructorMapping)
        {
            var newDestFunc = ExpressionExtensions.ToType(CreateNewDestinationFunc(out constructorMapping), _typeMap.DestinationTypeToUse);

            var getDest = _typeMap.DestinationTypeToUse.IsValueType()
                ? newDestFunc
                : Expression.Coalesce(_initialDestination, newDestFunc);

            Expression destinationFunc = Expression.Assign(_destination, getDest);

            if(_typeMap.PreserveReferences)
            {
                var dest = Expression.Variable(typeof(object), "dest");

                Expression valueBag = Expression.Property(_context, "InstanceCache");
                var set = Expression.Assign(Expression.Property(valueBag, "Item", _source), dest);
                var setCache =
                    Expression.IfThen(Expression.NotEqual(_source, Expression.Constant(null)), set);

                destinationFunc = Expression.Block(new[] { dest }, Expression.Assign(dest, destinationFunc), setCache, dest);
            }
            return destinationFunc;
        }

        private Expression CreateAssignmentFunc(Expression destinationFunc, bool constructorMapping)
        {
            var actions = new List<Expression>();
            foreach(var propertyMap in _typeMap.GetPropertyMaps())
            {
                if(!propertyMap.CanResolveValue())
                {
                    continue;
                }
                var property = TryPropertyMap(propertyMap);
                if(constructorMapping && _typeMap.ConstructorParameterMatches(propertyMap.DestMember.Name))
                {
                    property = Expression.IfThen(Expression.NotEqual(_initialDestination, Expression.Constant(null)), property);
                }
                actions.Add(property);
            }
            foreach(var beforeMapAction in _typeMap.BeforeMapActions)
            {
                actions.Insert(0, beforeMapAction.ReplaceParameters(_source, _destination, _context));
            }
            actions.Insert(0, destinationFunc);
            if(_typeMap.MaxDepth > 0)
            {
                actions.Insert(0, Expression.Call(_context, ((MethodCallExpression)IncTypeDepthInfo.Body).Method, Expression.Constant(_typeMap.TypePair)));
            }
            actions.AddRange(
                _typeMap.AfterMapActions.Select(
                    afterMapAction => afterMapAction.ReplaceParameters(_source, _destination, _context)));

            if(_typeMap.MaxDepth > 0)
            {
                actions.Add(Expression.Call(_context, ((MethodCallExpression)DecTypeDepthInfo.Body).Method, Expression.Constant(_typeMap.TypePair)));
            }

            actions.Add(_destination);

            return Expression.Block(actions);
        }

        private Expression CreateMapperFunc(Expression assignmentFunc)
        {
            var mapperFunc = assignmentFunc;

            if(_typeMap.Condition != null)
            {
                mapperFunc =
                    Expression.Condition(_typeMap.Condition.Body,
                        mapperFunc, Expression.Default(_typeMap.DestinationTypeToUse));
                //mapperFunc = (source, context, destFunc) => _condition(context) ? inner(source, context, destFunc) : default(TDestination);
            }

            if(_typeMap.MaxDepth > 0)
            {
                mapperFunc = Expression.Condition(
                    Expression.LessThanOrEqual(
                        Expression.Call(_context, ((MethodCallExpression)GetTypeDepthInfo.Body).Method, Expression.Constant(_typeMap.TypePair)),
                        Expression.Constant(_typeMap.MaxDepth)
                    ),
                    mapperFunc,
                    Expression.Default(_typeMap.DestinationTypeToUse));
                //mapperFunc = (source, context, destFunc) => context.GetTypeDepth(types) <= maxDepth ? inner(source, context, destFunc) : default(TDestination);
            }

            if(_typeMap.Profile.AllowNullDestinationValues && !_typeMap.SourceType.IsValueType())
            {
                mapperFunc =
                    Expression.Condition(Expression.Equal(_source, Expression.Default(_typeMap.SourceType)),
                        Expression.Default(_typeMap.DestinationTypeToUse), mapperFunc.RemoveIfNotNull(_source));
                //mapperFunc = (source, context, destFunc) => source == default(TSource) ? default(TDestination) : inner(source, context, destFunc);
            }

            if(_typeMap.PreserveReferences)
            {
                var cache = Expression.Variable(_typeMap.DestinationTypeToUse, "cachedDestination");

                var condition = Expression.Condition(
                    Expression.AndAlso(
                        Expression.NotEqual(_source, Expression.Constant(null)),
                        Expression.AndAlso(
                            Expression.Equal(_initialDestination, Expression.Constant(null)),
                            Expression.Call(Expression.Property(_context, "InstanceCache"),
                                typeof(Dictionary<object, object>).GetDeclaredMethod("ContainsKey"), _source)
                            )),
                    Expression.Assign(cache,
                        ExpressionExtensions.ToType(Expression.Property(Expression.Property(_context, "InstanceCache"), "Item", _source), _typeMap.DestinationTypeToUse)),
                    Expression.Assign(cache, mapperFunc)
                    );

                mapperFunc = Expression.Block(new[] { cache }, condition, cache);
            }
            return mapperFunc;
        }

        private Expression CreateNewDestinationFunc(out bool constructorMapping)
        {
            constructorMapping = false;
            if(_typeMap.DestinationCtor != null)
                return _typeMap.DestinationCtor.ReplaceParameters(_source, _context);

            if(_typeMap.ConstructDestinationUsingServiceLocator)
                return Expression.Call(Expression.MakeMemberAccess(_context, typeof(ResolutionContext).GetDeclaredProperty("Options")),
                    typeof(IMappingOperationOptions).GetDeclaredMethod("CreateInstance")
                        .MakeGenericMethod(_typeMap.DestinationTypeToUse)
                    );

            if(_typeMap.ConstructorMap?.CanResolve == true)
            {
                constructorMapping = true;
                return _typeMap.ConstructorMap.BuildExpression(this);
            }
#if NET45
            if(_typeMap.DestinationTypeToUse.IsInterface())
            {
                var ctor = Expression.Call(Expression.Constant(ObjectCreator.DelegateFactory), typeof(DelegateFactory).GetDeclaredMethod("CreateCtor", new[] { typeof(Type) }), Expression.Call(Expression.New(typeof(ProxyGenerator)), typeof(ProxyGenerator).GetDeclaredMethod("GetProxyType"), Expression.Constant(_typeMap.DestinationTypeToUse)));
                // We're invoking a delegate here
                return Expression.Invoke(ctor);
            }
#endif

            if(_typeMap.DestinationTypeToUse.IsAbstract())
                return Expression.Constant(null);

            if(_typeMap.DestinationTypeToUse.IsGenericTypeDefinition())
                return Expression.Constant(null);

            return DelegateFactory.GenerateConstructorExpression(_typeMap.DestinationTypeToUse);
        }

        private Expression TryPropertyMap(PropertyMap propertyMap)
        {
            var pmExpression = CreatePropertyMapFunc(propertyMap);

            if(pmExpression == null)
                return null;

            var exception = Expression.Parameter(typeof(Exception), "ex");

            var mappingExceptionCtor = ((NewExpression)CtorExpression.Body).Constructor;

            return Expression.TryCatch(Expression.Block(typeof(void), pmExpression),
                Expression.MakeCatchBlock(typeof(Exception), exception,
                    Expression.Throw(Expression.New(mappingExceptionCtor, Expression.Constant("Error mapping types."), exception, Expression.Constant(propertyMap.TypeMap.TypePair), Expression.Constant(propertyMap.TypeMap), Expression.Constant(propertyMap))), null));
        }

        private Expression CreatePropertyMapFunc(PropertyMap propertyMap)
        {
            var destMember = Expression.MakeMemberAccess(_destination, propertyMap.DestMember);

            Expression getter;

            var pi = propertyMap.DestMember as PropertyInfo;
            if(pi != null && pi.GetGetMethod(true) == null)
            {
                getter = Expression.Default(propertyMap.DestType);
            }
            else
            {
                getter = destMember;
            }

            Expression destValueExpr;
            if(propertyMap.UseDestinationValue)
            {
                destValueExpr = getter;
            }
            else
            {
                if(_initialDestination.Type.IsValueType())
                {
                    destValueExpr = Expression.Default(propertyMap.DestType);
                }
                else
                {
                    destValueExpr = Expression.Condition(Expression.Equal(_initialDestination, Expression.Constant(null)), Expression.Default(propertyMap.DestType), getter);
                }
            }

            var valueResolverExpr = BuildValueResolverFunc(propertyMap, getter);
            var resolvedValue = Expression.Variable(valueResolverExpr.Type, "resolvedValue");
            var setResolvedValue = Expression.Assign(resolvedValue, valueResolverExpr);
            valueResolverExpr = resolvedValue;

            if(propertyMap.DestType != null)
            {
                var typePair = new TypePair(valueResolverExpr.Type, propertyMap.DestType);
                valueResolverExpr = MapExpression(typePair, valueResolverExpr, propertyMap, destValueExpr);
            }
            else
            {
                valueResolverExpr = SetMap(propertyMap, valueResolverExpr, destValueExpr);
            }

            ParameterExpression propertyValue;
            Expression setPropertyValue;
            if(valueResolverExpr == resolvedValue)
            {
                propertyValue = resolvedValue;
                setPropertyValue = setResolvedValue;
            }
            else
            {
                propertyValue = Expression.Variable(valueResolverExpr.Type, "propertyValue");
                setPropertyValue = Expression.Assign(propertyValue, valueResolverExpr);
            }

            Expression mapperExpr;
            if(propertyMap.DestMember is FieldInfo)
            {
                mapperExpr = propertyMap.SrcType != propertyMap.DestType
                    ? Expression.Assign(destMember, ExpressionExtensions.ToType(propertyValue, propertyMap.DestType))
                    : Expression.Assign(getter, propertyValue);
            }
            else
            {
                var setter = ((PropertyInfo)propertyMap.DestMember).GetSetMethod(true);
                if(setter == null)
                {
                    mapperExpr = propertyValue;
                }
                else
                {
                    mapperExpr = Expression.Assign(destMember, ExpressionExtensions.ToType(propertyValue, propertyMap.DestType));
                }
            }

            if(propertyMap.Condition != null)
            {
                mapperExpr = Expression.IfThen(
                    propertyMap.Condition.ConvertReplaceParameters(
                        _source,
                        _destination,
                        ExpressionExtensions.ToType(propertyValue, propertyMap.Condition.Parameters[2].Type),
                        ExpressionExtensions.ToType(getter, propertyMap.Condition.Parameters[2].Type),
                        _context
                        ),
                    mapperExpr
                    );
            }

            mapperExpr = Expression.Block(new[] { setResolvedValue, setPropertyValue, mapperExpr }.Distinct());

            if(propertyMap.PreCondition != null)
            {
                mapperExpr = Expression.IfThen(
                    propertyMap.PreCondition.ConvertReplaceParameters(_source, _context),
                    mapperExpr
                    );
            }

            return Expression.Block(new[] { resolvedValue, propertyValue }.Distinct(), mapperExpr);
        }

        private Expression SetMap(PropertyMap propertyMap, Expression valueResolverExpression, Expression destinationValueExpression)
        {
            return ContextMap(new TypePair(valueResolverExpression.Type, propertyMap.DestType), valueResolverExpression, destinationValueExpression, _context);
        }

        private Expression BuildValueResolverFunc(PropertyMap propertyMap, Expression destValueExpr)
        {
            Expression valueResolverFunc;
            var destinationPropertyType = propertyMap.DestType;
            var valueResolverConfig = propertyMap.ValueResolverConfig;
            var typeMap = propertyMap.TypeMap;

            if(valueResolverConfig != null)
            {
                Expression ctor;
                Type resolverType;
                if(valueResolverConfig.Instance != null)
                {
                    ctor = Expression.Constant(valueResolverConfig.Instance);
                    resolverType = valueResolverConfig.Instance.GetType();
                }
                else
                {
                    ctor = Expression.Call(Expression.MakeMemberAccess(_context, typeof(ResolutionContext).GetDeclaredProperty("Options")),
                        typeof(IMappingOperationOptions).GetDeclaredMethod("CreateInstance")
                            .MakeGenericMethod(valueResolverConfig.Type)
                        );
                    resolverType = valueResolverConfig.Type;
                }

                if(valueResolverConfig.SourceMember != null)
                {
                    var sourceMember = valueResolverConfig.SourceMember.ReplaceParameters(_source);

                    var iResolverType =
                        resolverType.GetTypeInfo()
                            .ImplementedInterfaces.First(t => t.ImplementsGenericInterface(typeof(IMemberValueResolver<,,,>)));

                    var sourceResolverParam = iResolverType.GetGenericArguments()[0];
                    var destResolverParam = iResolverType.GetGenericArguments()[1];
                    var sourceMemberResolverParam = iResolverType.GetGenericArguments()[2];
                    var destMemberResolverParam = iResolverType.GetGenericArguments()[3];

                    valueResolverFunc =
                        ExpressionExtensions.ToType(Expression.Call(ExpressionExtensions.ToType(ctor, resolverType), resolverType.GetDeclaredMethod("Resolve"),
                            ExpressionExtensions.ToType(_source, sourceResolverParam),
                            ExpressionExtensions.ToType(_destination, destResolverParam),
                            ExpressionExtensions.ToType(sourceMember, sourceMemberResolverParam),
                            ExpressionExtensions.ToType(destValueExpr, destMemberResolverParam),
                            _context),
                            destinationPropertyType);
                }
                else if(valueResolverConfig.SourceMemberName != null)
                {
                    var sourceMember = Expression.MakeMemberAccess(_source,
                        typeMap.SourceType.GetFieldOrProperty(valueResolverConfig.SourceMemberName));

                    var iResolverType =
                        resolverType.GetTypeInfo()
                            .ImplementedInterfaces.First(t => t.ImplementsGenericInterface(typeof(IMemberValueResolver<,,,>)));

                    var sourceResolverParam = iResolverType.GetGenericArguments()[0];
                    var destResolverParam = iResolverType.GetGenericArguments()[1];
                    var sourceMemberResolverParam = iResolverType.GetGenericArguments()[2];
                    var destMemberResolverParam = iResolverType.GetGenericArguments()[3];

                    valueResolverFunc =
                        ExpressionExtensions.ToType(Expression.Call(ExpressionExtensions.ToType(ctor, resolverType), resolverType.GetDeclaredMethod("Resolve"),
                            ExpressionExtensions.ToType(_source, sourceResolverParam),
                            ExpressionExtensions.ToType(_destination, destResolverParam),
                            ExpressionExtensions.ToType(sourceMember, sourceMemberResolverParam),
                            ExpressionExtensions.ToType(destValueExpr, destMemberResolverParam),
                            _context),
                            destinationPropertyType);
                }
                else
                {
                    var iResolverType = resolverType.GetTypeInfo()
                            .ImplementedInterfaces.First(t => t.IsGenericType() && t.GetGenericTypeDefinition() == typeof(IValueResolver<,,>));

                    var sourceResolverParam = iResolverType.GetGenericArguments()[0];
                    var destResolverParam = iResolverType.GetGenericArguments()[1];
                    var destMemberResolverParam = iResolverType.GetGenericArguments()[2];

                    valueResolverFunc =
                        ExpressionExtensions.ToType(Expression.Call(ExpressionExtensions.ToType(ctor, resolverType), iResolverType.GetDeclaredMethod("Resolve"),
                            ExpressionExtensions.ToType(_source, sourceResolverParam),
                            ExpressionExtensions.ToType(_destination, destResolverParam),
                            ExpressionExtensions.ToType(destValueExpr, destMemberResolverParam),
                            _context),
                            destinationPropertyType);
                }

            }
            else if(propertyMap.CustomResolver != null)
            {
                valueResolverFunc = propertyMap.CustomResolver.ReplaceParameters(_source, _destination, destValueExpr, _context);
            }
            else if(propertyMap.CustomExpression != null)
            {
                var nullCheckedExpression = propertyMap.CustomExpression.ReplaceParameters(_source).IfNotNull(destinationPropertyType);
                var destinationNullable = destinationPropertyType.IsNullableType();
                var returnType = destinationNullable && destinationPropertyType.GetTypeOfNullable() == nullCheckedExpression.Type
                    ? destinationPropertyType
                    : nullCheckedExpression.Type;
                valueResolverFunc = nullCheckedExpression.Type.IsValueType() && !destinationNullable
                    ? nullCheckedExpression
                    : Expression.TryCatch(ExpressionExtensions.ToType(nullCheckedExpression, returnType),
                        Expression.Catch(typeof(NullReferenceException), Expression.Default(returnType)),
                        Expression.Catch(typeof(ArgumentNullException), Expression.Default(returnType))
                        );
            }
            else if(propertyMap.SourceMembers.Any()
                     && propertyMap.SrcType != null
                )
            {
                var last = propertyMap.SourceMembers.Last();
                var pi = last as PropertyInfo;
                if(pi != null && pi.GetGetMethod(true) == null)
                {
                    valueResolverFunc = Expression.Default(last.GetMemberType());
                }
                else
                {
                    valueResolverFunc = propertyMap.SourceMembers.Aggregate(
                        (Expression)_source,
                        (inner, getter) => getter is MethodInfo
                            ? getter.IsStatic()
                                ? Expression.Call(null, (MethodInfo)getter, inner)
                                : (Expression)Expression.Call(inner, (MethodInfo)getter)
                            : Expression.MakeMemberAccess(getter.IsStatic() ? null : inner, getter)
                        );
                    if(destinationPropertyType == valueResolverFunc.Type || _configurationProvider.ResolveTypeMap(valueResolverFunc.Type, destinationPropertyType) == null)
                    {
                        valueResolverFunc = valueResolverFunc.IfNotNull(destinationPropertyType);
                    }
                }
            }
            else if(propertyMap.SrcMember != null)
            {
                valueResolverFunc = Expression.MakeMemberAccess(_source, propertyMap.SrcMember);
            }
            else
            {
                valueResolverFunc = Expression.Throw(Expression.Constant(new Exception("I done blowed up")));
            }

            if(propertyMap.NullSubstitute != null)
            {
                var nullSubstitute = Expression.Constant(propertyMap.NullSubstitute);
                valueResolverFunc = Expression.Coalesce(valueResolverFunc, ExpressionExtensions.ToType(nullSubstitute, valueResolverFunc.Type));
            }
            else if(!typeMap.Profile.AllowNullDestinationValues)
            {
                var toCreate = propertyMap.SrcType ?? destinationPropertyType;
                if(!toCreate.IsAbstract() && toCreate.IsClass())
                {
                    valueResolverFunc = Expression.Coalesce(
                        valueResolverFunc,
                        ExpressionExtensions.ToType(Expression.Call(
                            typeof(ObjectCreator).GetDeclaredMethod("CreateNonNullValue"),
                            Expression.Constant(toCreate)
                            ), propertyMap.SrcType));
                }
            }

            return valueResolverFunc;
        }

        public Expression MapExpression(TypePair typePair, Expression sourceParameter, PropertyMap propertyMap = null, Expression destinationParameter = null)
        {
            return MapExpression(_typeMapRegistry, _configurationProvider, typePair, sourceParameter, _context, propertyMap, destinationParameter);
        }

        public static Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            TypePair typePair, Expression sourceParameter, Expression contextParameter, PropertyMap propertyMap = null, Expression destinationParameter = null)
        {
            if(destinationParameter == null)
            {
                destinationParameter = Expression.Default(typePair.DestinationType);
            }
            var typeMap = configurationProvider.ResolveTypeMap(typePair);
            if(typeMap != null)
            {
                if(!typeMap.HasDerivedTypesToInclude())
                {
                    typeMap.Seal(typeMapRegistry, configurationProvider);
                    if(typeMap.MapExpression != null)
                    {
                        return typeMap.MapExpression.ConvertReplaceParameters(sourceParameter, destinationParameter, contextParameter);
                    }
                    else
                    {
                        return ContextMap(typePair, sourceParameter, contextParameter, destinationParameter);
                    }
                }
                else
                {
                    return ContextMap(typePair, sourceParameter, contextParameter, destinationParameter);
                }
            }
            var match = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
            if(match != null)
            {
                var mapperExpression = match.MapExpression(typeMapRegistry, configurationProvider, propertyMap, sourceParameter, destinationParameter, contextParameter);
                return ExpressionExtensions.ToType(mapperExpression, typePair.DestinationType);
            }
            return ContextMap(typePair, sourceParameter, contextParameter, destinationParameter);
        }

        private static Expression ContextMap(TypePair typePair, Expression sourceParameter, Expression contextParameter, Expression destinationParameter)
        {
            var mapMethod = typeof(ResolutionContext).GetDeclaredMethods().First(m => m.Name == "Map").MakeGenericMethod(typePair.SourceType, typePair.DestinationType);
            return Expression.Call(contextParameter, mapMethod, sourceParameter, destinationParameter);
        }
    }
}
