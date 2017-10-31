using System;
using System.Collections;
using System.Collections.Generic;
using Nelibur.ObjectMapper.Core.Extensions;

namespace Nelibur.ObjectMapper.Mappers.Collections
{
    internal abstract class CollectionMapper<TSource, TTarget> : MapperOf<TSource, TTarget> where TTarget : class
    {
        protected virtual object ConvertItem(object item)
        {
            throw new NotImplementedException();
        }

        protected virtual object ConvertItemKey(object item)
        {
            throw new NotImplementedException();
        }

        protected virtual TTarget DictionaryToDictionary(IEnumerable source)
        {
            throw new NotImplementedException();
        }

        protected Dictionary<TTargetKey, TTargetValue> DictionaryToDictionaryTemplate<TSourceKey, TSourceValue, TTargetKey, TTargetValue>(IEnumerable source)
        {
            var result = new Dictionary<TTargetKey, TTargetValue>();
            foreach (KeyValuePair<TSourceKey, TSourceValue> item in source)
            {
                var key = (TTargetKey)ConvertItemKey(item.Key);
                var value = (TTargetValue)ConvertItem(item.Value);
                result.Add(key, value);
            }
            return result;
        }

        protected virtual TTarget EnumerableToArray(IEnumerable source)
        {
            throw new NotImplementedException();
        }

        protected Array EnumerableToArrayTemplate<TTargetItem>(IEnumerable source)
        {
            var result = new TTargetItem[source.Count()];
            int index = 0;
            foreach (var item in source)
            {
                result[index++] = ((TTargetItem)ConvertItem(item));
            }
            return result;
        }

        protected virtual TTarget EnumerableToList(IEnumerable source)
        {
            throw new NotImplementedException();
        }

        protected List<TTargetItem> EnumerableToListTemplate<TTargetItem>(IEnumerable source)
        {
            var result = new List<TTargetItem>();
            foreach (var item in source)
            {
                result.Add((TTargetItem)ConvertItem(item));
            }
            return result;
        }

        protected virtual TTarget EnumerableToArrayList(IEnumerable source)
        {
            var result = new ArrayList();

            foreach (var item in source)
            {
                result.Add(ConvertItem(item));
            }

            return result as TTarget;
        }

        protected virtual bool EnumerableToTarget(TTarget target, IEnumerable source)
        {
            return false;
        }

        protected bool EnumerableToTargetTemplate<TRealTarget, TTargetItem>(TRealTarget target, IEnumerable source) where TRealTarget : IEnumerable<TTargetItem>
        {
            Inner<TRealTarget, TTargetItem>.Clear?.Invoke(target);
            foreach (var item in source)
            {
                var targetItem = (TTargetItem)ConvertItem(item);
                Inner<TRealTarget, TTargetItem>.AddItem?.Invoke(target, targetItem);
            }
            return true;
        }
        protected virtual TTarget EnumerableToEnumerableOf(IEnumerable source)
        {
            throw new NotImplementedException();
        }

        protected virtual TTarget EnumerableToEnumerableOfTemplate<TRealTarget, TTargetItem>(IEnumerable source) where TRealTarget : class, IEnumerable<TTargetItem>
        {
            List<TTargetItem> result = new List<TTargetItem>();
            foreach (var item in source)
            {
                result.Add((TTargetItem)ConvertItem(item));
            }
            var rst = result as TTarget;
            if (rst == null)
            {
                rst = Inner<TRealTarget, TTargetItem>.New?.Invoke(result) as TTarget;
            }

            return rst;
        }

        protected virtual TTarget EnumerableToEnumerable(IEnumerable source)
        {
            IList result = null;
            foreach (var item in source)
            {
                var titem = ConvertItem(item);
                if (result == null)
                {
                    result = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(titem.GetType()));
                }

                result.Add(titem);
            }
            return result as TTarget;
        }

        protected override TTarget MapCore(TSource source, TTarget target)
        {
            Type targetType = typeof(TTarget);
            var enumerable = (IEnumerable)source;

            //if (targetType.IsListOf())
            //{
            //    return EnumerableToList(enumerable);
            //}
            //else 

            if (target != null)
            {
                //var list = EnumerableToList(enumerable);
                //AddToTarget(target,list);
                if (EnumerableToTarget(target, enumerable))
                {
                    return target;
                }
            }

            if (targetType.IsListOf())
            {
                return EnumerableToList(enumerable);
            }
            else if (targetType.IsArray)
            {
                return EnumerableToArray(enumerable);
            }
            else if (typeof(TSource).IsDictionaryOf() && targetType.IsDictionaryOf())
            {
                return DictionaryToDictionary(enumerable);
            }
            else if (targetType == typeof(ArrayList))
            {
                return EnumerableToArrayList(enumerable);
            }
            else if (targetType.IsIEnumerableOf())
            {
                return EnumerableToEnumerableOf(enumerable);
            }
            else
            {
                return EnumerableToEnumerable(enumerable);
            }

            //string errorMessage = string.Format("Not suppoerted From {0} To {1}", typeof(TSource).Name, targetType.Name);
            //throw new NotSupportedException(errorMessage);
        }

        public class Inner<TT, TItem> where TT : IEnumerable<TItem>
        {
            public static Action<TT, TItem> AddItem { get; set; }
            public static Action<TT> Clear { get; set; }
            public static Func<List<TItem>, TT> New { get; set; }

            static Inner()
            {
                Inner<IList<TItem>, TItem>.AddItem = (l, i) => l.Add(i);
                Inner<IList<TItem>, TItem>.Clear = l => l.Clear();
                Inner<IList<TItem>, TItem>.New = l => l;

                Inner<List<TItem>, TItem>.AddItem = (l, i) => l.Add(i);
                Inner<List<TItem>, TItem>.Clear = l => l.Clear();
                Inner<List<TItem>, TItem>.New = l => l;

                Inner<ICollection<TItem>, TItem>.AddItem = (l, i) => l.Add(i);
                Inner<ICollection<TItem>, TItem>.Clear = l => l.Clear();
                Inner<ICollection<TItem>, TItem>.New = l => l;

                Inner<HashSet<TItem>, TItem>.AddItem = (l, i) => l.Add(i);
                Inner<HashSet<TItem>, TItem>.Clear = l => l.Clear();
                Inner<HashSet<TItem>, TItem>.New = l => new HashSet<TItem>(l);

                Inner<ISet<TItem>, TItem>.AddItem = (l, i) => l.Add(i);
                Inner<ISet<TItem>, TItem>.Clear = l => l.Clear();
                Inner<ISet<TItem>, TItem>.New = l => new HashSet<TItem>(l);

                Inner<Queue<TItem>, TItem>.Clear = l => l.Clear();
                Inner<Queue<TItem>, TItem>.AddItem = (l, i) => l.Enqueue(i);
                Inner<Queue<TItem>, TItem>.New = l => new Queue<TItem>(l);

                Inner<Stack<TItem>, TItem>.AddItem = (l, i) => l.Push(i);
                Inner<Stack<TItem>, TItem>.Clear = l => l.Clear();
                Inner<Stack<TItem>, TItem>.New = l => new Stack<TItem>(l);
            }
        }
    }
}
