using System;
using System.Linq.Expressions;

namespace Nelibur.ObjectMapper.Bindings
{
    public interface IBindingConfig<TSource, TTarget>
    {
        void Bind(Expression<Func<TTarget, object>> target, Expression<Func<TSource, object>> source);
        //void Bind<TField>(Expression<Func<TTarget, TField>> target, TField value); not working yet
        //void Bind(Expression<Func<TTarget, object>> target, Type targetType);
        void Ignore(Expression<Func<TTarget, object>> expression);
    }
}
