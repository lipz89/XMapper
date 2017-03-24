using System;

namespace XMapper.Common
{
    internal static class Error
    {
        public static Exception ArgumentNullException(string name)
        {
            return new ArgumentNullException(name);
        }

        public static Exception ArgumentException(string message, string name)
        {
            return new ArgumentException(message, name);
        }

        public static Exception Exception(string message)
        {
            return new Exception(message);
        }

    }
}
