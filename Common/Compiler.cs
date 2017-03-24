using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.CSharp;

using XMapper.Builder;

namespace XMapper.Common
{
    internal static class Compiler
    {
        internal static Assembly Compile(string code, IEnumerable<string> assemblyPaths = null)
        {
            var parameter = new CompilerParameters();

            assemblyPaths = assemblyPaths?.Distinct() ?? new List<string>();
            //Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //assemblyPaths.AddRange(assemblies.Where(x => !x.IsDynamic).Select(x => x.Location));

            foreach (var assemblyPath in assemblyPaths)
            {
                if (!string.IsNullOrWhiteSpace(assemblyPath) && !parameter.ReferencedAssemblies.Contains(assemblyPath))
                    parameter.ReferencedAssemblies.Add(assemblyPath);
            }

            //parameter.CompilerOptions = "/target:library /optimize";
            //parameter.IncludeDebugInformation = true;
            parameter.GenerateExecutable = false;
            parameter.OutputAssembly = CodeBuilder.AssemblyName + ".dll";
            parameter.GenerateInMemory = true;

            var provider = new CSharpCodeProvider();

            CompilerResults result = provider.CompileAssemblyFromSource(parameter, code);
            if (result.Errors.Count > 0)
            {
                var errorMsg = result.Errors.OfType<CompilerError>()
                    .Aggregate(string.Empty, (current, error) => current + Environment.NewLine + error.ToString());
                throw new Exception(errorMsg);
            }

            return result.CompiledAssembly;
        }
    }
}
