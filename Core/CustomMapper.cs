using System;

namespace XMapper.Core
{
    internal sealed class CustomMapper<TSource, TTarget> : BaseMapper<TSource, TTarget>
    {
        internal static Func<TSource, TTarget> Mapper { get; set; }

        public override TTarget Map(TSource source, TTarget target)
        {
            return Mapper(source);
        }
    }
}