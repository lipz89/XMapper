using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using XMapper.Builder;
using XMapper.Common;
using XMapper.Config;

namespace XMapper.Core
{
    internal static class MapperRoute
    {
        private static Assembly assembly;

        private static readonly Type baseDictionaryMapper = typeof(DictionaryMapper<,,,,,>);
        private static readonly Type baseCollectionMapper = typeof(CollectionMapper<,>);
        private static readonly Type baseCollectionOfMapper = typeof(CollectionMapper<,,>);
        private static readonly Type baseCollectionOfOfMapper = typeof(CollectionMapper<,,,>);
        internal static readonly MethodInfo MapperMethod = typeof(MapperRoute).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).FirstOrDefault(x => x.Name == "Map" && x.GetParameters().Length == 2);

        internal static void Init(MapperConfig config)
        {
            foreach (var pair in config.List)
            {
                pair.InitMap();
            }
            var codeBuilder = new CodeBuilder(config);
            var code = codeBuilder.CreateCode();
            assembly = Compiler.Compile(code, codeBuilder.GetReferencedAssemblies());
            //assembly = Assembly.GetEntryAssembly();
        }

        internal static void RegisterMapper<TSource, TTarget>(BaseMapper<TSource, TTarget> mapper)
        {
            MapperInstances<TSource, TTarget>.Instance = mapper;
        }

        internal static TTarget Map<TTarget>(object source)
        {
            var typeResult = typeof(TTarget);
            if (source == null)
            {
                if (typeResult.IsClass || typeResult.IsNullable())
                    return default(TTarget);
                throw new InvalidCastException("�ն�����ת��Ϊֵ���͡�");
            }
            var typeSource = source.GetType();
            var mi = MapperRoute.MapperMethod.MakeGenericMethod(typeSource, typeResult);
            var target = mi.Invoke(null, new object[] { source, null });
            return (TTarget)target;
        }

        internal static TTarget Map<TSource, TTarget>(TSource source, TTarget result = default(TTarget))
        {
            var typeResult = typeof(TTarget);
            var typeSource = typeof(TSource);
            if (source == null)
            {
                if (typeResult.IsClass || typeResult.IsNullable())
                    return default(TTarget);
                throw new InvalidCastException("�ն�����ת��Ϊֵ���͡�");
            }
            if (source is TTarget || typeResult.IsAssignableFrom(typeSource))
            {
                return (TTarget)(object)source;
            }

            var key = typeSource.GetHashCode() ^ source.GetHashCode();

            var instance = MapperInstances<TSource, TTarget>.Instance;
            if (instance.NeedSetMaps)
            {
                if (!Cache.TryGet<TTarget>(key, out result))
                {
                    result = instance.Map(source, result);
                    Cache.Set<TTarget>(key, result);
                    instance.SetMaps(source, result);
                }
                return result;
            }
            return instance.Map(source, result);
        }

        private static BaseMapper<TSource, TTarget> GetMapper<TSource, TTarget>()
        {
            var mapper = GetBaseMapper<TSource, TTarget>() ?? GetCollectionMapper<TSource, TTarget>();
            if (mapper != null)
            {
                return mapper;
            }

            if (assembly == null)
            {
                throw new Exception("û���ṩ�κ�ӳ�����");
            }

            var type = typeof(BaseMapper<TSource, TTarget>);
            var mapperType = assembly.GetTypes().FirstOrDefault(x => x.IsSubclassOf(type) && !x.IsAbstract);
            if (mapperType != null)
                return Activator.CreateInstance(mapperType) as BaseMapper<TSource, TTarget>;

            throw new Exception(string.Format("û���ṩӳ����� {0} --> {1}",
                                              typeof(TSource).ObtainOriginalName(),
                                              typeof(TTarget).ObtainOriginalName()));
        }

        private static BaseMapper<TSource, TTarget> GetBaseMapper<TSource, TTarget>()
        {
            var custom = CustomMapper<TSource, TTarget>.Mapper;
            if (custom != null)
            {
                return new CustomMapper<TSource, TTarget>();
            }

            var typeResult = typeof(TTarget);
            var typeSource = typeof(TSource);
            if (typeResult.NonNullableType().IsEnum || typeSource.NonNullableType().IsEnum)
            {
                return new EnumMapper<TSource, TTarget>();
            }

            var mapper = new SupportedMapper<TSource, TTarget>();
            if (mapper.IsCanConvert())
            {
                return mapper;
            }

            if (typeResult.NonNullableType() == typeof(Guid) || typeSource.NonNullableType() == typeof(Guid))
            {
                return new GuidMapper<TSource, TTarget>();
            }

            if (typeSource.IsNullable() || typeResult.IsNullable())
            {
                return new NullableMapper<TSource, TTarget>();
            }
            return null;
        }

