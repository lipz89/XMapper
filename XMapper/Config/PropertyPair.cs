using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

using XMapper.Common;

namespace XMapper.Config
{
    /// <summary>
    /// 属性配置
    /// </summary>
    [DebuggerDisplay("{ResultPropertyName}")]
    public sealed class PropertyPair
    {
        /// <summary>
        /// 是否忽略该属性的映射
        /// </summary>
        public bool IsIgnore { get; internal set; }
        /// <summary>
        /// 是否使用源类型相关的表达式映射
        /// </summary>
        public bool IsUseSolver { get; internal set; }
        /// <summary>
        /// 是否在转换时使用实例赋值
        /// </summary>
        public bool IsUseInstance { get; internal set; }
        internal bool IsUseMap { get; set; }
        internal bool IsUseClassMap { get; set; }
        /// <summary>
        /// 目标属性名称
        /// </summary>
        public string ResultPropertyName { get; internal set; }
        /// <summary>
        /// 源属性名称
        /// </summary>
        public string SourcePropertyName { get; internal set; }
        /// <summary>
        /// 目标属性类型
        /// </summary>
        public Type ResultPropertyType { get; internal set; }
        /// <summary>
        /// 源属性类型
        /// </summary>
        public Type SourcePropertyType { get; internal set; }
        /// <summary>
        /// 目标属性转换时使用的与源类型相关的表达式映射
        /// </summary>
        public Expression Solver { get; internal set; }
        /// <summary>
        /// 目标属性转换时使用的实例对象
        /// </summary>
        public object Instance { get; internal set; }
        internal string InstanceString { get; set; }
        internal string SolverString { get; set; }
        internal bool IsUseExplicitConverter { get; private set; }
        internal bool IsUseSupportedConverter { get; private set; }
        internal bool IsNeedCollectionMapper { get; private set; }

        internal bool CanWrite { get; set; }

        internal void CheckConvert()
        {
            if (IsIgnore)
            {
                return;
            }
            if (ResultPropertyType == null || SourcePropertyType == null)
            {
                return;
            }
            if (ResultPropertyType.IsAssignableFrom(SourcePropertyType))
            {
                return;
            }

            //TypeConverter fromConverter = TypeDescriptor.GetConverter(SourcePropertyType);
            //if (fromConverter.CanConvertTo(ResultPropertyType))
            //{
            //    IsUseExplicitConverter = true;
            //    return;
            //}

            //TypeConverter toConverter = TypeDescriptor.GetConverter(ResultPropertyType);
            //if (toConverter.CanConvertFrom(SourcePropertyType))
            //{
            //    IsUseExplicitConverter = true;
            //    return;
            //}

            //if (SourcePropertyType.NonNullableType() == ResultPropertyType.NonNullableType())
            //{
            //    IsUseExplicitConverter = true;
            //    return;
            //}

            if (ResultPropertyType.IsSupportedType() && SourcePropertyType.IsSupportedType())
            {
                IsUseSupportedConverter = true;
                return;
            }
            //if (ResultPropertyType.IsEnumerable() && SourcePropertyType.IsEnumerable())
            //{
            //    IsNeedCollectionMapper = true;
            //}
            if (!ResultPropertyType.IsValueType)
            {
                IsUseClassMap = true;
            }
            IsUseMap = true;
        }
    }
}