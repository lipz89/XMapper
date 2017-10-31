using System;
using System.Linq;
using System.Reflection;
using Nelibur.ObjectMapper.CodeGenerators.Emitters;
using Nelibur.ObjectMapper.Core.DataStructures;

namespace Nelibur.ObjectMapper.Mappers.Caches
{
    /// <summary>
    /// 为指定的类型生产TinyMapper.Map&lt;,&gt;方法调用的Emit代码，
    /// lpz增加，2017-06-10
    /// </summary>
    internal sealed class MapperStatic
    {
        private static readonly MethodInfo StaticMapper = typeof(TinyMapper).GetMethod("MapCore", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo StaticVoidMapper = typeof(TinyMapper).GetMethod("MapVoid", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo StaticExpressionMapper = typeof(ExpressionCache).GetMethod("GetValue", BindingFlags.Static | BindingFlags.NonPublic);


        public static IEmitterType EmitMapMethod(IEmitterType sourceMemeber, IEmitterType targetMember)
        {
            MethodInfo mapMethod = StaticMapper.GetGenericMethodDefinition().MakeGenericMethod(sourceMemeber.ObjectType, targetMember.ObjectType);
            IEmitterType result = EmitMethod.CallStatic(mapMethod, sourceMemeber, targetMember);
            return result;
        }
        public static IEmitterType EmitMapVoidMethod(IEmitterType sourceMemeber, IEmitterType targetMember)
        {
            MethodInfo mapMethod = StaticVoidMapper.GetGenericMethodDefinition().MakeGenericMethod(sourceMemeber.ObjectType, targetMember.ObjectType);
            IEmitterType result = EmitMethod.CallStatic(mapMethod, sourceMemeber, targetMember);
            return result;
        }
        public static IEmitterType EmitMapMethodObject(IEmitterType sourceMemeber, IEmitterType targetMember, TypePair typePair)
        {
            MethodInfo mapMethod = StaticMapper.GetGenericMethodDefinition().MakeGenericMethod(typePair.Source, typePair.Target);
            IEmitterType result = EmitMethod.CallStatic(mapMethod, sourceMemeber, targetMember);
            return result;
        }
        public static IEmitterType EmitMapExpressionMethod(Type sourceType, Type targetType, IEmitterType sourceObject, IEmitterType targetName)
        {
            MethodInfo mapMethod = StaticExpressionMapper.GetGenericMethodDefinition().MakeGenericMethod(sourceType, targetType);
            IEmitterType result = EmitMethod.CallStatic(mapMethod, targetName, sourceObject);
            return result;
        }
    }
}