using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using XMapper.Common;

namespace XMapper.Config
{
    /// <summary>
    /// 类型对基类
    /// 名称相同的属性默认自动绑定
    /// 本类型绑定方法和忽略方法中所有表示属性的表达式均表示表达式或公开字段
    /// </summary>
    [DebuggerDisplay("{SourceType.Name} -> {ResultType.Name}")]
    public class TypePair
    {
        internal TypePair(Type source, Type result)
        {
            SourceType = source;
            ResultType = result;
        }
        /// <summary>
        /// 源类型
        /// </summary>
        public Type SourceType { get; }
        /// <summary>
        /// 目标类型
        /// </summary>
        public Type ResultType { get; }

        /// <summary>
        /// 类型配置要转换的属性列表
        /// </summary>
        public IReadOnlyList<PropertyPair> PropertyPairs => _propertyPairs.AsReadOnly();

        protected readonly List<PropertyPair> _propertyPairs = new List<PropertyPair>();

        protected internal virtual void InitMap()
        {
            var sourceProperties = ReflectionCache.GetMembers(SourceType).Where(x => x.CanRead()).ToList();
            var resultProperties = ReflectionCache.GetMembers(ResultType).Where(x => x.CanWrite()).ToList();

            foreach (var accessor in resultProperties)
            {
                var source = sourceProperties.FirstOrDefault(x => x.Name == accessor.Name);
                if (source != null)
                {
                    this.InitBind(accessor, source);
                }
            }
        }
        protected void InitBind(MemberInfo resultProperty, MemberInfo sourceProperty)
        {
            var exists = this._propertyPairs.Any(x => x.ResultPropertyName == resultProperty.Name);
            if (!exists)
            {
                var pair = new PropertyPair
                {
                    ResultPropertyName = resultProperty.Name,
                    SourcePropertyName = sourceProperty.Name,
                    ResultPropertyType = resultProperty.ThisType(),
                    SourcePropertyType = sourceProperty.ThisType()
                };

                this._propertyPairs.Add(pair);
            }
        }
        ///// <summary>
        ///// 绑定一个转换属性
        ///// </summary>
        ///// <param name="resultProperty"></param>
        ///// <param name="sourceProperty"></param>
        ///// <returns></returns>
        //public TypePair Bind(string resultProperty, string sourceProperty)
        //{
        //    var piResult = ResultType.GetProperty(resultProperty);
        //    if (piResult == null || !piResult.CanWrite)
        //    {
        //        throw new Exception("指定的参数 " + resultProperty + " 不是属性或是不可写属性。");
        //    }
        //    var piSource = SourceType.GetProperty(sourceProperty);
        //    if (piSource == null || !piSource.CanRead)
        //    {
        //        throw new Exception("指定的参数 " + sourceProperty + " 不是属性或是不可读属性。");
        //    }

        //    var pair = new PropertyPair
        //    {
        //        ResultPropertyName = resultProperty,
        //        SourcePropertyName = sourceProperty,
        //        ResultPropertyType = piResult.PropertyType,
        //        SourcePropertyType = piSource.PropertyType
        //    };

        //    CheckProperty(pair);
        //    return this;
        //}
        protected void CheckProperty(PropertyPair pair)
        {
            var exists = this._propertyPairs.FirstOrDefault(x => x.ResultPropertyName == pair.ResultPropertyName);
            if (exists != null)
                this._propertyPairs.Remove(exists);

            this._propertyPairs.Add(pair);
        }
    }

