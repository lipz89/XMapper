using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using XMapper.Common;
using XMapper.Config;

namespace XMapper.Builder
{
    /*
    internal class CollectionBuilder : BaseBuilder
    {
        public CollectionBuilder(PropertyPair pair)
        {
            this.pair = pair;
        }

        private readonly PropertyPair pair;
        private const string varSource = "_source";
        private const string varResult = "_result";

        private const string formatClass =
            "\tinternal sealed class Mapper{0} : XMapper.Core.CollectionMapper<{1}, {2}>\r\n\t" +
            "{{\r\n\t\t" +
            "{3}\r\n\t" +
            "}}";

        private const string fmtDictionaryToDictionary =
            "protected override {0} DictionaryToDictionary({1} {2})\r\n\t\t" +
            "{{\r\n\t\t\t" +
            "return DictionaryToDictionaryTemplate<{3},{4}>({2});\r\n\t\t" +
            "}}";

        private const string fmtDictionaryToReadonly =
            "protected override {0} DictionaryToDictionary({1} {2})\r\n\t\t" +
            "{{\r\n\t\t\t" +
            "return new ReadOnlyDictionary<{4}>(DictionaryToDictionaryTemplate<{3},{4}>({2}));\r\n\t\t" +
            "}}";

        private const string fmtDictionaryToConcurrent =
            "protected override {0} DictionaryToDictionary({1} {2})\r\n\t\t" +
            "{{\r\n\t\t\t" +
            "return new ConcurrentDictionary<{4}>(DictionaryToDictionaryTemplate<{3},{4}>({2}));\r\n\t\t" +
            "}}";

        private const string fmtToArrryList =
            "protected override {0} EnumerableToEnumerable({1} {2})\r\n\t\t" +
            "{{\r\n\t\t\t" +
            "return ({0})Inner.{3}(EnumerableToListTemplate({2}));\r\n\t\t" +
            "}}";

        private const string fmtToList =
            "protected override {0} EnumerableToEnumerable({1} {2})\r\n\t\t" +
            "{{\r\n\t\t\t" +
            "return ({0})Inner<{4}>.{5}(EnumerableToListTemplate<{3}{4}>({2}));\r\n\t\t" +
            "}}";

        private string CreateMethodCode()
        {
            List<string> methods = new List<string>();
            if (pair.ResultPropertyType.HasInterfaceOf(typeof(IDictionary<,>))
                && pair.SourcePropertyType.HasInterfaceOf(typeof(IDictionary<,>)))
            {
                #region Dictionary

                var sgps = pair.SourcePropertyType.GetGenericArguments().ExtractGenericArguments();
                var rgps = pair.ResultPropertyType.GetGenericArguments().ExtractGenericArguments();
                if (pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(ReadOnlyDictionary<,>))
                {
                    var code = string.Format(fmtDictionaryToReadonly,
                                             pair.ResultPropertyType.ObtainOriginalName(),
                                             pair.SourcePropertyType.ObtainOriginalName(),
                                             varSource,
                                             sgps,
                                             rgps);
                    methods.Add(code);
                }
                else if (pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>))
                {
                    var code = string.Format(fmtDictionaryToConcurrent,
                                             pair.ResultPropertyType.ObtainOriginalName(),
                                             pair.SourcePropertyType.ObtainOriginalName(),
                                             varSource,
                                             sgps,
                                             rgps);
                    methods.Add(code);
                }
                else
                {
                    var code = string.Format(fmtDictionaryToDictionary,
                                             pair.ResultPropertyType.ObtainOriginalName(),
                                             pair.SourcePropertyType.ObtainOriginalName(),
                                             varSource,
                                             sgps,
                                             rgps);
                    methods.Add(code);
                }

                #endregion
            }
            else
            {
                #region 结果是IEnumerable

                var method = GetMatchMethodName();
                if (!pair.ResultPropertyType.HasInterfaceOf(typeof(IEnumerable<>)))
                {
                    var code = string.Format(fmtToArrryList,
                                             pair.ResultPropertyType.ObtainOriginalName(),
                                             pair.SourcePropertyType.ObtainOriginalName(),
                                             varSource,
                                             method);
                    methods.Add(code);
                }
                else
                {
                    var sgps = string.Empty;
                    if (pair.SourcePropertyType.HasInterfaceOf(typeof(IEnumerable<>)))
                    {
                        sgps += pair.SourcePropertyType.GetItemType().ObtainOriginalName() + ",";
                    }
                    var rgps = pair.ResultPropertyType.GetItemType().ObtainOriginalName();
                    var code = string.Format(fmtToList,
                                             pair.ResultPropertyType.ObtainOriginalName(),
                                             pair.SourcePropertyType.ObtainOriginalName(),
                                             varSource,
                                             sgps,
                                             rgps,
                                             method);
                    methods.Add(code);
                }

                #endregion
            }
            return methods.Aggregate((x, i) => x + "\r\n\t\t" + i);
        }

        private string GetMatchMethodName()
        {
            var method = "ToList";
            if (pair.ResultPropertyType.IsArray)
            {
                method = "ToArray";
            }
            else if (pair.ResultPropertyType == typeof(Queue))
            {
                method = "ToQueue";
            }
            else if (pair.ResultPropertyType == typeof(Stack))
            {
                method = "ToStack";
            }
            else if (pair.ResultPropertyType.IsGenericType)
            {
                if (pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(Queue<>))
                {
                    method = "ToQueue";
                }
                else if (pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(Stack<>))
                {
                    method = "ToStack";
                }
                else if (pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
                         || pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>)
                         || pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>))
                {
                    method = "ToReadonly";
                }
                else if (pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(ArraySegment<>))
                {
                    method = "ToArraySegment";
                }
                else if (pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(HashSet<>)
                         || pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(ISet<>))
                {
                    method = "ToHashSet";
                }
                else if (pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(LinkedList<>))
                {
                    method = "ToLinkedList";
                }
                else if (pair.ResultPropertyType.GetGenericTypeDefinition() == typeof(ConcurrentBag<>))
                {
                    method = "ToConcurrentBag";
                }
            }
            return method;
        }

        internal override string CreateCode()
        {
            var methodCodes = CreateMethodCode().Trim();

            if (!string.IsNullOrWhiteSpace(methodCodes))
            {
                return string.Format(formatClass,
                                     Guid.NewGuid().ToString("N"),
                                     pair.SourcePropertyType.ObtainOriginalName(),
                                     pair.ResultPropertyType.ObtainOriginalName(),
                                     methodCodes);
            }
            return string.Empty;
        }
    }
    //*/
}