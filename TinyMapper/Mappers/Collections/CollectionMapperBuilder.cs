using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Nelibur.ObjectMapper.CodeGenerators;
using Nelibur.ObjectMapper.CodeGenerators.Emitters;
using Nelibur.ObjectMapper.Core.DataStructures;
using Nelibur.ObjectMapper.Core.Extensions;
using Nelibur.ObjectMapper.Mappers.Caches;
using Nelibur.ObjectMapper.Mappers.Classes.Members;

namespace Nelibur.ObjectMapper.Mappers.Collections
{
    internal sealed class CollectionMapperBuilder : MapperBuilder
    {
        private const string ConvertItemKeyMethod = "ConvertItemKey";
        private const string ConvertItemMethod = "ConvertItem";
        private const string DictionaryToDictionaryMethod = "DictionaryToDictionary";
        private const string DictionaryToDictionaryTemplateMethod = "DictionaryToDictionaryTemplate";
        private const string EnumerableToArrayMethod = "EnumerableToArray";
        private const string EnumerableToArrayTemplateMethod = "EnumerableToArrayTemplate";
        private const string EnumerableToListMethod = "EnumerableToList";
        private const string EnumerableToListTemplateMethod = "EnumerableToListTemplate";
        private const string EnumerableToTargetMethod = "EnumerableToTarget";
        private const string EnumerableToTargetTemplateMethod = "EnumerableToTargetTemplate";
        private const string EnumerableToEnumerableMethod = "EnumerableToEnumerable";
        private const string EnumerableToEnumerableTemplateMethod = "EnumerableToEnumerableTemplate";
        private bool IsTargetWritable = true;

        public CollectionMapperBuilder(IMapperBuilderConfig config) : base(config)
        {
        }

        protected override string ScopeName
        {
            get { return "CollectionMappers"; }
        }

        protected override Mapper BuildCore(TypePair typePair)
        {
            Type parentType = typeof(CollectionMapper<,>).MakeGenericType(typePair.Source, typePair.Target);
            TypeBuilder typeBuilder = _assembly.DefineType(GetMapperFullName(typePair), parentType);

            Type sourceItemType = typePair.Source.GetCollectionItemType();
            Type targetItemType = typePair.Target.GetCollectionItemType();
            if (!IsTargetWritable)
            {
                EmitConvertItem(typeBuilder, new TypePair(sourceItemType, targetItemType));
                EmitAddEnumerableToTarget(parentType, typeBuilder, typePair);
                EmitEnumerableToList(parentType, typeBuilder, typePair);
            }
            else if (IsIEnumerableToList(typePair))
            {
                EmitConvertItem(typeBuilder, new TypePair(sourceItemType, targetItemType));
                EmitEnumerableToList(parentType, typeBuilder, typePair);
            }
            else if (IsIEnumerableToArray(typePair))
            {
                EmitConvertItem(typeBuilder, new TypePair(sourceItemType, targetItemType));
                EmitEnumerableToArray(parentType, typeBuilder, typePair);
            }
            else if (IsDictionaryToDictionary(typePair))
            {
                EmitDictionaryToDictionary(parentType, typeBuilder, typePair);
            }
            else if (IsEnumerableToEnumerable(typePair))
            {
                EmitConvertItem(typeBuilder, new TypePair(sourceItemType, targetItemType));
                EmitEnumerableToEnumerable(parentType, typeBuilder, typePair);
            }
            else
            {
                EmitConvertItem(typeBuilder, new TypePair(sourceItemType, targetItemType));
                EmitEnumerableToList(parentType, typeBuilder, typePair);
            }
            var result = (Mapper)Activator.CreateInstance(typeBuilder.CreateType());
            return result;
        }

        protected override Mapper BuildCore(TypePair parentTypePair, MappingMember mappingMember)
        {
            IsTargetWritable = mappingMember.Target.IsWritable();
            return BuildCore(mappingMember.TypePair);
        }

        protected override bool IsSupportedCore(TypePair typePair)
        {
            return typePair.IsEnumerableTypes;
        }

        private static bool IsDictionaryToDictionary(TypePair typePair)
        {
            return typePair.Source.IsDictionaryOf() && typePair.Target.IsDictionaryOf();
        }

        private static bool IsIEnumerableToArray(TypePair typePair)
        {
            return typePair.Source.IsIEnumerable() && typePair.Target.IsArray;
        }

        private static bool IsIEnumerableToList(TypePair typePair)
        {
            return typePair.Source.IsIEnumerable() && typePair.Target.IsListOf();
        }

        private bool IsEnumerableToEnumerable(TypePair typePair)
        {
            return typePair.Source.IsIEnumerable() && typePair.Target.IsIEnumerableOf();
        }

        private void CreateMapperCacheItem(TypePair typePair)
        {
            MapperBuilder mapperBuilder = GetMapperBuilder(typePair);
            mapperBuilder.Build(typePair);
        }

