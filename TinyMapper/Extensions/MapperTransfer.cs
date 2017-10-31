using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Nelibur.ObjectMapper.Core.Extensions;
using Nelibur.ObjectMapper.Mappers.Classes.Members;

namespace Nelibur.ObjectMapper.Extensions
{
    public class MapperTransfer<TSource, TTarget> : ExpressionVisitor
    {
        private readonly ParameterExpression parameter;
        private readonly List<MappingMember> maps;

        public MapperTransfer(ParameterExpression parameter, List<MappingMember> maps)
        {
            this.parameter = parameter;
            this.maps = maps;
        }

        public static Expression RepalceParameter(Expression expression, ParameterExpression parameter, List<MappingMember> maps)
        {
            try
            {
                var transfer = new MapperTransfer<TSource, TTarget>(parameter, maps);
                return transfer.Visit(expression);
            }
            catch (Exception ex)
            {
                throw new Exception("表达式转换失败，更多内容查看内部异常。", ex);
            }
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type == typeof(TSource))
            {
                return parameter;
            }
            return base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var expTarget = base.Visit(node.Expression);

            //递归检查表达式根节点是不是常量，如果是常量，依次取值到当前节点
            if (expTarget.NodeType == ExpressionType.Constant)
            {
                var target = (expTarget as ConstantExpression).Value;
                var parTarget = Expression.Parameter(target.GetType());
                var targetMember = Expression.MakeMemberAccess(parTarget, node.Member);
                var lbdMember = Expression.Lambda(targetMember, parTarget).Compile();
                var memValue = lbdMember.DynamicInvoke(target);
                return Expression.Constant(memValue, targetMember.Type);
            }

            //映射属性访问
            if (node.Member.ReflectedType.IsAssignableFrom(typeof(TSource)))
            {
                if (expTarget.Type == typeof(TTarget))
                {
                    var map = maps.FirstOrDefault(x => x.Source == node.Member || (x.Source.Name == node.Member.Name && x.Source.GetMemberType() == node.Member.GetMemberType()));
                    if (map != null)
                    {
                        if (map.Target != null)
                        {
                            var prop = map.Target;
                            if (prop != null)
                            {
                                return Expression.MakeMemberAccess(expTarget, prop);
                            }
                        }
                        else if (map.Ignored)
                        {
                            throw new Exception("使用的成员" + typeof(TSource).FullName + "." + node.Member.Name + "已被忽略。");
                        }

                        throw new Exception("成员" + typeof(TSource).FullName + "." + node.Member.Name + "的映射关系不可转换为表达式。");
                    }

                    return Expression.MakeMemberAccess(expTarget, node.Member);
                    //throw new Exception("没找到成员" + typeof(DTO).FullName + "." + node.Member.Name + "的映射关系");
                }
            }

            return Expression.MakeMemberAccess(expTarget, node.Member);
            //return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.ReflectedType.IsAssignableFrom(typeof(TSource)))
            {
                var name = node.Method.Name;
                var pi = typeof(TTarget).GetMethod(name);
                var pps = node.Arguments.Select(this.Visit);
                return Expression.Call(parameter, pi, pps);
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Type == typeof(TSource))
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

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type == typeof(TSource) && node.Value == null)
            {
                return Expression.Constant(null, typeof(TTarget));
            }
            return base.VisitConstant(node);
        }
    }
}