using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

using Nelibur.ObjectMapper.Bindings;
using Nelibur.ObjectMapper.Core.DataStructures;
using Nelibur.ObjectMapper.Mappers;
using Nelibur.ObjectMapper.Mappers.Classes.Members;
using Nelibur.ObjectMapper.Reflection;

namespace Nelibur.ObjectMapper
{
    public static class TinyMapper
    {
        internal static readonly ConcurrentDictionary<TypePair, Mapper> Mappers = new ConcurrentDictionary<TypePair, Mapper>();
        private static readonly TargetMapperBuilder _targetMapperBuilder;
        private static readonly TinyMapperConfig _config;
        private static IDynamicAssembly assembly;
        static TinyMapper()
        {
            assembly = DynamicAssemblyBuilder.Get();
            _targetMapperBuilder = new TargetMapperBuilder(assembly);
            _config = new TinyMapperConfig(_targetMapperBuilder);
        }

        [Conditional("XDEBUG")]
        public static void Save()
        {
            assembly.Save();
        }
        #region Bind & Config

        public static void Bind(Type source, Type target)
        {
            if (BindingExists(source, target))
            {
                return;
            }
            TypePair typePair = TypePair.Create(source, target);

            /*_mappers[typePair] =*/
            _targetMapperBuilder.Build(typePair);
        }

        public static void Bind<TSource, TTarget>()
        {
            if (BindingExists<TSource, TTarget>())
            {
                return;
            }
            TypePair typePair = TypePair.Create<TSource, TTarget>();

            /*_mappers[typePair] =*/
            _targetMapperBuilder.Build(typePair);
        }

        public static void Bind<TSource, TTarget>(Action<IBindingConfig<TSource, TTarget>> config)
        {
            if (BindingExists<TSource, TTarget>())
            {
                return;
            }
            TypePair typePair = TypePair.Create<TSource, TTarget>();

            var bindingConfig = new BindingConfigOf<TSource, TTarget>();
            config(bindingConfig);

            /*_mappers[typePair] =*/
            _targetMapperBuilder.Build(typePair, bindingConfig);
        }

        public static void BindPair(Type source, Type target)
        {
            TinyMapper.Bind(source, target);
            TinyMapper.Bind(target, source);
        }

        public static void BindPair<T1, T2>(Action<IBindingConfig<T1, T2>> config1 = null, Action<IBindingConfig<T2, T1>> config2 = null)
        {
            if (config1 != null)
                TinyMapper.Bind<T1, T2>(config1);
            else
                TinyMapper.Bind<T1, T2>();

            if (config2 != null)
                TinyMapper.Bind<T2, T1>(config2);
            else
                TinyMapper.Bind<T2, T1>();
        }

        public static void Config(Action<ITinyMapperConfig> config)
        {
            config(_config);
        }

        /// <summary>
        ///     Find out if a binding exists for Source to Target
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <returns></returns>
        public static List<MappingMember> GetMemberBinding<TSource, TTarget>()
        {
            if (BindingExists<TSource, TTarget>())
            {
                TypePair typePair = TypePair.Create<TSource, TTarget>();
                var memberBuilder = new MappingMemberBuilder(_targetMapperBuilder);
                var result = memberBuilder.Build(typePair);
                return result;
            }

            return null;
        }

        public static bool BindingExists(Type source, Type target)
        {
            TypePair typePair = TypePair.Create(source, target);
            Mapper mapper;

            var result = Mappers.TryGetValue(typePair, out mapper);

            return result;
        }

        /// <summary>
        ///     Find out if a binding exists for Source to Target
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <returns></returns>
        public static bool BindingExists<TSource, TTarget>()
        {
            TypePair typePair = TypePair.Create<TSource, TTarget>();
            Mapper mapper;

            var result = Mappers.TryGetValue(typePair, out mapper);

            return result;
        }

        #endregion

        #region MapCore

        internal static TTarget MapCore<TSource, TTarget>(TSource source, TTarget target = default(TTarget))
        {
            if (source == null)
            {
                return default(TTarget);
            }
            var key = typeof(TSource).GetHashCode() ^ source.GetHashCode();
            TTarget rst;
            var flag = MapperCache.TryGet(key, out rst);
            if (flag)
            {
                return rst;
            }
            TypePair typePair = TypePair.Create<TSource, TTarget>();

            Mapper mapper = GetMapper(typePair);
            var result = (TTarget)mapper.Map(source, target);
            MapperCache.Set(key, result);
            mapper.MapObjects(source, result);
            return result;
        }


