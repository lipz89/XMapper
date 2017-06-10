using System;
using System.Collections.Generic;
using System.Linq;

using XMapper.Config;

namespace XMapper.Builder
{
    internal class CodeBuilder : BaseBuilder
    {
        private readonly MapperConfig config;

        public CodeBuilder(MapperConfig config)
        {
            this.config = config;
            classes = config.List?.Select(x => new ClassBuilder(x));
            //collections = config.List?.SelectMany(t => t.PropertyPairs.Where(x => x.IsNeedCollectionMapper))
            //    .Select(x => new CollectionBuilder(x));
        }
        internal const string AssemblyName = "DynamicXMapper";
        private const string formatAllClasses = "namespace DynamicXMapper.ClassMappers\r\n{{\r\n\t{0}\r\n}}";
        //private const string formatAllCollections = "namespace DynamicXMapper.CollectionMappers\r\n{{\r\n\t{0}\r\n}}";

        private readonly IEnumerable<ClassBuilder> classes;
        //private readonly IEnumerable<CollectionBuilder> collections;
        internal override string CreateCode()
        {
            var code = string.Empty;
            var classCode = CreateCode(classes);
            if (!string.IsNullOrWhiteSpace(classCode))
            {
                code += string.Format(formatAllClasses, classCode);
            }
            //code += "\r\n\r\n\r\n";
            //var collectionCode = CreateCode(collections);
            //if (!string.IsNullOrWhiteSpace(collectionCode))
            //{
            //    code += string.Format(formatAllCollections, collectionCode);
            //}
            return code.Trim();
        }

        private string CreateCode(IEnumerable<BaseBuilder> builders)
        {
            var clss = builders?.Select(x => x.CreateCode()).Where(x => !string.IsNullOrWhiteSpace(x));
            if (clss?.Any() == true)
            {
                return clss.Aggregate((x, i) => x + "\r\n\r\n" + i).Trim();
            }
            return string.Empty;
        }

        internal IEnumerable<string> GetReferencedAssemblies()
        {
            var ass = new List<string>();
            ass.AddRange(GetReferencedAssemblies(config.GetType()));
            foreach (var typePair in this.config.List)
            {
                ass.AddRange(GetReferencedAssemblies(typePair.SourceType));
                ass.AddRange(GetReferencedAssemblies(typePair.ResultType));
                foreach (var propertyPair in typePair.PropertyPairs)
                {
                    ass.AddRange(GetReferencedAssemblies(propertyPair.SourcePropertyType));
                    ass.AddRange(GetReferencedAssemblies(propertyPair.ResultPropertyType));
                }
            }
            return ass.Distinct();
        }
        private IEnumerable<string> GetReferencedAssemblies(Type type)
        {
            var ass = new List<string>();

            if (type == null)
            {
                return ass;
            }

            if (type.IsGenericType)
            {
                ass.Add(type.GetGenericTypeDefinition().Assembly.Location);
                var ps = type.GetGenericArguments().Where(x => !x.IsGenericParameter);
                ass.AddRange(ps.SelectMany(GetReferencedAssemblies));
            }
            else
            {
                ass.Add(type.Assembly.Location);
            }

            return ass.Distinct();
        }
    }
}