        private static BaseMapper<TSource, TTarget> GetCollectionMapper<TSource, TTarget>()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            if (targetType.IsEnumerable() && sourceType.IsEnumerable())
            {
                var genericTypes = new List<Type> { sourceType, targetType };
                var sourceGenericTypes = new List<Type>();
                var targetGenericTypes = new List<Type>();
                if (sourceType.HasInterfaceOf(typeof(IDictionary<,>), sourceGenericTypes)
                    && targetType.HasInterfaceOf(typeof(IDictionary<,>), targetGenericTypes))
                {
                    genericTypes.AddRange(sourceGenericTypes);
                    genericTypes.AddRange(targetGenericTypes);
                    var typeDictionaryMapper = baseDictionaryMapper.MakeGenericType(genericTypes.ToArray());
                    return (BaseMapper<TSource, TTarget>)Activator.CreateInstance(typeDictionaryMapper);
                }
                if (targetType.HasInterfaceOf(typeof(IEnumerable<>), targetGenericTypes))
                {
                    if (sourceType.HasInterfaceOf(typeof(IEnumerable<>), sourceGenericTypes))
                    {
                        genericTypes.AddRange(sourceGenericTypes);
                        genericTypes.AddRange(targetGenericTypes);
                        var typeCollectionOfOfMapper = baseCollectionOfOfMapper.MakeGenericType(genericTypes.ToArray());
                        return (BaseMapper<TSource, TTarget>)Activator.CreateInstance(typeCollectionOfOfMapper);
                    }
                    genericTypes.AddRange(targetGenericTypes);
                    var typeCollectionOfMapper = baseCollectionOfMapper.MakeGenericType(genericTypes.ToArray());
                    return (BaseMapper<TSource, TTarget>)Activator.CreateInstance(typeCollectionOfMapper);
                }
                var typeCollectionMapper = baseCollectionMapper.MakeGenericType(genericTypes.ToArray());
                return (BaseMapper<TSource, TTarget>)Activator.CreateInstance(typeCollectionMapper);
            }
            return null;
        }

        internal static void Clear()
        {
            Cache.Cleaner.Value?.ForEach(x => x());
        }

        private static class MapperInstances<TSource, TTarget>
        {
            private static BaseMapper<TSource, TTarget> instance;

            internal static BaseMapper<TSource, TTarget> Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = GetMapper<TSource, TTarget>();
                        //#if DEBUG
                        //                        Console.WriteLine("{0} --> {1} ʹ�� {2}",
                        //                                          typeof(TSource).ObtainOriginalName(),
                        //                                          typeof(TTarget).ObtainOriginalName(),
                        //                                          instance.GetType().ObtainOriginalName());
                        //#endif
                    }
                    return instance;
                }
                set
                {
                    instance = value;
                    //#if DEBUG
                    //                    Console.WriteLine("{0} --> {1} ע�� {2}",
                    //                                      typeof(TSource).ObtainOriginalName(),
                    //                                      typeof(TTarget).ObtainOriginalName(),
                    //                                      instance.GetType().ObtainOriginalName());
                    //#endif
                }
            }
        }

        private static class Cache
        {
            private class Inner<T>
            {
                static Inner()
                {
                    Dictionary = new Dictionary<int, T>();
                    Cleaner.Value = Cleaner.Value ?? new List<Action>();
                    Cleaner.Value.Add(Dictionary.Clear);
                }
                internal static IDictionary<int, T> Dictionary { get; }
            }

            internal static ThreadLocal<List<Action>> Cleaner { get; } = new ThreadLocal<List<Action>>();


            internal static bool TryGet<T>(int key, out T t)
            {
                return Inner<T>.Dictionary.TryGetValue(key, out t);
            }

            internal static void Set<T>(int key, T t)
            {
                if (!Inner<T>.Dictionary.ContainsKey(key))
                {
                    Inner<T>.Dictionary.Add(key, t);
                }
            }

            internal static T Get<T>(int key, Func<T> creator)
            {
                if (!Inner<T>.Dictionary.ContainsKey(key))
                {
                    var t = creator();
                    Inner<T>.Dictionary.Add(key, t);
                }
                return Inner<T>.Dictionary[key];
            }
        }
    }
}
