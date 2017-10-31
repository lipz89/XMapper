using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nelibur.ObjectMapper.Bindings;
using Nelibur.ObjectMapper.Core.DataStructures;
using Nelibur.ObjectMapper.Core.Extensions;

namespace Nelibur.ObjectMapper.Mappers.Classes.Members
{
    internal sealed class MappingMemberBuilder
    {
        private readonly IMapperBuilderConfig _config;

        public MappingMemberBuilder(IMapperBuilderConfig config)
        {
            _config = config;
        }

        public List<MappingMember> Build(TypePair typePair)
        {
            return ParseMappingTypes(typePair);
        }

        private static List<MemberInfo> GetPublicMembers(Type type)
        {
            return type.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                       .Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field)
                       .ToList();
        }

        private static List<MemberInfo> GetSourceMembers(Type sourceType)
        {
            var result = new List<MemberInfo>();

            List<MemberInfo> members = GetPublicMembers(sourceType);
            foreach (MemberInfo member in members)
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    MethodInfo method = ((PropertyInfo)member).GetGetMethod();
                    if (method.IsNull())
                    {
                        continue;
                    }
                }
                result.Add(member);
            }
            return result;
        }

        private static List<MemberInfo> GetTargetMembers(Type targetType)
        {
            var result = new List<MemberInfo>();

            List<MemberInfo> members = GetPublicMembers(targetType);
            foreach (MemberInfo member in members)
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    MethodInfo method = ((PropertyInfo)member).GetSetMethod();
                    if ((method.IsNull() || method.GetParameters().Length != 1) && ((PropertyInfo)member).PropertyType.IsValueType)
                    {
                        continue;
                    }
                }
                result.Add(member);
            }
            return result;
        }
        private Expression GetSourceExpression(
            Option<BindingConfig> bindingConfig,
            TypePair typePair,
            MemberInfo targetMember)
        {
            Option<Expression> expression = bindingConfig.Map(x => x.GetBindExpression(targetMember.Name));
            if (expression.HasNoValue)
            {
                return null;
            }
            return expression.Value;
        }
        private string GetSourceName(
            Option<BindingConfig> bindingConfig,
            TypePair typePair,
            MemberInfo targetMember)
        {
            Option<string> sourceName = bindingConfig.Map(x => x.GetBindField(targetMember.Name));
            if (sourceName.HasNoValue)
            {
                sourceName = new Option<string>(targetMember.Name);
            }
            return sourceName.Value;
        }

        private bool IsIgnore(Option<BindingConfig> bindingConfig, TypePair typePair, MemberInfo targetMember)
        {
            return _config.IsGlobalIgnore(targetMember)
                || bindingConfig.Map(x => x.IsIgnoreTargetField(targetMember.Name)).Value;
        }

        private List<MappingMember> ParseMappingTypes(TypePair typePair)
        {
            var result = new List<MappingMember>();

            List<MemberInfo> sourceMembers = GetSourceMembers(typePair.Source);
            List<MemberInfo> targetMembers = GetTargetMembers(typePair.Target);

            Option<BindingConfig> bindingConfig = _config.GetBindingConfig(typePair);

            foreach (var member in targetMembers)
            {
                if (IsIgnore(bindingConfig, typePair, member))
                {
                    result.Add(new MappingMember(member));
                    continue;
                }
                var exp = GetSourceExpression(bindingConfig, typePair, member);
                if (exp.IsNotNull())
                {
                    result.Add(new MappingMember(member, exp));
                    continue;
                }

                string sourceName = GetSourceName(bindingConfig, typePair, member);
                MemberInfo sourceMember = sourceMembers.FirstOrDefault(x => _config.NameMatching(sourceName, x.Name));

                if (sourceMember.IsNull())
                {
                    result.Add(new MappingMember(member));
                    continue;
                }
                result.Add(new MappingMember(member, sourceMember));
            }
            return result;
        }
    }
}