        private void EmitConvertItem(TypeBuilder typeBuilder, TypePair typePair, string methodName = ConvertItemMethod)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName, OverrideProtected, typeof(object), new[] { typeof(object) });

            //原方案是遇到为复杂类型的属性时，自动生成一个映射类，并使用这个映射类，
            //修改的方案是遇到一般类型的属性时，直接调用TinyMapper.Map<,>方法
            //遇到枚举，集合或可空类型时仍使用原来的方案
            //lpz修改，2017-06-10
            IEmitterType sourceMember = EmitArgument.Load(typeof(object), 1);
            IEmitterType targetMember = EmitNull.Load();
            if (typePair.IsEnumerableTypes || typePair.IsEnumTypes || typePair.IsNullableToNotNullable)
            {
                CreateMapperCacheItem(typePair);
            }
            var rst = MapperStatic.EmitMapMethodObject(sourceMember, targetMember, typePair);
            EmitReturn.Return(rst).Emit(new CodeGenerator(methodBuilder.GetILGenerator()));
        }

        private void EmitDictionaryToDictionary(Type parentType, TypeBuilder typeBuilder, TypePair typePair)
        {
            EmitDictionaryToTarget(parentType, typeBuilder, typePair, DictionaryToDictionaryMethod, DictionaryToDictionaryTemplateMethod);
        }

        private void EmitDictionaryToTarget(Type parentType, TypeBuilder typeBuilder, TypePair typePair,
            string methodName, string templateMethodName)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName, OverrideProtected, typePair.Target, new[] { typeof(IEnumerable) });

            KeyValuePair<Type, Type> sourceTypes = typePair.Source.GetDictionaryItemTypes();
            KeyValuePair<Type, Type> targetTypes = typePair.Target.GetDictionaryItemTypes();

            EmitConvertItem(typeBuilder, new TypePair(sourceTypes.Key, targetTypes.Key), ConvertItemKeyMethod);
            EmitConvertItem(typeBuilder, new TypePair(sourceTypes.Value, targetTypes.Value));

            var arguments = new[] { sourceTypes.Key, sourceTypes.Value, targetTypes.Key, targetTypes.Value };
            MethodInfo methodTemplate = parentType.GetGenericMethod(templateMethodName, arguments);

            IEmitterType returnValue = EmitMethod.Call(methodTemplate, EmitThis.Load(parentType), EmitArgument.Load(typeof(IEnumerable), 1));
            EmitReturn.Return(returnValue).Emit(new CodeGenerator(methodBuilder.GetILGenerator()));
        }
        private void EmitAddEnumerableToTarget(Type parentType, TypeBuilder typeBuilder, TypePair typePair)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(EnumerableToTargetMethod, OverrideProtected, typeof(bool), new[] { typePair.Target, typeof(IEnumerable) });

            Type targetItemType = typePair.Target.GetCollectionItemType();

            MethodInfo methodTemplate = parentType.GetGenericMethod(EnumerableToTargetTemplateMethod, typePair.Target, targetItemType);

            IEmitterType returnValue = EmitMethod.Call(methodTemplate, EmitThis.Load(parentType), EmitArgument.Load(typeof(IEnumerable), 1), EmitArgument.Load(typePair.Target, 2));

            EmitReturn.Return(returnValue).Emit(new CodeGenerator(methodBuilder.GetILGenerator()));
        }

        private void EmitEnumerableToArray(Type parentType, TypeBuilder typeBuilder, TypePair typePair)
        {
            EmitEnumerableToTarget(parentType, typeBuilder, typePair, EnumerableToArrayMethod, EnumerableToArrayTemplateMethod);
        }

        private void EmitEnumerableToList(Type parentType, TypeBuilder typeBuilder, TypePair typePair)
        {
            EmitEnumerableToTarget(parentType, typeBuilder, typePair, EnumerableToListMethod, EnumerableToListTemplateMethod);
        }

        private void EmitEnumerableToEnumerable(Type parentType, TypeBuilder typeBuilder, TypePair typePair)
        {
            EmitEnumerableToTarget(parentType, typeBuilder, typePair, EnumerableToEnumerableMethod, EnumerableToEnumerableTemplateMethod);
        }

        private void EmitEnumerableToTarget(Type parentType, TypeBuilder typeBuilder, TypePair typePair,
            string methodName, string templateMethodName)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName, OverrideProtected, typePair.Target, new[] { typeof(IEnumerable) });

            Type targetItemType = typePair.Target.GetCollectionItemType();

            MethodInfo methodTemplate = parentType.GetGenericMethod(templateMethodName, targetItemType);

            IEmitterType returnValue = EmitMethod.Call(methodTemplate, EmitThis.Load(parentType), EmitArgument.Load(typeof(IEnumerable), 1));
            EmitReturn.Return(returnValue).Emit(new CodeGenerator(methodBuilder.GetILGenerator()));
        }
    }
}
