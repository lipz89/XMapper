﻿using System;
using System.Reflection.Emit;

namespace Nelibur.ObjectMapper.CodeGenerators.Emitters
{
    internal sealed class EmitNull : IEmitterType
    {
        private EmitNull()
        {
            ObjectType = typeof(object);
        }

        public Type ObjectType { get; private set; }

        public void Emit(CodeGenerator generator)
        {
            generator.Emit(OpCodes.Ldnull);
        }

        public static IEmitterType Load()
        {
            return new EmitNull();
        }
    }
    internal sealed class EmitNop : IEmitterType
    {
        private EmitNop()
        {
            ObjectType = typeof(object);
        }

        public Type ObjectType { get; private set; }

        public void Emit(CodeGenerator generator)
        {
            generator.Emit(OpCodes.Nop);
        }

        public static IEmitterType Load()
        {
            return new EmitNop();
        }
    }
}
