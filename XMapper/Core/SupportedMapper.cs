using System;
using System.ComponentModel;

using XMapper.Common;

namespace XMapper.Core
{
    internal sealed class SupportedMapper<TSource, TTarget> : BaseMapper<TSource, TTarget>
    {
        private Func<TSource, TTarget> mapper;
        internal bool IsCanConvert()
        {
            TypeConverter fromConverter = TypeDescriptor.GetConverter(realSourceType);
            if (fromConverter.CanConvertTo(realTargetType))
            {
                mapper = x => (TTarget)fromConverter.ConvertTo(x, realTargetType);
                return true;
            }

            TypeConverter toConverter = TypeDescriptor.GetConverter(realTargetType);
            if (toConverter.CanConvertFrom(realSourceType))
            {
                mapper = x => (TTarget)toConverter.ConvertFrom(x);
                return true;
            }

            return false;
        }
        private TTarget MapCore(TSource source)
        {
            if (mapper != null)
            {
                return mapper(source);
            }
            //TypeConverter fromConverter = TypeDescriptor.GetConverter(realSourceType);
            //if (fromConverter.CanConvertTo(realTargetType))
            //{
            //    return (TTarget)fromConverter.ConvertTo(source, realTargetType);
            //}

            //TypeConverter toConverter = TypeDescriptor.GetConverter(realTargetType);
            //if (toConverter.CanConvertFrom(realSourceType))
            //{
            //    return (TTarget)toConverter.ConvertFrom(source);
            //}

            throw new Exception(string.Format("没有提供映射程序 {0} --> {1}",
                                              sourceType.ObtainOriginalName(),
                                              targetType.ObtainOriginalName()));
        }

        public override TTarget Map(TSource source, TTarget target)
        {
            return MapCore(source);
        }
    }
}
