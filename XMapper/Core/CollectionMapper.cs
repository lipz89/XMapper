using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using XMapper.Common;

namespace XMapper.Core
{
    /*
    internal abstract class CollectionMapper<TSource, TTarget> : BaseMapper<TSource, TTarget>
    {
        #region Item & Key

        protected virtual TTargetItem ConvertItem<TTargetItem>(object item)
        {
            if (typeof(TTargetItem) == typeof(object))
            {
                return (TTargetItem)item;
            }
            return MapperRoute.Map<TTargetItem>(item);
        }
        protected virtual TTargetItem ConvertItem<TSourceItem, TTargetItem>(TSourceItem item)
        {
            if (typeof(TTargetItem) == typeof(object))
            {
                return (TTargetItem)(object)item;
            }
            return MapperRoute.Map<TSourceItem, TTargetItem>(item);
        }

        protected virtual TTargetKey ConvertItemKey<TSourceKey, TTargetKey>(TSourceKey item)
        {
            return MapperRoute.Map<TSourceKey, TTargetKey>(item);
        }

        #endregion

        #region Dictionary
        protected virtual TTarget DictionaryToDictionary(TSource source)
        {
            throw new NotImplementedException();
        }

        protected Dictionary<TTargetKey, TTargetValue> DictionaryToDictionaryTemplate<TSourceKey, TSourceValue, TTargetKey, TTargetValue>(IEnumerable source)
        {
            var result = new Dictionary<TTargetKey, TTargetValue>();
            foreach (KeyValuePair<TSourceKey, TSourceValue> item in source)
            {
                var key = ConvertItemKey<TSourceKey, TTargetKey>(item.Key);
                var value = ConvertItem<TSourceValue, TTargetValue>(item.Value);
                result.Add(key, value);
            }
            return result;
        }

        #endregion

        #region List
        protected virtual TTarget EnumerableToEnumerable(TSource source)
        {
            throw new NotImplementedException();
        }

        protected ArrayList EnumerableToListTemplate(IEnumerable source)
        {
            var result = new ArrayList();
            foreach (object item in source)
            {
                result.Add(item);
            }
            return result;
        }

        protected List<TTargetItem> EnumerableToListTemplate<TTargetItem>(IEnumerable source)
        {
            var result = new List<TTargetItem>();
            foreach (object item in source)
            {
                result.Add(ConvertItem<TTargetItem>(item));
            }
            return result;
        }
        protected List<TTargetItem> EnumerableToListTemplate<TSourceItem, TTargetItem>(IEnumerable<TSourceItem> source)
        {
            var result = new List<TTargetItem>();
            foreach (var item in source)
            {
                result.Add(ConvertItem<TSourceItem, TTargetItem>(item));
            }
            return result;
        }

        #endregion

        #region MapCore
        public override TTarget Map(TSource source, TTarget target)
        {
            return MapCore(source, target);
        }
        private TTarget MapCore(TSource source, TTarget target)
        {
            if (sourceType.HasInterfaceOf(typeof(IDictionary<,>)) && targetType.HasInterfaceOf(typeof(IDictionary<,>)))
            {
                return DictionaryToDictionary(source);
            }

            return EnumerableToEnumerable(source);
        }
        #endregion
    }

    #region Converter

    internal class Inner<TItem>
    {
        //static Inner()
        //{
        //    InnerMapper<IList<TItem>>.Mapper = x => x;
        //    InnerMapper<List<TItem>>.Mapper = x => x;
        //    InnerMapper<TI[]>.Mapper = x => x.ToArray();
        //    InnerMapper<IReadOnlyList<TItem>>.Mapper = x => x.AsReadOnly();
        //    InnerMapper<IReadOnlyCollection<TItem>>.Mapper = x => x.AsReadOnly();
        //    InnerMapper<ReadOnlyCollection<TItem>>.Mapper = x => x.AsReadOnly();
        //    InnerMapper<ArraySegment<TItem>>.Mapper = x => new ArraySegment<TItem>(x.ToArray());
        //    InnerMapper<ISet<TItem>>.Mapper = x => new HashSet<TItem>(x);
        //    InnerMapper<HashSet<TItem>>.Mapper = x => new HashSet<TItem>(x);
        //    InnerMapper<Queue<TItem>>.Mapper = x => new Queue<TItem>(x);
        //    InnerMapper<Stack<TItem>>.Mapper = x => new Stack<TItem>(x);
        //    InnerMapper<LinkedList<TItem>>.Mapper = x => new LinkedList<TItem>(x);
        //    InnerMapper<ConcurrentBag<TItem>>.Mapper = x => new ConcurrentBag<TItem>(x);
        //}

        //public static TResult Mapper<TResult>(List<TItem> list) where TResult : class
        //{
        //    if (list is TResult)
        //        return list as TResult;

        //    var mapper = InnerMapper<TResult>.Mapper;
        //    if (mapper != null)
        //    {
        //        return mapper(list);
        //    }

        //    throw new Exception(string.Format("没有提供映射程序 {0} --> {1}",
        //                                      typeof(List<TItem>).ObtainOriginalName(),
        //                                      typeof(TResult).ObtainOriginalName()));
        //}

        //class InnerMapper<TResult>
        //{
        //    public static Func<List<TItem>, TResult> Mapper { get; set; }
        //}

        public static Func<List<TItem>, List<TItem>> ToList = x => x;
        public static Func<List<TItem>, TItem[]> ToArray = x => x.ToArray();
        public static Func<List<TItem>, ReadOnlyCollection<TItem>> ToReadonly = x => x.AsReadOnly();
        public static Func<List<TItem>, ArraySegment<TItem>> ToArraySegment = x => new ArraySegment<TItem>(x.ToArray());
        public static Func<List<TItem>, HashSet<TItem>> ToHashSet = x => new HashSet<TItem>(x.ToArray());
        public static Func<List<TItem>, Queue<TItem>> ToQueue = x => new Queue<TItem>(x.ToArray());
        public static Func<List<TItem>, Stack<TItem>> ToStack = x => new Stack<TItem>(x.ToArray());
        public static Func<List<TItem>, LinkedList<TItem>> ToLinkedList = x => new LinkedList<TItem>(x.ToArray());
        public static Func<List<TItem>, ConcurrentBag<TItem>> ToConcurrentBag = x => new ConcurrentBag<TItem>(x.ToArray());
    }
    internal class Inner
    {
        //static Inner()
        //{
        //    InnerMapper<IList>.Mapper = x => x;
        //    InnerMapper<ArrayList>.Mapper = x => x;
        //    InnerMapper<object[]>.Mapper = x => x.ToArray();
        //    InnerMapper<Queue>.Mapper = x => new Queue(x);
        //    InnerMapper<Stack>.Mapper = x => new Stack(x);
        //}
        //public static TResult Mapper<TResult>(ArrayList list) where TResult : class
        //{
        //    if (list is TResult)
        //        return list as TResult;

        //    var mapper = InnerMapper<TR>.Mapper;
        //    if (mapper != null)
        //    {
        //        return mapper(list);
        //    }

        //    throw new Exception(string.Format("没有提供映射程序 {0} --> {1}",
        //                                      typeof(ArrayList).ObtainOriginalName(),
        //                                      typeof(TResult).ObtainOriginalName()));
        //}

        //class InnerMapper<TResult>
        //{
        //    public static Func<ArrayList, TResult> Mapper { get; set; }
        //}

        public static Func<ArrayList, ArrayList> ToList = x => x;
        public static Func<ArrayList, object[]> ToArray = x => x.ToArray();
        public static Func<ArrayList, Queue> ToQueue = x => new Queue(x);
        public static Func<ArrayList, Stack> ToStack = x => new Stack(x);
    }

    #endregion
    //*/

