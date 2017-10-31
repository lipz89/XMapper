using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Nelibur.ObjectMapper.Mappers.Classes.Members;

namespace Nelibur.ObjectMapper.Extensions
{
    public static class MapperExtensions
    {
        /// <summary>
        /// 改用TinyMapper
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDestination MapTo<TSource, TDestination>(this TSource source) where TSource : class where TDestination : class
        {
            return TinyMapper.Map<TSource, TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination) where TSource : class where TDestination : class
        {
            return TinyMapper.Map<TSource, TDestination>(source, destination);
        }
        /// <summary>
        /// 改用TinyMapper
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<TDestination> MapTo<TSource, TDestination>(this IEnumerable<TSource> source) where TSource : class where TDestination : class
        {
            return TinyMapper.Map<TSource, TDestination>(source);
        }

        #region DTO表达式转换成Domain表达式

        public static Expression<Func<T, bool>> Transfer<DTO, T>(this Expression<Func<DTO, bool>> exp)
        {
            var p = Expression.Parameter(typeof(T), "t");
            return exp.RepalceParameter<DTO, T, Func<T, bool>>(p);
        }
        public static Expression<Func<T, TR>> Transfer<DTO, T, TR>(this Expression<Func<DTO, TR>> exp)
        {
            var p = Expression.Parameter(typeof(T), "t");
            return exp.RepalceParameter<DTO, T, Func<T, TR>>(p);
        }
        public static Expression<Predicate<T>> Transfer<DTO, T>(this Expression<Predicate<DTO>> exp)
        {
            var p = Expression.Parameter(typeof(T), "t");
            return exp.RepalceParameter<DTO, T, Predicate<T>>(p);
        }

        private static List<MappingMember> GetMaps<DTO, T>()
        {
            var maps = TinyMapper.GetMemberBinding<DTO, T>()?.Where(x => x.IsMemberMapping).ToList();
            if (maps == null)
            {
                return new List<MappingMember>();
                //throw new Exception("没找到类型 " + typeof(DTO).FullName + " 到类型 " + typeof(T).FullName + " 的映射关系。");
            }
            return maps;
        }

        private static Expression<TDelegate> RepalceParameter<DTO, T, TDelegate>(this LambdaExpression exp, ParameterExpression parameter)
        {
            if (exp == null)
            {
                return null;
            }

            var maps = GetMaps<DTO, T>();

            var body = MapperTransfer<DTO, T>.RepalceParameter(exp.Body, parameter, maps);
            return Expression.Lambda<TDelegate>(body, parameter);
        }

        #endregion
    }
}
