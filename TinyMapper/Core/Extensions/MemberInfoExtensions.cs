using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nelibur.ObjectMapper.Core.DataStructures;

namespace Nelibur.ObjectMapper.Core.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static Option<TAttribute> GetAttribute<TAttribute>(this MemberInfo value)
            where TAttribute : Attribute
        {
            return Attribute.GetCustomAttributes(value)
                            .FirstOrDefault(x => x is TAttribute)
                            .ToType<TAttribute>();
        }

        public static List<TAttribute> GetAttributes<TAttribute>(this MemberInfo value)
            where TAttribute : Attribute
        {
            return Attribute.GetCustomAttributes(value).OfType<TAttribute>().ToList();
        }

        public static Type GetMemberType(this MemberInfo value)
        {
            if (value.IsField())
            {
                return ((FieldInfo)value).FieldType;
            }
            else if (value.IsProperty())
            {
                return ((PropertyInfo)value).PropertyType;
            }
            else if (value.IsMethod())
            {
                return ((MethodInfo)value).ReturnType;
            }
            throw new NotSupportedException();
        }

        public static bool IsField(this MemberInfo value)
        {
            return value.MemberType == MemberTypes.Field;
        }

        public static bool IsProperty(this MemberInfo value)
        {
            return value.MemberType == MemberTypes.Property;
        }

        private static bool IsMethod(this MemberInfo value)
        {
            return value.MemberType == MemberTypes.Method;
        }

        public static bool IsWritable(this MemberInfo value)
        {
            return value.IsField() || (((PropertyInfo)value).GetSetMethod() != null);
        }


        public static MemberInfo GetMemberInfo<T, TField>(this Expression<Func<T, TField>> expression)
        {
            var member = expression.Body as MemberExpression;
            if (member == null)
            {
                var unaryExpression = expression.Body as UnaryExpression;
                if (unaryExpression != null)
                {
                    member = unaryExpression.Operand as MemberExpression;
                }

                if (member == null)
                {
                    return null;
                }
            }
            return member.Member;
        }
    }
}
