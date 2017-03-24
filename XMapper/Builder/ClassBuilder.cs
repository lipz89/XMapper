using System;
using System.Collections.Generic;
using System.Linq;

using XMapper.Common;
using XMapper.Config;

namespace XMapper.Builder
{
    internal class ClassBuilder : BaseBuilder
    {
        private const string varSource = "_source";
        private const string varResult = "_result";

        private const string formatClass =
            "\tinternal sealed class Mapper{0} : XMapper.Core.ClassMapper<{1}, {2}>\r\n\t" +
            "{{\r\n\t\t" +
                "protected override {2} CreateTargetInstance()\r\n\t\t" +
                "{{\r\n\t\t\t" +
                    "return new {2}();\r\n\t\t" +
                "}}\r\n\r\n\t\t" +
                "protected override {2} MapClass({1} {3}, {2} {4})\r\n\t\t" +
                "{{\r\n\t\t\t" +
                    "{5}" +
                    "\r\n\t\t\treturn {4};\r\n\t\t" +
                "}}\r\n\r\n\t\t" +
                "public override void SetMapsCoreInner({1} {3}, {2} {4})\r\n\t\t" +
                "{{\r\n\t\t\t" +
                    "{6}" +
                    "\r\n\t\t" +
                "}}\r\n\t" +
            "}}";
        private readonly TypePair typePair;

        private readonly IEnumerable<MemberBuilder> members;

        public ClassBuilder(TypePair typePair)
        {
            this.typePair = typePair;

            members = typePair.PropertyPairs.Select(x => new MemberBuilder(x, varSource, varResult));
        }
        internal override string CreateCode()
        {
            if (!members.Any())
                return string.Empty;
            return CreateCore();
        }

        private string CreateCore()
        {
            var propertySets = members?
                .Where(x => !x.Pair.IsUseMap)
                .Select(x => x.CreateCode());
            var propertyMaps = members?
                .Where(x => x.Pair.IsUseMap)
                .Select(x => x.CreateCode());

            return string.Format(formatClass,
                                 Guid.NewGuid().ToString("N"),
                                 typePair.SourceType.ObtainOriginalName(),
                                 typePair.ResultType.ObtainOriginalName(),
                                 varSource,
                                 varResult,
                                 Join(propertySets),
                                 Join(propertyMaps));
        }

        private string Join(IEnumerable<string> codes)
        {
            var cs = codes?.Where(x => !string.IsNullOrWhiteSpace(x));
            if (cs?.Any() == true)
            {
                return cs.Aggregate((x, i) => x + "\r\n\t\t\t" + i).Trim();
            }
            return string.Empty;
        }
    }
}