        /// <summary>
        ///     Maps the specified source to <see cref="TTarget" /> type.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="source">The source value.</param>
        /// <returns>Value</returns>
        /// <remarks>For mapping nullable type use <see cref="Map{TTarget}" />method.</remarks>
        internal static TTarget MapCore2<TTarget>(object source)
        {
            if (source == null)
            {
                return default(TTarget);
            }

            var sourceType = source.GetType();
            var key = sourceType.GetHashCode() ^ source.GetHashCode();

            TTarget rst;
            var flag = MapperCache.TryGet(key, out rst);
            if (flag)
            {
                return rst;
            }

            TypePair typePair = TypePair.Create(sourceType, typeof(TTarget));

            Mapper mapper = GetMapper(typePair);
            var result = (TTarget)mapper.Map(source);
            MapperCache.Set(key, result);
            mapper.MapObjects(source, result);

            return result;
        }

        internal static IEnumerable<TTarget> MapCores<TSource, TTarget>(IEnumerable<TSource> source)
        {
            if (source == null)
            {
                return null;
            }
            TypePair typePair = TypePair.Create<TSource, TTarget>();
            Mapper mapper = GetMapper(typePair);
            var list = new List<TTarget>();
            foreach (TSource item in source)
            {
                var key = typeof(TSource).GetHashCode() ^ item.GetHashCode();
                TTarget rst;
                var flag = MapperCache.TryGet(key, out rst);
                if (flag)
                {
                    list.Add(rst);
                }
                else
                {
                    var result = (TTarget)mapper.Map(item);
                    MapperCache.Set(key, result);
                    mapper.MapObjects(item, result);
                    list.Add(result);
                }
            }

            return list;
        }

        internal static void MapVoid<TSource, TTarget>(TSource source, TTarget target = default(TTarget))
        {
            MapCore<TSource, TTarget>(source, target);
        }

        #endregion

        #region Map
        public static TTarget Map<TSource, TTarget>(TSource source, TTarget target = default(TTarget))
        {
            var result = MapCore<TSource, TTarget>(source, target);
            MapperCache.Clear();
            return result;
        }

        public static TTarget Map<TTarget>(object source)
        {
            var result = MapCore2<TTarget>(source);
            MapperCache.Clear();
            return result;
        }
        public static IEnumerable<TTarget> Map<TSource, TTarget>(IEnumerable<TSource> source, TTarget target = default(TTarget))
        {
            var result = MapCores<TSource, TTarget>(source);
            MapperCache.Clear();
            return result;
        }
        #endregion

        #region GetMapper

        private static Mapper GetMapper(TypePair typePair)
        {
            Mapper mapper;
            var flag = Mappers.TryGetValue(typePair, out mapper);

            if (!flag)
            {
                if (_config.EnablePolymorphicMapping && (mapper = GetPolymorphicMapping(typePair)) != null)
                {
                    return mapper;
                }
#if XDEBUG
                if (_config.EnableAutoBinding)
                {
                    throw new MappingException($"未定义映射 '{typePair.Source?.Name}' to '{typePair.Target?.Name}'.");
                }
#endif
                if (_config.EnableAutoBinding)
                {
                    mapper = _targetMapperBuilder.Build(typePair);
                }
                else
                {
                    throw new MappingException($"未定义映射 '{typePair.Source?.Name}' to '{typePair.Target?.Name}'.");
                }
            }
            return mapper;
        }

        //Note: Lock should already be acquired for the mapper
        private static Mapper GetPolymorphicMapping(TypePair types)
        {
            // Walk the polymorphic heirarchy until we find a mapping match
            Type source = types.Source;

            do
            {
                Mapper result;
                foreach (var iface in source.GetInterfaces())
                {
                    if (Mappers.TryGetValue(TypePair.Create(iface, types.Target), out result))
                        return result;
                }

                if (Mappers.TryGetValue(TypePair.Create(source, types.Target), out result))
                    return result;
            }
            while ((source = source.BaseType) != null);

            return null;
        }
        #endregion
    }
}
