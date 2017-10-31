using System;
using System.Reflection.Emit;

namespace Nelibur.ObjectMapper.CodeGenerators.Emitters
{
    internal sealed class EmitString : IEmitterType
    {
        private readonly string value;

        public Type ObjectType { get; private set; }

        public EmitString(string value)
        {
            ObjectType = typeof(string);
            this.value = value;
        }
        public void Emit(CodeGenerator generator)
        {
            generator.Emit(OpCodes.Ldstr, this.value);
        }

        public static IEmitterType Load(string value)
        {
            return new EmitString(value);
        }
    }
}