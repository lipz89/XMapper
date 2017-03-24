using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using XMapper.Config;

namespace XMapper.Common
{
    public static class MapperExtensions
    {
        public static Expression<Func<T, bool>> Transfer<DTO, T>(this Expression<Func<DTO, bool>> exp)
        {
            var p = Expression.Parameter(typeof(T), "t");
            return exp.RepalceParameter<DTO, T, Func<T, bool>>(p);

            //return (Expression<Func<T, bool>>)exp.Transfer<DTO, T>(p);
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

        private static Expression<TDelegate> RepalceParameter<DTO, T, TDelegate>(this LambdaExpression exp, ParameterExpression parameter)
        {
            var body = MapperTransfer<DTO, T>.RepalceParameter(exp.Body, parameter, GetMaps<DTO, T>());
            return Expression.Lambda<TDelegate>(body, parameter);
        }

        private static IEnumerable<PropertyPair> GetMaps<DTO, T>()
        {
            return Mapper.Config.GetMap<T, DTO>()?.PropertyPairs;
            //if (maps == null)
            //    throw new Exception("没找到类型 " + typeof(DTO).FullName + " 到类型 " + typeof(T).FullName + " 的映射关系。");
            //return maps;
        }

        private static Expression Transfer<DTO, T>(this Expression exp, ParameterExpression parameter)
        {
            if (exp == null)
                return null;

            if (exp is LambdaExpression)
            {
                var lambda = exp as LambdaExpression;
                var body = lambda.Body.Transfer<DTO, T>(parameter);
                var pps = lambda.Parameters.Select(x => (ParameterExpression)Transfer<DTO, T>(x, parameter));
                return Expression.Lambda(body, pps);
            }
            if (exp is BinaryExpression)
            {
                var binary = exp as BinaryExpression;
                var left = Transfer<DTO, T>(binary.Left, parameter);
                var right = Transfer<DTO, T>(binary.Right, parameter);
                switch (exp.NodeType)
                {
                    case ExpressionType.Add:
                        return Expression.Add(left, right);
                    case ExpressionType.AddChecked:
                        return Expression.AddChecked(left, right);
                    case ExpressionType.And:
                        return Expression.And(left, right);
                    case ExpressionType.AndAlso:
                        return Expression.AndAlso(left, right);
                    case ExpressionType.ArrayIndex:
                        return Expression.ArrayIndex(left, right);
                    case ExpressionType.Coalesce:
                        return Expression.Coalesce(left, right);
                    case ExpressionType.Divide:
                        return Expression.Divide(left, right);
                    case ExpressionType.Equal:
                        return Expression.Equal(left, right);
                    case ExpressionType.ExclusiveOr:
                        return Expression.ExclusiveOr(left, right);
                    case ExpressionType.GreaterThan:
                        return Expression.GreaterThan(left, right);
                    case ExpressionType.GreaterThanOrEqual:
                        return Expression.GreaterThanOrEqual(left, right);
                    case ExpressionType.LeftShift:
                        return Expression.LeftShift(left, right);
                    case ExpressionType.LessThan:
                        return Expression.LessThan(left, right);
                    case ExpressionType.LessThanOrEqual:
                        return Expression.LessThanOrEqual(left, right);
                    case ExpressionType.Modulo:
                        return Expression.Modulo(left, right);
                    case ExpressionType.Multiply:
                        return Expression.Multiply(left, right);
                    case ExpressionType.MultiplyChecked:
                        return Expression.MultiplyChecked(left, right);
                    case ExpressionType.NotEqual:
                        return Expression.NotEqual(left, right);
                    case ExpressionType.Or:
                        return Expression.Or(left, right);
                    case ExpressionType.OrElse:
                        return Expression.OrElse(left, right);
                    case ExpressionType.Power:
                        return Expression.Power(left, right);
                    case ExpressionType.RightShift:
                        return Expression.RightShift(left, right);
                    case ExpressionType.Subtract:
                        return Expression.Subtract(left, right);
                    case ExpressionType.SubtractChecked:
                        return Expression.SubtractChecked(left, right);
                }
            }
            else if (exp is MethodCallExpression)
            {
                var method = exp as MethodCallExpression;
                var pps = method.Arguments.Select(x => x.Transfer<DTO, T>(parameter));
                if (method.Method.ReflectedType == typeof(DTO))
                {
                    var name = method.Method.Name;
                    var pi = typeof(T).GetMethod(name);
                    if (method.Object?.Type == typeof(DTO))
                    {
                        return Expression.Call(parameter, pi, pps);
                    }
                    else
                    {
                        return Expression.Call(method.Object, pi, pps);
                    }
                }
                else
                {
                    var obj = method.Object.Transfer<DTO, T>(parameter);
                    return Expression.Call(obj, method.Method, pps);
                }
            }
            else if (exp is MemberExpression)
            {
                var member = exp as MemberExpression;
                if (member.Member.ReflectedType == typeof(DTO))
                {
                    var mem = typeof(T).GetMember(member.Member.Name).FirstOrDefault();
                    if (mem != null && (mem.MemberType == MemberTypes.Field || mem.MemberType == MemberTypes.Property))
                    {
                        return Expression.MakeMemberAccess(parameter, member.Member);
                    }
                    else
                    {
                        throw new Exception("没找到成员" + typeof(T).FullName + "." + member.Member.Name + "。");
                    }
                }
                else
                {
                    var obj = member.Expression.Transfer<DTO, T>(parameter);
                    return Expression.MakeMemberAccess(obj, member.Member);
                }
            }
            else if (exp is NewExpression)
            {
                var @new = exp as NewExpression;
                if (@new.Type == typeof(DTO))
                {
                    var contructor = typeof(T).GetConstructor(Type.EmptyTypes);
                    if (contructor != null)
                    {
                        return Expression.New(contructor);
                    }
                    else
                    {
                        throw new Exception("太复杂了，不会写。");
                    }
                }
                else
                {
                    return exp;
                }
            }
            else if (exp is ConstantExpression)
            {
                return exp;
            }
            else if (exp is ParameterExpression)
            {
                var param = exp as ParameterExpression;
                if (param.Type == typeof(DTO))
                {
                    return parameter;
                }
                else
                {
                    return exp;
                }
            }
            else
            {
                switch (exp.NodeType)
                {
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.Negate:
                    case ExpressionType.UnaryPlus:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                        return exp;
                }
            }

            throw new Exception("未知");
        }
        //*/
    }
}