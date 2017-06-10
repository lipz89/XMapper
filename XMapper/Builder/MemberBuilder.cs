using XMapper.Common;
using XMapper.Config;

namespace XMapper.Builder
{
    internal class MemberBuilder : BaseBuilder
    {
        private const string formatSetter = "{0} = {1}";
        private const string formatProperty = "{0}.{1}";
        private const string formatExplicitConverter = "({2})({0}.{1})";
        private const string formatInstance = "{0}";
        private const string formatSolver = "{1}({0})";
        private const string formatMapper = "XMapper.Core.MapperRoute.Map<{3}, {4}>({0}.{1},{2})";

        private readonly string varSource;

        private readonly string resultProperty;
        internal PropertyPair Pair { get; }

        public MemberBuilder(PropertyPair Pair, string varSource, string varResult)
        {
            this.Pair = Pair;
            this.Pair.CheckConvert();
            this.varSource = varSource;
            this.resultProperty = varResult + "." + this.Pair.ResultPropertyName;
        }


        private string CreateCore()
        {
            if (Pair.IsIgnore)
            {
                return string.Empty;
            }
            if (Pair.IsUseInstance)
            {
                return string.Format(formatInstance, Pair.InstanceString);
            }
            if (Pair.IsUseSolver)
            {
                return string.Format(formatSolver, varSource, Pair.SolverString);
            }
            if (Pair.IsUseExplicitConverter)
            {
                return string.Format(formatExplicitConverter,
                                     varSource,
                                     Pair.SourcePropertyName,
                                     Pair.ResultPropertyType.ObtainOriginalName());
            }
            if (Pair.IsUseSupportedConverter) { }
            if (Pair.IsUseMap)
            {
                return string.Format(formatMapper,
                                     varSource,
                                     Pair.SourcePropertyName,
                                     resultProperty,
                                     Pair.SourcePropertyType.ObtainOriginalName(),
                                     Pair.ResultPropertyType.ObtainOriginalName());
            }
            return string.Format(formatProperty,
                                 varSource,
                                 Pair.SourcePropertyName);
        }

        internal override string CreateCode()
        {
            var code = CreateCore() + ";";
            if (!string.IsNullOrWhiteSpace(code))
            {
                if (Pair.CanWrite)
                {
                    return string.Format(formatSetter, resultProperty, code);
                }
                if (Pair.IsUseClassMap)
                {
                    return code;
                }
            }
            return string.Empty;
        }
    }
}