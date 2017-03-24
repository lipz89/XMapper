using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using XMapper.Core;

namespace XMapper.Config
{
    /// <summary>
    /// 映射配置项
    /// </summary>
    public class MapperConfig
    {
        internal MapperConfig() { }

        private List<TypePair> _list = new List<TypePair>();
        /// <summary>
        /// 配置的类型对集合
        /// </summary>
        public IReadOnlyList<TypePair> List => _list.AsReadOnly();
        /// <summary>
        /// 配置一个类型对映射转换
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <returns>返回类型对</returns>
        public TypePair<TSource, TTarget> Map<TSource, TTarget>()
            where TTarget : new()
        {
            var pair = new TypePair<TSource, TTarget>();

            var exists = this._list.FirstOrDefault(x => x.SourceType == pair.SourceType && x.ResultType == pair.ResultType);
            if (exists != null)
            {
                this._list.Remove(exists);
            }

            this._list.Add(pair);

            return pair;
        }
        /// <summary>
        /// 使用一个转换方法配置一个映射转换
        /// 该转换的优先级高于普通类型对
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="mapper">转换方法</param>
        public void Map<TSource, TTarget>(Func<TSource, TTarget> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            var exists = this._list.FirstOrDefault(x => x.SourceType == typeof(TSource) && x.ResultType == typeof(TTarget));
            if (exists != null)
            {
                this._list.Remove(exists);
            }

            CustomMapper<TSource, TTarget>.Mapper = mapper;
        }
        /// <summary>
        /// 使用一个转换类对象配置一个映射转换
        /// 该转换的优先级高于普通类型对和转换方法的配置
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="mapper">转换对象</param>
        public void Map<TSource, TTarget>(BaseMapper<TSource, TTarget> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            var exists = this._list.FirstOrDefault(x => x.SourceType == typeof(TSource) && x.ResultType == typeof(TTarget));
            if (exists != null)
            {
                this._list.Remove(exists);
            }

            MapperRoute.RegisterMapper(mapper);
        }

        public void Map<TItem, TTarget>(Func<List<TItem>, TTarget> mapper) where TTarget : IEnumerable<TItem>
        {
            InnerMapper<TItem, TTarget>.Mapper = mapper;
        }
        public void Map<TTarget>(Func<ArrayList, TTarget> mapper) where TTarget : IEnumerable
        {
            InnerMapper<TTarget>.Mapper = mapper;
        }

        /// <summary>
        /// 获取指定类型的转换配置
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="targetType">目标类型</param>
        /// <returns></returns>
        public TypePair GetMap(Type sourceType, Type targetType)
        {
            return _list.FirstOrDefault(x => x.SourceType == sourceType && x.ResultType == targetType);
        }
        /// <summary>
        /// 获取指定类型的转换配置
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <returns></returns>
        public TypePair GetMap<TSource, TTarget>()
        {
            return GetMap(typeof(TSource), typeof(TTarget));
        }
    }
}
