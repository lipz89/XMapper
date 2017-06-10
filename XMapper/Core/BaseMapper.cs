using System;

using XMapper.Common;

namespace XMapper.Core
{
    /// <summary>
    /// 基础转换对象抽象类
    /// </summary>
    public abstract class BaseMapper
    {
        internal virtual bool NeedSetMaps{get { return false; }}
        internal object Map(object source, object target = null)
        {
            return MapCore(source, target);
        }

        internal abstract object MapCore(object source, object target);

        internal void SetMaps(object source, object target)
        {
            SetMapsCore(source, target);
        }
        internal abstract void SetMapsCore(object source, object target);
    }
    /// <summary>
    /// 泛型转换对象抽象类
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    public abstract class BaseMapper<TSource, TTarget> : BaseMapper
    {
        /// <summary>
        /// 源类型
        /// </summary>
        protected readonly Type sourceType;
        /// <summary>
        /// 目标类型
        /// </summary>
        protected readonly Type targetType;
        /// <summary>
        /// 源类型（Nullable的非空类型）
        /// </summary>
        protected readonly Type realSourceType;
        /// 目标类型（Nullable的非空类型）
        protected readonly Type realTargetType;

        protected BaseMapper()
        {
            sourceType = typeof(TSource);
            targetType = typeof(TTarget);
            realSourceType = sourceType.NonNullableType();
            realTargetType = targetType.NonNullableType();
        }
        internal override object MapCore(object source, object target)
        {
            if (source == null)
            {
                return default(TTarget);
            }
            return Map((TSource)source, (TTarget)target);
        }
        /// <summary>
        /// 基础属性转换方法
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        /// <returns></returns>
        public abstract TTarget Map(TSource source, TTarget target);

        internal void SetMaps(TSource source, TTarget target)
        {
            if (target.IsNotNull())
            {
                SetMapsCoreInner(source, target);
            }
        }

        internal override void SetMapsCore(object source, object target)
        {
            if (target == null)
            {
                return;
            }
            SetMaps((TSource)source, (TTarget)target);
        }
        /// <summary>
        /// 复杂属性转换方法，一般没有递归属性不需要重写本方法
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        protected virtual void SetMapsCoreInner(TSource source, TTarget target)
        {
        }
    }
}
