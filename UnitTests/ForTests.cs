using System.Collections.Generic;
using Nelibur.ObjectMapper;
using UnitTests.Mappings.Classes;
using Xunit;

namespace UnitTests
{
    public sealed class ForTests
    {
        //public static void Main()
        //{
        //    TinyMapper.Bind<Source, Target>(cfg =>
        //    {
        //        cfg.Bind(x => x.FullName, x => x.FirstName + x.LastName);
        //    });
        //    TinyMapper.Bind<Target, Source>();

        //    TinyMapper.Save();

        //    var source=new Source()
        //    {
        //        FirstName = "Jim",
        //        LastName = "Green"
        //    };

        //    var target = TinyMapper.Map<Target>(source);
        //    var source2 = TinyMapper.Map<Source>(target);
        //}
        

        public class Source
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }



        public sealed class Target
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get; set; }
        }
    }
}
