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
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TTarget MapTo<TSource, TTarget>(this TSource source) where TSource : class where TTarget : class
        {
            return TinyMapper.Map<TSource, TTarget>(source);
        }

        public static TTarget MapTo<TSource, TTarget>(this TSource source, TTarget destination) where TSource : class where TTarget : class
        {
            return TinyMapper.Map<TSource, TTarget>(source, destination);
        }
        /// <summary>
        /// 改用TinyMapper
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<TTarget> MapTo<TSource, TTarget>(this IEnumerable<TSource> source) where TSource : class where TTarget : class
        {
            return TinyMapper.Map<TSource, TTarget>(source);
        }

        #region DTO表达式转换成Domain表达式

        public static Expression<Func<TTarget, bool>> Transfer<TSource, TTarget>(this Expression<Func<TSource, bool>> exp)
        {
            var p = Expression.Parameter(typeof(TTarget), "t");
            return exp.RepalceParameter<TSource, TTarget, Func<TTarget, bool>>(p);
        }
        public static Expression<Func<TTarget, TProperty>> Transfer<TSource, TTarget, TProperty>(this Expression<Func<TSource, TProperty>> exp)
        {
            var p = Expression.Parameter(typeof(TTarget), "t");
            return exp.RepalceParameter<TSource, TTarget, Func<TTarget, TProperty>>(p);
        }
        public static Expression<Predicate<TTarget>> Transfer<TSource, TTarget>(this Expression<Predicate<TSource>> exp)
        {
            var p = Expression.Parameter(typeof(TTarget), "t");
            return exp.RepalceParameter<TSource, TTarget, Predicate<TTarget>>(p);
        }

        private static List<MappingMember> GetMaps<TSource, TTarget>()
        {
            var maps = TinyMapper.GetMemberBinding<TSource, TTarget>()?.Where(x => x.IsMemberMapping).ToList();
            if (maps == null)
            {
                return new List<MappingMember>();
                //throw new Exception("没找到类型 " + typeof(DTO).FullName + " 到类型 " + typeof(T).FullName + " 的映射关系。");
            }
            return maps;
        }

        private static Expression<TDelegate> RepalceParameter<TSource, TTarget, TDelegate>(this LambdaExpression exp, ParameterExpression parameter)
        {
            if (exp == null)
            {
                return null;
            }

            var maps = GetMaps<TSource, TTarget>();

            var body = MapperTransfer<TSource, TTarget>.RepalceParameter(exp.Body, parameter, maps);
            return Expression.Lambda<TDelegate>(body, parameter);
        }

        #endregion
    }
}
