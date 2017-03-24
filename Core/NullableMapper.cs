using System;

using XMapper.Common;

namespace XMapper.Core
{
    internal sealed class NullableMapper<TSource, TTarget> : BaseMapper<TSource, TTarget>
    {
        public override TTarget Map(TSource source, TTarget target)
        {
            return MapCore(source);
        }

        private TTarget MapCore(TSource source)
        {
            return (TTarget)Convert.ChangeType(source, targetType.NonNullableType());
        }
    }
}