    /// <summary>
    /// 泛型的类型对
    /// 名称相同的属性默认自动绑定
    /// </summary>
    /// <typeparam name="TSource">源类型</typeparam>
    /// <typeparam name="TTarget">目标类型</typeparam>
    public class TypePair<TSource, TTarget> : TypePair
        where TTarget : new()
    {
        internal TypePair() : base(typeof(TSource), typeof(TTarget))
        {
        }
        /// <summary>
        /// 绑定一个属性对
        /// </summary>
        /// <typeparam name="TSourceProperty">源属性类型</typeparam>
        /// <typeparam name="TResultProperty">目标属性类型</typeparam>
        /// <param name="resultProperty">目标属性表达式</param>
        /// <param name="sourceProperty">源属性表达式</param>
        /// <returns>返回当前类型对</returns>
        public TypePair<TSource, TTarget> Bind<TSourceProperty, TResultProperty>(Expression<Func<TTarget, TResultProperty>> resultProperty, Expression<Func<TSource, TSourceProperty>> sourceProperty)
        {
            if (sourceProperty == null) throw Error.ArgumentNullException(nameof(sourceProperty));
            if (resultProperty == null) throw Error.ArgumentNullException(nameof(resultProperty));
            if (!(resultProperty.Body is MemberExpression))
                throw Error.ArgumentException("指定的参数不是属性或公开字段。", nameof(resultProperty));

            var resultName = resultProperty.GetPropertyName();

            if (!((MemberExpression)resultProperty.Body).Member.CanWrite())
                throw Error.ArgumentException("指定的属性 " + resultName + " 不可写。", nameof(resultProperty));

            var tResult = typeof(TResultProperty);
            var tSource = typeof(TSourceProperty);
            var pair = new PropertyPair
            {
                ResultPropertyName = resultName,
                ResultPropertyType = tResult,
                SourcePropertyType = tSource
            };
            if (sourceProperty.Body.NodeType == ExpressionType.MemberAccess)
            {
                pair.SourcePropertyName = sourceProperty.GetPropertyName();
            }
            else
            {
                pair.IsUseSolver = true;
                pair.Solver = sourceProperty;
                pair.SolverString = typeof(MapperInstances<Func<TSource, TSourceProperty>>).ObtainOriginalName()
                    + ".Instance[" + MapperInstances<Func<TSource, TSourceProperty>>.Instance.Count + "]";
                MapperInstances<Func<TSource, TSourceProperty>>.Instance.Add(sourceProperty.Compile());
            }

            CheckProperty(pair);
            return this;
        }
        /*
        /// <summary>
        /// 绑定一个属性对,
        /// 或者绑定一个源对象相关的表达式
        /// </summary>
        /// <typeparam name="TProperty">目标属性类型</typeparam>
        /// <param name="resultProperty">目标属性表达式</param>
        /// <param name="sourceProperty">源属性表达式</param>
        /// <returns>返回当前类型对</returns>
        public TypePair<TSource, TTarget> Bind<TProperty>(Expression<Func<TTarget, TProperty>> resultProperty, Expression<Func<TSource, TProperty>> sourceProperty)
        {
            if (sourceProperty == null) throw Error.ArgumentNullException(nameof(sourceProperty));
            if (sourceProperty.Body.NodeType == ExpressionType.MemberAccess)
            {
                Bind<TProperty, TProperty>(resultProperty, sourceProperty);
                return this;
            }

            if (resultProperty == null) throw Error.ArgumentNullException(nameof(resultProperty));

            if (!(resultProperty.Body is MemberExpression))
                throw Error.ArgumentException("指定的参数不是属性或公开字段。", nameof(resultProperty));

            var resultName = resultProperty.GetPropertyName();

            if (!((MemberExpression)resultProperty.Body).Member.CanWrite())
                throw Error.ArgumentException("指定的属性 " + resultName + " 不可写。", nameof(resultProperty));

            var tResult = typeof(TProperty);
            var pair = new PropertyPair
            {
                ResultPropertyName = resultName,
                ResultPropertyType = tResult,
                SourcePropertyType = tResult,
                IsUseSolver = true,
            };

            pair.Solver = sourceProperty;
            pair.SolverString = typeof(MapperInstances<Func<TSource, TProperty>>).ObtainOriginalName()
                + ".Instance[" + MapperInstances<Func<TSource, TProperty>>.Instance.Count + "]";
            MapperInstances<Func<TSource, TProperty>>.Instance.Add(sourceProperty.Compile());

            CheckProperty(pair);
            return this;
        }
        //*/
        /// <summary>
        /// 为目标属性绑定一个实例
        /// </summary>
        /// <typeparam name="TProperty">目标属性类型</typeparam>
        /// <param name="resultProperty">目标属性表达式</param>
        /// <param name="instance">要绑定的实例对象</param>
        /// <returns>返回当前类型对</returns>
        public TypePair<TSource, TTarget> Bind<TProperty>(Expression<Func<TTarget, TProperty>> resultProperty, TProperty instance)
        {
            if (resultProperty == null) throw Error.ArgumentNullException(nameof(resultProperty));
            if (instance == null) throw Error.ArgumentNullException(nameof(instance));

            if (!(resultProperty.Body is MemberExpression))
                throw Error.ArgumentException("指定的参数不是属性或公开字段。", nameof(resultProperty));

            var resultName = resultProperty.GetPropertyName();

            if (!((MemberExpression)resultProperty.Body).Member.CanWrite())
                throw Error.ArgumentException("指定的属性 " + resultName + " 不可写。", nameof(resultProperty));

            var tResult = typeof(TProperty);
            var pair = new PropertyPair
            {
                ResultPropertyName = resultName,
                ResultPropertyType = tResult,
                SourcePropertyType = tResult,
                IsUseInstance = true
            };

            pair.Instance = instance;
            pair.InstanceString = typeof(MapperInstances<TProperty>).ObtainOriginalName()
                + ".Instance[" + MapperInstances<TProperty>.Instance.Count + "]";
            MapperInstances<TProperty>.Instance.Add(instance);

            CheckProperty(pair);
            return this;
        }
        /// <summary>
        /// 忽略一个或多个目标属性的转换
        /// </summary>
        /// <param name="resultProperties">要忽略的目标属性的列表</param>
        /// <returns>返回当前类型对</returns>
        public TypePair<TSource, TTarget> Ignore(params Expression<Func<TTarget, object>>[] resultProperties)
        {
            foreach (var expression in resultProperties)
            {
                if (expression.Body.NodeType != ExpressionType.MemberAccess)
                {
                    throw Error.ArgumentException("指定的参数不是属性或公开字段。", nameof(resultProperties));
                }

                var name = expression.GetPropertyName();
                var pair = new PropertyPair
                {
                    IsIgnore = true,
                    ResultPropertyName = name
                };

                CheckProperty(pair);
            }
            return this;
        }
    }
}
