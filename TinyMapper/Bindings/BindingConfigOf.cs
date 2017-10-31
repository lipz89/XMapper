using System;
using System.Linq.Expressions;
using Nelibur.ObjectMapper.Core.Extensions;
using Nelibur.ObjectMapper.Mappers.Caches;

namespace Nelibur.ObjectMapper.Bindings
{
    internal sealed class BindingConfigOf<TSource, TTarget> : BindingConfig, IBindingConfig<TSource, TTarget>
    {
        public void Bind(Expression<Func<TTarget, object>> target, Expression<Func<TSource, object>> source)
        {
            string targetName = GetMemberInfo(target);
            if (targetName == null)
            {
                throw new ArgumentException("Expression is not a MemberExpression", nameof(target));
            }
            string sourceName = GetMemberInfo(source);
            if (sourceName == null && source != null)
            {
                ExpressionCache<TSource, TTarget>.Add(targetName, source);
                BindExpression(targetName, source);
            }
            if (string.Equals(sourceName, targetName, StringComparison.Ordinal))
            {
                return;
            }

            BindFields(targetName, sourceName);
        }

        //public void Bind<TField>(Expression<Func<TTarget, TField>> target, TField value)
        //{
        //    Func<object, object> func = x => value;
        //    BindConverter(GetMemberInfo(target), func);
        //}

        //public void Bind(Expression<Func<TTarget, object>> target, Type targetType)
        //{
        //    string targetName = GetMemberInfo(target);
        //    BindType(targetName, targetType);
        //}

        public void Ignore(Expression<Func<TTarget, object>> expression)
        {
            string memberName = GetMemberInfo(expression);
            IgnoreTargetField(memberName);
        }

        private static string GetMemberInfo<T, TField>(Expression<Func<T, TField>> expression)
        {
            return expression.GetMemberInfo()?.Name;
            //var member = expression.Body as MemberExpression;
            //if (member == null)
            //{
            //    var unaryExpression = expression.Body as UnaryExpression;
            //    if (unaryExpression != null)
            //    {
            //        member = unaryExpression.Operand as MemberExpression;
            //    }

            //    if (member == null)
            //    {
            //        return null;
            //    }
            //}
            //return member.Member.Name;
        }
    }
}
