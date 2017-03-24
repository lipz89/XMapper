using System;

namespace XMapper.Core
{
    internal sealed class EnumMapper<TSource, TTarget> : BaseMapper<TSource, TTarget>
    {
        public override TTarget Map(TSource source, TTarget target)
        {
            return MapCore(source);
        }

        private TTarget MapCore(TSource source)
        {
            if (realTargetType.IsEnum)
            {
                if (source is string)
                {
                    var t = (TTarget)Enum.Parse(realTargetType, source.ToString());
                    if (Enum.IsDefined(realTargetType, t))
                    {
                        return t;
                    }
                    throw new ArgumentException("未找到请求的值“" + source + "”。");
                }

                var val = Convert.ChangeType(source, Enum.GetUnderlyingType(realTargetType));
                if (Enum.IsDefined(realTargetType, val))
                {
                    return (TTarget)Enum.ToObject(realTargetType, val);
                }
                throw new ArgumentException("未找到请求的值“" + val + "”。");
            }

            return (TTarget)Convert.ChangeType(source, realTargetType);
        }
    }
}