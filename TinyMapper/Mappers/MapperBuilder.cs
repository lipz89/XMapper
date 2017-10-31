using System;
using System.Reflection;
using Nelibur.ObjectMapper.Core.DataStructures;
using Nelibur.ObjectMapper.Mappers.Classes.Members;
using Nelibur.ObjectMapper.Reflection;

namespace Nelibur.ObjectMapper.Mappers
{
    internal abstract class MapperBuilder
    {
        protected const MethodAttributes OverrideProtected = MethodAttributes.Family | MethodAttributes.Virtual;
        protected readonly IDynamicAssembly _assembly;
        protected readonly IMapperBuilderConfig _config;
        private const string AssemblyName = "DynamicTinyMapper";

        protected MapperBuilder(IMapperBuilderConfig config)
        {
            _config = config;
            _assembly = config.Assembly;
        }

        protected abstract string ScopeName { get; }

        public Mapper Build(TypePair typePair)
        {
            var mapper = BuildCore(typePair);
            TinyMapper.Mappers.TryAdd(typePair, mapper);
            return mapper;
        }

        public Mapper Build(TypePair parentTypePair, MappingMember mappingMember)
        {
            var mapper = BuildCore(parentTypePair, mappingMember);
            TinyMapper.Mappers.TryAdd(mappingMember.TypePair, mapper);
            return mapper;
        }

        public bool IsSupported(TypePair typePair)
        {
            return IsSupportedCore(typePair);
        }

        protected abstract Mapper BuildCore(TypePair typePair);
        protected abstract Mapper BuildCore(TypePair parentTypePair, MappingMember mappingMember);

        protected MapperBuilder GetMapperBuilder(TypePair typePair)
        {
            return _config.GetMapperBuilder(typePair);
        }

        protected string GetMapperFullName(TypePair typePair)
        {
            string random = Guid.NewGuid().ToString("N");
            var name = string.Format("{0}.{1}.Mapper_{2}_{3}", AssemblyName, ScopeName, typePair, random);
            return name;
        }

        protected abstract bool IsSupportedCore(TypePair typePair);
    }
}
