using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Nelibur.ObjectMapper.CodeGenerators.Emitters;
using Nelibur.ObjectMapper.Core.DataStructures;
using Nelibur.ObjectMapper.Core.Extensions;
using Nelibur.ObjectMapper.Mappers.Caches;

namespace Nelibur.ObjectMapper.Mappers.Classes.Members
{
    internal sealed class MemberMapper
    {
        private readonly IMapperBuilderConfig _config;

        public MemberMapper(IMapperBuilderConfig config)
        {
            _config = config;
        }

        public IEmitter Build(TypePair parentTypePair, List<MappingMember> members)
        {
            var emitter = new EmitComposite();
            members.ForEach(x =>
                            {
                                if (!x.Ignored)
                                    emitter.Add(Build(parentTypePair, x));
                            });
            return emitter;
        }

        private static IEmitterType StoreFiled(FieldInfo field, IEmitterType targetObject, IEmitterType value)
        {
            return EmitField.Store(field, targetObject, value);
        }

        private static IEmitterType StoreProperty(PropertyInfo property, IEmitterType targetObject, IEmitterType value)
        {
            return EmitProperty.Store(property, targetObject, value);
        }

        private static IEmitterType StoreTargetObjectMember(MappingMember member, IEmitterType targetObject, IEmitterType convertedMember)
        {
            IEmitterType result = null;
            member.Target
                  .ToOption()
                  .Match(x => x.IsField(), x => result = StoreFiled((FieldInfo)x, targetObject, convertedMember))
                  .Match(x => x.IsProperty(), x => result = StoreProperty((PropertyInfo)x, targetObject, convertedMember));
            return result;
        }

        private IEmitter Build(TypePair parentTypePair, MappingMember member)
        {
            IEmitterType sourceObject = EmitArgument.Load(member.TypePair.Source, 1);
            IEmitterType targetObject = EmitArgument.Load(member.TypePair.Target, 2);

            IEmitterType sourceMember = LoadMember(member.Source, sourceObject);
            IEmitterType targetMember = LoadMember(member.Target, targetObject);
            if (member.IsExpressionMapping)
            {
                var targetName = EmitString.Load(member.Target.Name);
                sourceMember = MapperStatic.EmitMapExpressionMethod(parentTypePair.Source, parentTypePair.Target, sourceObject, targetName);
            }

            IEmitterType convertedMember = ConvertMember(parentTypePair, member, sourceMember, targetMember);

            if (member.Target.IsWritable())
            {
                IEmitter result = StoreTargetObjectMember(member, targetObject, convertedMember);
                return result;
            }
            else
            {
                return convertedMember;
            }
        }

        private IEmitterType ConvertMember(TypePair parentTypePair, MappingMember member, IEmitterType sourceMemeber, IEmitterType targetMember)
        {
            if (member.TypePair.IsDeepCloneable)
            {
                return sourceMemeber;
            }

            //原方案是遇到为复杂类型的属性时，自动生成一个映射类，并使用这个映射类，
            //修改的方案是遇到一般类型的属性时，直接调用TinyMapper.Map<,>方法
            //遇到枚举，集合或可空类型时仍使用原来的方案
            //lpz修改，2017-06-10
            if (member.TypePair.IsEnumerableTypes || member.TypePair.IsEnumTypes || member.TypePair.IsNullableToNotNullable)
            {
                CreateMapperCacheItem(parentTypePair, member);
            }
            if (member.Target.IsWritable())
            {//目标属性可写使用属性赋值方法
                var rst = MapperStatic.EmitMapMethod(sourceMemeber, targetMember);
                return rst;
            }
            else
            {//目标属性不可写，使用无返回值的Map方法
                var rst = MapperStatic.EmitMapVoidMethod(sourceMemeber, targetMember);
                return rst;
            }
        }

        private void CreateMapperCacheItem(TypePair parentTypePair, MappingMember mappingMember)
        {
            MapperBuilder mapperBuilder = _config.GetMapperBuilder(parentTypePair, mappingMember);
            mapperBuilder.Build(parentTypePair, mappingMember);
        }

        private IEmitterType LoadField(IEmitterType source, FieldInfo field)
        {
            return EmitField.Load(source, field);
        }

        private IEmitterType LoadMember(MemberInfo member, IEmitterType sourceObject)
        {
            IEmitterType result = null;
            member.ToOption()
                  .Match(x => x.IsField(), x => result = LoadField(sourceObject, (FieldInfo)x))
                  .Match(x => x.IsProperty(), x => result = LoadProperty(sourceObject, (PropertyInfo)x));
            return result;
        }

        private IEmitterType LoadProperty(IEmitterType source, PropertyInfo property)
        {
            return EmitProperty.Load(source, property);
        }
    }
}
