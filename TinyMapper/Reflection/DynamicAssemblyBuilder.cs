using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Nelibur.ObjectMapper.Reflection
{
    internal class DynamicAssemblyBuilder
    {
        internal const string AssemblyName = "DynamicTinyMapper";
        private static readonly DynamicAssembly _dynamicAssembly = new DynamicAssembly();
#if XDEBUG
        private const string AssemblyNameFileName = AssemblyName + ".dll";
        private static AssemblyBuilder _assemblyBuilder;
#endif
        public static IDynamicAssembly Get()
        {
            return _dynamicAssembly;
        }

        private sealed class DynamicAssembly : IDynamicAssembly
        {
            private readonly ModuleBuilder _moduleBuilder;

            public DynamicAssembly()
            {
                var assemblyName = new AssemblyName(AssemblyName);
#if XDEBUG
                _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
                _moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName.Name, AssemblyNameFileName, true);
#else
                AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                _moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
#endif
            }

            public TypeBuilder DefineType(string typeName, Type parentType)
            {
                return _moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Sealed, parentType, null);
            }

            public void Save()
            {
#if XDEBUG
                _assemblyBuilder.Save(AssemblyNameFileName);
#endif
            }
        }
    }
}