    internal class CollectionMapper<TSource, TTarget, TSourceItem, TTargetItem> : BaseMapper<TSource, TTarget>
        where TSource : IEnumerable<TSourceItem>
        where TTarget : class, IEnumerable<TTargetItem>
    {
        public override TTarget Map(TSource source, TTarget target)
        {
            return MapCore(source, target);
        }

        protected virtual TTargetItem ConvertItem(TSourceItem item)
        {
            return MapperRoute.Map<TSourceItem, TTargetItem>(item);
        }
        protected List<TTargetItem> EnumerableToList(IEnumerable<TSourceItem> source)
        {
            var result = new List<TTargetItem>();
            foreach (var item in source)
            {
                result.Add(ConvertItem(item));
            }
            return result;
        }

        private TTarget MapCore(TSource source, TTarget target)
        {
            var list = EnumerableToList(source);

            var coll = target as ICollection<TTargetItem>;
            if (coll != null)
            {
                foreach (var item in list)
                {
                    coll.Add(item);
                }
                return target;
            }

            if (list is TTarget)
                return list as TTarget;

            var mapper = InnerMapper<TTargetItem, TTarget>.Mapper;
            if (mapper != null)
            {
                return mapper(list);
            }

            throw Error.Exception("没有到集合类型的映射：" + targetType.ObtainOriginalName());
        }
    }
    internal class CollectionMapper<TSource, TTarget, TTargetItem> : BaseMapper<TSource, TTarget>
        where TSource : IEnumerable
        where TTarget : class, IEnumerable<TTargetItem>
    {
        public override TTarget Map(TSource source, TTarget target)
        {
            var arraySource = source.OfType<object>();
            var mapper = new CollectionMapper<IEnumerable<object>, TTarget, object, TTargetItem>();
            return mapper.Map(arraySource, target);
        }
    }

