using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XMapper.Config;

namespace XMapper.Common
{
    internal class MapperTransfer<DTO, T> : ExpressionVisitor
    {
        private readonly ParameterExpression parameter;
        private readonly IEnumerable<PropertyPair> maps;

        public MapperTransfer(ParameterExpression parameter, IEnumerable<PropertyPair> maps)
        {
            this.parameter = parameter;
            this.maps = maps;
        }

        public static Expression RepalceParameter(Expression expression, ParameterExpression parameter, IEnumerable<PropertyPair> maps = null)
        {
            return new MapperTransfer<DTO, T>(parameter, maps).Visit(expression);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type == typeof(DTO))
            {
                return parameter;
            }
            return base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.ReflectedType.IsAssignableFrom(typeof(DTO)))
            {
                if (maps.IsNullOrEmpty())
                {
                    var prop = typeof(T).GetMember(node.Member.Name).FirstOrDefault();
                    if (prop != null)
                    {
                        return Expression.MakeMemberAccess(parameter, prop);
                    }
                    throw new Exception("没找到类型" + typeof(T).FullName + "名称为" + node.Member.Name + "的成员。");
                }
                var map = maps.FirstOrDefault(x => x.ResultPropertyName == node.Member.Name);
                if (map != null)
                {
                    if (map.IsIgnore)
                    {
                        throw new Exception("使用的成员" + typeof(DTO).FullName + "." + node.Member.Name + "已被忽略。");
                    }
                    if (map.IsUseSolver)
                    {
                        var lambda = map.Solver as LambdaExpression;
                        return MapperTransfer<T, T>.RepalceParameter(lambda.Body, parameter);
                    }
                    if (map.IsUseInstance)
                    {
                        return Expression.Constant(map.Instance);
                    }
                    if (map.SourcePropertyName != null)
                    {
                        var prop = typeof(T).GetMember(map.SourcePropertyName).FirstOrDefault();
                        if (prop != null)
                        {
                            return Expression.MakeMemberAccess(parameter, prop);
                        }
                    }
                    throw new Exception("成员" + typeof(DTO).FullName + "." + node.Member.Name + "的映射关系不可转换为表达式。");
                }
                else
                {
                    throw new Exception("没找到成员" + typeof(DTO).FullName + "." + node.Member.Name + "的映射关系。");
                }
            }
            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.ReflectedType.IsAssignableFrom(typeof(DTO)))
            {
                var name = node.Method.Name;
                var pi = typeof(T).GetMethod(name);
                var pps = node.Arguments.Select(this.Visit);
                return Expression.Call(parameter, pi, pps);
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Type == typeof(DTO))
            {
                throw new Exception("请勿在DTO相关表达式中使用DTO类型的构造函数。");
                //var contructor = typeof(T).GetConstructor(Type.EmptyTypes);
                //if (contructor != null)
                //{
                //    return Expression.New(contructor);
                //}
                //throw new Exception("请勿在DTO相关表达式中使用DTO类型有参的构造函数。");
            }
            return base.VisitNew(node);
        }
    }
}