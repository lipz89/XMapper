using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nelibur.ObjectMapper.Bindings;
using Nelibur.ObjectMapper.Core.DataStructures;
using Nelibur.ObjectMapper.Core.Extensions;
using Nelibur.ObjectMapper.Mappers.Classes;
using Nelibur.ObjectMapper.Mappers.Classes.Members;
using Nelibur.ObjectMapper.Mappers.Collections;
using Nelibur.ObjectMapper.Mappers.Types.Convertible;
using Nelibur.ObjectMapper.Mappers.Types.Custom;
using Nelibur.ObjectMapper.Reflection;

namespace Nelibur.ObjectMapper.Mappers
{
    internal sealed class TargetMapperBuilder : IMapperBuilderConfig
    {
        public static readonly Func<string, string, bool> DefaultNameMatching = (source, target) => string.Equals(source, target, StringComparison.Ordinal);

        private readonly Dictionary<TypePair, BindingConfig> _bindingConfigs = new Dictionary<TypePair, BindingConfig>();
        private readonly ClassMapperBuilder _classMapperBuilder;
        private readonly CollectionMapperBuilder _collectionMapperBuilder;
        private readonly ConvertibleTypeMapperBuilder _convertibleTypeMapperBuilder;
        private readonly CustomTypeMapperBuilder _customTypeMapperBuilder;
        private readonly List<MemberInfo> _ignoreMembers = new List<MemberInfo>();

        public Func<string, string, bool> NameMatching { get; private set; }

        public void GlobalIgnore<T>(Expression<Func<T, dynamic>> member)
        {
            var memberInfo = member.GetMemberInfo();
            if (memberInfo != null)
            {
                _ignoreMembers.Add(memberInfo);
            }
        }

        public bool IsGlobalIgnore(MemberInfo member)
        {
            if (member == null)
            {
                return true;
            }
            return _ignoreMembers.Any(x => x.Name == member.Name && x.GetMemberType() == member.GetMemberType());
        }

        public TargetMapperBuilder(IDynamicAssembly assembly)
        {
            Assembly = assembly;

            _classMapperBuilder = new ClassMapperBuilder(this);
            _collectionMapperBuilder = new CollectionMapperBuilder(this);
            _convertibleTypeMapperBuilder = new ConvertibleTypeMapperBuilder(this);
            _customTypeMapperBuilder = new CustomTypeMapperBuilder(this);

            NameMatching = DefaultNameMatching;
        }

        public void SetNameMatching(Func<string, string, bool> nameMatching)
        {
            NameMatching = nameMatching;
        }

        public IDynamicAssembly Assembly { get; private set; }

        public Mapper Build(TypePair typePair, BindingConfig bindingConfig)
        {
            _bindingConfigs[typePair] = bindingConfig;
            return Build(typePair);
        }

        public Mapper Build(TypePair typePair)
        {
            MapperBuilder mapperBuilder = GetMapperBuilder(typePair);
            Mapper mapper = mapperBuilder.Build(typePair);
            return mapper;
        }

        public Option<BindingConfig> GetBindingConfig(TypePair typePair)
        {
            Option<BindingConfig> result = _bindingConfigs.GetValue(typePair);
            return result;
        }

        public MapperBuilder GetMapperBuilder(TypePair parentTypePair, MappingMember mappingMember)
        {
            if (_customTypeMapperBuilder.IsSupported(parentTypePair, mappingMember))
            {
                return _customTypeMapperBuilder;
            }
            return GetTypeMapperBuilder(mappingMember.TypePair);
        }

        public MapperBuilder GetMapperBuilder(TypePair typePair)
        {
            return GetTypeMapperBuilder(typePair);
        }

        private MapperBuilder GetTypeMapperBuilder(TypePair typePair)
        {
            if (_convertibleTypeMapperBuilder.IsSupported(typePair))
            {
                return _convertibleTypeMapperBuilder;
            }
            else if (_collectionMapperBuilder.IsSupported(typePair))
            {
                return _collectionMapperBuilder;
            }
            return _classMapperBuilder;
        }
    }
}
