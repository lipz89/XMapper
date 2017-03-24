using System;

using XMapper.Common;
using XMapper.Config;
using XMapper.Core;

namespace XMapper
{
    /// <summary>
    /// XMapper主入口类
    /// </summary>
    public static class Mapper
    {
        static Mapper()
        {
            Config = new MapperConfig();
        }

        /// <summary>
        /// 配置
        /// </summary>
        public static MapperConfig Config { get; private set; }
        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="action">配置方法</param>
        public static void Init(Action<MapperConfig> action)
        {
            action(Config);

            MapperRoute.Init(Config);
        }
        /// <summary>
        /// 源对象类型未知时的转换方法
        /// </summary>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="source">源对象</param>
        /// <returns>返回转换后的目标对象</returns>
        public static TTarget Map<TTarget>(object source)
        {
            try
            {
                if (source.IsNull())
                    return default(TTarget);

                return MapperRoute.Map<TTarget>(source);
            }
            finally
            {
                MapperRoute.Clear();
            }
        }
        /// <summary>
        /// 转换方法
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="result">目标对象</param>
        /// <returns>转换后的目标对象</returns>
        public static TTarget Map<TSource, TTarget>(TSource source, TTarget result = default(TTarget))
        {
            try
            {
                if (source.IsNull())
                    return default(TTarget);

                return MapperRoute.Map<TSource, TTarget>(source, result);
            }
            finally
            {
                MapperRoute.Clear();
            }
        }
    }
}