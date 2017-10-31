using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Nelibur.ObjectMapper.CodeGenerators;
using Nelibur.ObjectMapper.CodeGenerators.Emitters;
using Nelibur.ObjectMapper.Core.DataStructures;
using Nelibur.ObjectMapper.Core.Extensions;
using Nelibur.ObjectMapper.Mappers.Caches;
using Nelibur.ObjectMapper.Mappers.Classes.Members;

namespace Nelibur.ObjectMapper.Mappers.Classes
{
    internal sealed class ClassMapperBuilder : MapperBuilder
    {
        private const string CreateTargetInstanceMethod = "CreateTargetInstance";
        private const string MapClassMethod = "MapClass";
        private const string MapObjectsMethod = "MapClassObjects";
        private const string MapExpressionMethod = "MapExpression";
        private readonly MappingMemberBuilder _mappingMemberBuilder;
        private readonly MemberMapper _memberMapper;
        private List<MappingMember> members;

        public ClassMapperBuilder(IMapperBuilderConfig config) : base(config)
        {
            _memberMapper = new MemberMapper(config);
            _mappingMemberBuilder = new MappingMemberBuilder(config);
        }

        protected override string ScopeName
        {
            get { return "ClassMappers"; }
        }

        protected override Mapper BuildCore(TypePair typePair)
        {
            Type parentType = typeof(ClassMapper<,>).MakeGenericType(typePair.Source, typePair.Target);
            TypeBuilder typeBuilder = _assembly.DefineType(GetMapperFullName(typePair), parentType);
            EmitCreateTargetInstance(typePair.Target, typeBuilder);
            members = _mappingMemberBuilder.Build(typePair).Where(x => !x.Ignored).ToList();
            EmitMapClass(typePair, typeBuilder);
            EmitMapClassObjects(typePair, typeBuilder);

            var result = (Mapper)Activator.CreateInstance(typeBuilder.CreateType());
            return result;
        }

        protected override Mapper BuildCore(TypePair parentTypePair, MappingMember mappingMember)
        {
            return BuildCore(mappingMember.TypePair);
        }

        protected override bool IsSupportedCore(TypePair typePair)
        {
            return true;
        }

        private static void EmitCreateTargetInstance(Type targetType, TypeBuilder typeBuilder)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(CreateTargetInstanceMethod, OverrideProtected, targetType, Type.EmptyTypes);
            var codeGenerator = new CodeGenerator(methodBuilder.GetILGenerator());

            IEmitterType result = targetType.IsValueType ? EmitValueType(targetType, codeGenerator) : EmitRefType(targetType);

            EmitReturn.Return(result, targetType).Emit(codeGenerator);
        }

        private static IEmitterType EmitRefType(Type type)
        {
            return type.HasDefaultCtor() ? EmitNewObj.NewObj(type) : EmitNull.Load();
        }

        private static IEmitterType EmitValueType(Type type, CodeGenerator codeGenerator)
        {
            LocalBuilder builder = codeGenerator.DeclareLocal(type);
            EmitLocalVariable.Declare(builder).Emit(codeGenerator);
            return EmitBox.Box(EmitLocal.Load(builder));
        }

        private void EmitMapClass(TypePair typePair, TypeBuilder typeBuilder)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(MapClassMethod, OverrideProtected, typePair.Target,
                new[] { typePair.Source, typePair.Target });
            var codeGenerator = new CodeGenerator(methodBuilder.GetILGenerator());

            var emitterComposite = new EmitComposite();

            IEmitter emitter = EmitMappingMembers(typePair);

            emitterComposite.Add(emitter);
            emitterComposite.Add(EmitReturn.Return(EmitArgument.Load(typePair.Target, 2)));
            emitterComposite.Emit(codeGenerator);
        }

        private IEmitter EmitMappingMembers(TypePair typePair)
        {
            List<MappingMember> _members = members.Where(x => x.TypePair.IsDeepCloneable).ToList();
            IEmitter result = _memberMapper.Build(typePair, _members);
            return result;
        }

        private void EmitMapClassObjects(TypePair typePair, TypeBuilder typeBuilder)
        {
            IEmitter emitter = EmitMappingObjects(typePair);
            if (emitter != null)
            {
                MethodBuilder methodBuilder = typeBuilder.DefineMethod(MapObjectsMethod, OverrideProtected, typePair.Target, new[] { typePair.Source, typePair.Target });
                var codeGenerator = new CodeGenerator(methodBuilder.GetILGenerator());

                var emitterComposite = new EmitComposite();
                emitterComposite.Add(emitter);
                emitterComposite.Add(EmitReturn.Return(EmitArgument.Load(typePair.Target, 2)));
                emitterComposite.Emit(codeGenerator);
            }
        }

        private IEmitter EmitMappingObjects(TypePair typePair)
        {
            List<MappingMember> _members = members.Where(x => !x.TypePair.IsDeepCloneable).ToList();
            if (_members.Any())
            {
                IEmitter result = _memberMapper.Build(typePair, _members);
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
