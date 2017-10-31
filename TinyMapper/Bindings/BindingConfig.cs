using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Nelibur.ObjectMapper.Core.DataStructures;
using Nelibur.ObjectMapper.Core.Extensions;

namespace Nelibur.ObjectMapper.Bindings
{
    internal class BindingConfig
    {
        private readonly Dictionary<string, string> _bindFields = new Dictionary<string, string>();
        private readonly Dictionary<string, Expression> _bindExpressions = new Dictionary<string, Expression>();
        //private readonly Dictionary<string, Type> _bindTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, Func<object, object>> _customTypeConverters = new Dictionary<string, Func<object, object>>();
        private readonly HashSet<string> _ignoreFields = new HashSet<string>();

        internal void BindConverter(string targetName, Func<object, object> func)
        {
            _customTypeConverters[targetName] = func;
        }

        internal void BindFields(string targetName, string sourceName)
        {
            _bindFields[targetName] = sourceName;
        }

        internal void BindExpression(string targetName, Expression expression)
        {
            _bindExpressions[targetName] = expression;
        }

        //internal void BindType(string targetName, Type value)
        //{
        //    _bindTypes[targetName] = value;
        //}

        internal Option<string> GetBindField(string targetName)
        {
            string result;
            bool exsist = _bindFields.TryGetValue(targetName, out result);
            return new Option<string>(result, exsist);
        }

        internal Option<Expression> GetBindExpression(string targetName)
        {
            Expression result;
            bool exsist = _bindExpressions.TryGetValue(targetName, out result);
            return new Option<Expression>(result, exsist);
        }

        //internal Option<Type> GetBindType(string targetName)
        //{
        //    Type result;
        //    bool exsist = _bindTypes.TryGetValue(targetName, out result);
        //    return new Option<Type>(result, exsist);
        //}

        internal Option<Func<object, object>> GetCustomTypeConverter(string targetName)
        {
            return _customTypeConverters.GetValue(targetName);
        }

        internal bool HasCustomTypeConverter(string targetName)
        {
            return _customTypeConverters.ContainsKey(targetName);
        }

        internal void IgnoreTargetField(string targetName)
        {
            _ignoreFields.Add(targetName);
        }

        internal bool IsIgnoreTargetField(string targetName)
        {
            if (string.IsNullOrEmpty(targetName))
            {
                return true;
            }
            return _ignoreFields.Contains(targetName);
        }
    }
}
