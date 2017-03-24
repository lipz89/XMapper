using System;

using XMapper.Common;

namespace XMapper.Core
{
    internal sealed class GuidMapper<TSource, TTarget> : BaseMapper<TSource, TTarget>
    {
        static GuidMapper()
        {
            Inner<Guid, byte[]>.Mapper = g => g.ToByteArray();
            Inner<Guid?, byte[]>.Mapper = g => g?.ToByteArray();
            Inner<byte[], Guid>.Mapper = bs => new Guid(bs);
            Inner<byte[], Guid?>.Mapper = bs => bs == null ? (Guid?)null : new Guid(bs);
        }

        public override TTarget Map(TSource source, TTarget target)
        {
            return MapCore(source);
        }
        private TTarget MapCore(TSource source)
        {
            var mapper = Inner<TSource, TTarget>.Mapper;
            if (mapper != null)
            {
                return mapper(source);
            }

            throw new Exception(string.Format("没有提供映射程序 {0} --> {1}",
                                              sourceType.ObtainOriginalName(),
                                              targetType.ObtainOriginalName()));
        }

        private class Inner<TS, TR>
        {
            public static Func<TS, TR> Mapper { get; set; }
        }
    }
}