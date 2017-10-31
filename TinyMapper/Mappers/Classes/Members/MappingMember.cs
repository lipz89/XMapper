using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Nelibur.ObjectMapper.Core.DataStructures;
using Nelibur.ObjectMapper.Core.Extensions;

namespace Nelibur.ObjectMapper.Mappers.Classes.Members
{
    public sealed class MappingMember
    {
        internal MappingMember(MemberInfo target, MemberInfo source)
            : this(target, source, new TypePair(source.GetMemberType(), target.GetMemberType()))
        {
        }
        internal MappingMember(MemberInfo target)
        {
            Target = target;
            Ignored = true;
        }

        internal MappingMember(MemberInfo target, MemberInfo source, TypePair typePair)
        {
            Source = source;
            Target = target;
            TypePair = typePair;
            IsMemberMapping = true;
        }
        internal MappingMember(MemberInfo target, Expression expression)
        {
            Target = target;
            TypePair = new TypePair(target.GetMemberType(), ((LambdaExpression)expression).Body.Type);
            IsExpressionMapping = true;
        }

        public bool Ignored { get; private set; }
        public bool IsMemberMapping { get; private set; }
        public bool IsExpressionMapping { get; private set; }
        public MemberInfo Source { get; private set; }
        public MemberInfo Target { get; private set; }
        internal TypePair TypePair { get; private set; }

        public override string ToString()
        {
            if (Ignored)
            {
                return Target.Name + " IsIgnored";
            }
            else if (IsMemberMapping)
            {
                return Source.Name + " to " + Target.Name;
            }
            else
            {
                return "(lambda) to " + Target.Name;
            }
        }
    }
}
