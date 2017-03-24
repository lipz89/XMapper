using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace XMapper.Common
{
    internal static class Extensions
    {
        public static bool IsNotNull(this object obj)
        {
            return obj != null;
        }

        public static bool IsNull(this object obj)
        {
            return obj == null;
        }
        public static bool IsNullOrEmpty(this IEnumerable source)
        {
            if (source == null)
                return true;
            return !source.Cast<object>().Any();
        }
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return true;
            return !source.Any();
        }

        public static string GetPropertyName<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            if (expression == null) throw Error.ArgumentNullException(nameof(expression));

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
                    throw Error.ArgumentException("指定表达式不是成员访问类型。", nameof(expression));
                }
            }
            return member.Member.Name;
        }
        // Methods
        private static string ExtractGenericArguments(this IEnumerable<Type> names)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Type type in names)
            {
                if (builder.Length > 1)
                {
                    builder.Append(", ");
                }
                builder.Append(type.ObtainOriginalName());
            }
            return builder.ToString();
        }

        private static string ExtractName(string name)
        {
            int length = name.IndexOf("`", StringComparison.Ordinal);
            if (length > 0)
            {
                name = name.Substring(0, length);
            }
            return name;
        }

        public static string ObtainOriginalMethodName(this MethodInfo method)
        {
            if (method == null) throw Error.ArgumentNullException(nameof(method));

            if (!method.IsGenericMethod)
            {
                return method.Name;
            }
            return ExtractName(method.Name) + "<" + ExtractGenericArguments(method.GetGenericArguments()) + ">";
        }
        private static string ObtainOriginalNameCore(this Type type)
        {
            if (type == null) throw Error.ArgumentNullException(nameof(type));

            if (type.IsArray)
            {
                var n = type.Name;
                var etype = type.GetElementType();
                return n.Replace(etype.Name, etype.ObtainOriginalName());
            }
            if (type.IsGenericType)
            {
                var gt = ExtractName(type.FullName);
                var gtp = ExtractGenericArguments(type.GetGenericArguments());
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return gtp + "?";
                }
                return gt + "<" + gtp + ">";
            }
            return type.FullName;
        }

        public static string GetGenericName(this Type type)
        {
            if (type == null) throw Error.ArgumentNullException(nameof(type));

            if (type.IsGenericType)
            {
                return ExtractName(type.Name);
            }
            return string.Empty;
        }

        private static Dictionary<Type, string> simpleName = new Dictionary<Type, string>
        {
            { typeof(object), "object"},
            { typeof(string), "string"},
            { typeof(bool), "bool"},
            { typeof(char) ,"char"},
            { typeof(int), "int"},
            { typeof(uint), "uint"},
            { typeof(byte), "byte"},
            { typeof(sbyte), "sbyte"},
            { typeof(short), "short"},
            { typeof(ushort), "ushort"},
            { typeof(long), "long"},
            { typeof(ulong), "ulong"},
            { typeof(float), "float"},
            { typeof(double), "double"},
            { typeof(decimal), "decimal"}
        };

        public static string ObtainOriginalName(this Type type)
        {
            if (type == null) throw Error.ArgumentNullException(nameof(type));

            if (simpleName.ContainsKey(type))
            {
                return simpleName[type];
            }
            return type.ObtainOriginalNameCore();
        }

        public static bool IsNullable(this Type type)
        {
            if (type == null) throw Error.ArgumentNullException(nameof(type));

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type NonNullableType(this Type type)
        {
            if (type == null) throw Error.ArgumentNullException(nameof(type));

            return type.IsNullable() ? Nullable.GetUnderlyingType(type) : type;
        }
        public static bool IsSupportedType(this Type type)
        {
            if (type == null) throw Error.ArgumentNullException(nameof(type));

            return type.IsPrimitive
                   || type == typeof(string)
                   || type == typeof(Guid)
                   || type.IsEnum
                   || type == typeof(decimal)
                   || type.IsNullable() && IsSupportedType(Nullable.GetUnderlyingType(type));
        }

        public static Type ThisType(this MemberInfo memberInfo)
        {
            if (memberInfo == null) throw Error.ArgumentNullException(nameof(memberInfo));

            if (memberInfo is PropertyInfo)
            {
                return (memberInfo as PropertyInfo).PropertyType;
            }
            if (memberInfo is FieldInfo)
            {
                return (memberInfo as FieldInfo).FieldType;
            }
            return null;
        }
        public static bool CanRead(this MemberInfo memberInfo)
        {
            if (memberInfo == null) throw Error.ArgumentNullException(nameof(memberInfo));

            if (memberInfo is PropertyInfo)
            {
                return (memberInfo as PropertyInfo).CanRead;
            }
            return memberInfo is FieldInfo;
        }
        public static bool CanWrite(this MemberInfo memberInfo)
        {
            if (memberInfo == null) throw Error.ArgumentNullException(nameof(memberInfo));

            if (memberInfo is PropertyInfo)
            {
                return (memberInfo as PropertyInfo).CanWrite;
            }
            return memberInfo is FieldInfo;
        }

        public static Type GetItemType(this Type type)
        {
            if (type == null) throw Error.ArgumentNullException(nameof(type));

            if (type.IsArray)
            {
                return type.GetElementType();
            }
            if (type.IsEnumerableOf())
            {
                var itf = type.GetInterfaces().FirstOrDefault(x => x.Name == "IEnumerable`1");
                var t = itf?.GetGenericArguments().First();
                if (!t?.ContainsGenericParameters == true)
                    return t;
                return null;
            }
            if (type.IsEnumerable())
            {
                return typeof(object);
            }
            throw new NotSupportedException();
        }

        public static bool IsEnumerable(this Type type)
        {
            if (type == null) throw Error.ArgumentNullException(nameof(type));

            return type.HasInterface<IEnumerable>();
        }

        public static bool IsEnumerableOf(this Type type)
        {
            if (type == null) throw Error.ArgumentNullException(nameof(type));

            return type.HasInterfaceOf(typeof(IEnumerable<>));
        }
        public static bool HasInterface(this Type type, Type interfaceType)
        {
            if (type == null) throw Error.ArgumentNullException(nameof(type));
            if (interfaceType == null) throw Error.ArgumentNullException(nameof(interfaceType));

            if (!interfaceType.IsInterface)
                return false;
            return interfaceType.IsAssignableFrom(type);
        }
        public static bool HasInterface<T>(this Type type)
        {
            if (type == null)
                throw Error.ArgumentNullException(nameof(type));
            return type.HasInterface(typeof(T));
        }
        public static bool HasInterfaceOf(this Type type, Type interfaceType, List<Type> genericTypes = null)
        {
            if (type == null) throw Error.ArgumentNullException(nameof(type));
            if (interfaceType == null) throw Error.ArgumentNullException(nameof(interfaceType));

            if (!interfaceType.IsInterface)
                return false;

            if (type == interfaceType)
                return true;
            var its = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType);

            if (its != null)
            {
                if (genericTypes != null)
                {
                    genericTypes.Clear();
                    genericTypes.AddRange(its.GetGenericArguments());
                }
                return true;
            }
            return false;
        }
    }
}
