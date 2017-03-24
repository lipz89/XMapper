using System;

using XMapper.Common;

namespace XMapper.Core
{
    internal abstract class ClassMapper<TSource, TTarget> : BaseMapper<TSource, TTarget>
    {
        protected virtual TTarget CreateTargetInstance()
        {
            throw new NotImplementedException();
        }

        protected virtual TTarget MapClass(TSource source, TTarget target)
        {
            throw new NotImplementedException();
        }

        public override TTarget Map(TSource source, TTarget target)
        {
            if (target.IsNull())
            {
                target = CreateTargetInstance();
            }
            TTarget result = MapClass(source, target);
            return result;
        }
    }
}