    internal class CollectionMapper<TSource, TTarget> : BaseMapper<TSource, TTarget>
        where TSource : IEnumerable
        where TTarget : class, IEnumerable
    {
        public override TTarget Map(TSource source, TTarget target)
        {
            return MapCore(source, target);
        }
        private TTarget MapCore(TSource source, TTarget target)
        {
            var list = EnumerableToList(source);

            var coll = target as IList;
            if (coll != null)
            {
                foreach (var item in list)
                {
                    coll.Add(item);
                }
                return target;
            }

            if (list is TTarget)
                return list as TTarget;

            var mapper = InnerMapper<TTarget>.Mapper;
            if (mapper != null)
            {
                return mapper(list);
            }

            throw Error.Exception("没有到集合类型的映射：" + targetType.ObtainOriginalName());
        }

        protected ArrayList EnumerableToList(IEnumerable source)
        {
            var result = new ArrayList();
            foreach (object item in source)
            {
                result.Add(item);
            }
            return result;
        }
    }
    internal class DictionaryMapper<TSource, TTarget, TSourceKey, TSourceItem, TTargetKey, TTargetItem>
        : CollectionMapper<TSource, TTarget, KeyValuePair<TSourceKey, TSourceItem>, KeyValuePair<TTargetKey, TTargetItem>>
        where TSource : IDictionary<TSourceKey, TSourceItem>
        where TTarget : class, IDictionary<TTargetKey, TTargetItem>
    {
        public override TTarget Map(TSource source, TTarget target)
        {
            var list = EnumerableToList(source);

            if (target != null)
            {
                foreach (var item in list)
                {
                    target.Add(item.Key, item.Value);
                }
                return target;
            }

            Dictionary<TTargetKey, TTargetItem> dic = list.ToDictionary(x => x.Key, x => x.Value);

            if (dic is TTarget)
                return dic as TTarget;

            var mapper = InnerDictionaryMapper<TTargetKey, TTargetItem, TTarget>.Mapper;
            if (mapper != null)
            {
                return mapper(dic);
            }

            throw Error.Exception("没有到字典类型的映射：" + targetType.ObtainOriginalName());
        }

        protected override KeyValuePair<TTargetKey, TTargetItem> ConvertItem(KeyValuePair<TSourceKey, TSourceItem> item)
        {
            return new KeyValuePair<TTargetKey, TTargetItem>(MapperRoute.Map<TSourceKey, TTargetKey>(item.Key), MapperRoute.Map<TSourceItem, TTargetItem>(item.Value));
        }
    }

    static class InnerMapper<TTarget>
    {
        static InnerMapper()
        {
            //InnerMapper<IList>.Mapper = x => x;
            //InnerMapper<ArrayList>.Mapper = x => x;
            InnerMapper<Array>.Mapper = x => x.ToArray();
            InnerMapper<Queue>.Mapper = x => new Queue(x);
            InnerMapper<Stack>.Mapper = x => new Stack(x);
        }
        public static Func<ArrayList, TTarget> Mapper { get; set; }
    }
    static class InnerMapper<TItem, TTarget> where TTarget : IEnumerable<TItem>
    {
        static InnerMapper()
        {
            InnerMapper<TItem, TItem[]>.Mapper = x => x.ToArray();
            InnerMapper<TItem, IReadOnlyList<TItem>>.Mapper = x => x.AsReadOnly();
            InnerMapper<TItem, IReadOnlyCollection<TItem>>.Mapper = x => x.AsReadOnly();
            InnerMapper<TItem, ReadOnlyCollection<TItem>>.Mapper = x => x.AsReadOnly();
            InnerMapper<TItem, ArraySegment<TItem>>.Mapper = x => new ArraySegment<TItem>(x.ToArray());
            InnerMapper<TItem, ISet<TItem>>.Mapper = x => new HashSet<TItem>(x);
            InnerMapper<TItem, HashSet<TItem>>.Mapper = x => new HashSet<TItem>(x);
            InnerMapper<TItem, Queue<TItem>>.Mapper = x => new Queue<TItem>(x);
            InnerMapper<TItem, Stack<TItem>>.Mapper = x => new Stack<TItem>(x);
            InnerMapper<TItem, LinkedList<TItem>>.Mapper = x => new LinkedList<TItem>(x);
            InnerMapper<TItem, ConcurrentBag<TItem>>.Mapper = x => new ConcurrentBag<TItem>(x);
        }
        public static Func<List<TItem>, TTarget> Mapper { get; set; }
    }
    static class InnerDictionaryMapper<TKey, TItem, TTarget> where TTarget : IDictionary<TKey, TItem>
    {
        static InnerDictionaryMapper()
        {
            InnerDictionaryMapper<TKey, TItem, ConcurrentDictionary<TKey, TItem>>.Mapper = x => new ConcurrentDictionary<TKey, TItem>(x);
            InnerDictionaryMapper<TKey, TItem, ReadOnlyDictionary<TKey, TItem>>.Mapper = x => new ReadOnlyDictionary<TKey, TItem>(x);
        }
        public static Func<Dictionary<TKey, TItem>, TTarget> Mapper { get; set; }
    }
}