using System;
using System.Linq.Expressions;
using System.Reflection;
using Nelibur.ObjectMapper.Bindings;
using Nelibur.ObjectMapper.Core.DataStructures;
using Nelibur.ObjectMapper.Core.Extensions;
using Nelibur.ObjectMapper.Mappers;
using Nelibur.ObjectMapper.Mappers.Classes.Members;
using Nelibur.ObjectMapper.Reflection;

namespace UnitTests
{
    internal class MappingBuilderConfigStub : IMapperBuilderConfig
    {
        private readonly Option<BindingConfig> _bindingConfig = Option<BindingConfig>.Empty;

        public MappingBuilderConfigStub()
        {
        }

        public MappingBuilderConfigStub(BindingConfig bindingConfig)
        {
            _bindingConfig = bindingConfig.ToOption();
        }

        public IDynamicAssembly Assembly
        {
            get { return DynamicAssemblyBuilder.Get(); }
        }

        public Func<string, string, bool> NameMatching
        {
            get
            {
                return TargetMapperBuilder.DefaultNameMatching;
            }
        }

        public void GlobalIgnore<T>(Expression<Func<T, dynamic>> member)
        {
            throw new NotImplementedException();
        }

        public bool IsGlobalIgnore(MemberInfo member)
        {
            throw new NotImplementedException();
        }

        public Option<BindingConfig> GetBindingConfig(TypePair typePair)
        {
            return _bindingConfig;
        }

        public MapperBuilder GetMapperBuilder(TypePair typePair)
        {
            throw new NotImplementedException();
        }

        public MapperBuilder GetMapperBuilder(TypePair parentTypePair, MappingMember mappingMember)
        {
            throw new NotImplementedException();
        }
    }
}
