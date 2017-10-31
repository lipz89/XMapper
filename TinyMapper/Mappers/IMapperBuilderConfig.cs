using System;
using System.Linq.Expressions;
using System.Reflection;
using Nelibur.ObjectMapper.Bindings;
using Nelibur.ObjectMapper.Core.DataStructures;
using Nelibur.ObjectMapper.Mappers.Classes.Members;
using Nelibur.ObjectMapper.Reflection;

namespace Nelibur.ObjectMapper.Mappers
{
    internal interface IMapperBuilderConfig
    {
        IDynamicAssembly Assembly { get; }
        Option<BindingConfig> GetBindingConfig(TypePair typePair);
        MapperBuilder GetMapperBuilder(TypePair typePair);
        MapperBuilder GetMapperBuilder(TypePair parentTypePair, MappingMember mappingMember);
        Func<string, string, bool> NameMatching { get; }

        void GlobalIgnore<T>(Expression<Func<T, dynamic>> member);

        bool IsGlobalIgnore(MemberInfo member);
    }
}
