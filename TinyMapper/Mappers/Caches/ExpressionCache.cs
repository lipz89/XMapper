using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Nelibur.ObjectMapper.Mappers.Caches
{
    internal static class ExpressionCache
    {
        internal static object GetValue<TSource, TTarget>(string name, TSource source)
        {
            return ExpressionCache<TSource, TTarget>.GetValue(name, source);
        }
    }
    internal static class ExpressionCache<TSource, TTarget>
    {
        public static Dictionary<string, Expression<Func<TSource, object>>> cache = new Dictionary<string, Expression<Func<TSource, object>>>();

        public static void Add(string targetName, Expression<Func<TSource, object>> expression)
        {
            cache.Add(targetName, expression);
        }

        public static object GetValue(string name, TSource source)
        {
            Expression<Func<TSource, object>> exp;
            if (source != null && cache.TryGetValue(name, out exp))
            {
                return exp.Compile().Invoke(source);
            }
            else
            {
                return null;
            }
        }
    }
}