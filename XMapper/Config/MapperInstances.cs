using System.Collections.Generic;

namespace XMapper.Config
{
    internal class MapperInstances<T> : List<T>
    {
        private MapperInstances() { }

        private static MapperInstances<T> instance;

        internal static MapperInstances<T> Instance
        {
            get { return instance ?? (instance = new MapperInstances<T>()); }
        }
    }
}
