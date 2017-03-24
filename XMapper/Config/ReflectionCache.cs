using System;
using System.Collections.Generic;
using System.Reflection;

namespace XMapper.Config
{
    internal static class ReflectionCache
    {
        public static Dictionary<Type, List<MemberInfo>> TypeDictionary = new Dictionary<Type, List<MemberInfo>>();

        public static void SetType(Type type)
        {
            if (!TypeDictionary.ContainsKey(type))
            {
                var listProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                var listFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                var memberList = new List<MemberInfo>();
                memberList.AddRange(listProperties);
                memberList.AddRange(listFields);
                TypeDictionary.Add(type, memberList);
            }
        }
        public static List<MemberInfo> GetMembers<T>()
        {
            return GetMembers(typeof(T));
        }

        public static List<MemberInfo> GetMembers(Type type)
        {
            if (!TypeDictionary.ContainsKey(type))
            {
                SetType(type);
            }
            return TypeDictionary[type];
        }
    }
}
