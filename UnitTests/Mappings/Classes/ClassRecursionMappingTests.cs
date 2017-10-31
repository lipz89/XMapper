using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nelibur.ObjectMapper;
using Nelibur.ObjectMapper.Mappers;
using Xunit;

namespace UnitTests.Mappings.Classes
{
    public sealed class ClassRecursionMappingTests
    {
        //public static void Main()
        //{
        //    var test = new ClassRecursionMappingTests();
        //    test.Map_Hierarchy_Success();
        //}

        [Fact]
        public void Map_Hierarchy_Success()
        {
            TinyMapper.Config(cfg =>
            {
                cfg.NameMatching((x, y) =>
                {
                    if (x + "Model" == y)
                        return true;
                    if (y + "Model" == x)
                        return true;
                    if (x.Replace("Models", "s") == y)
                        return true;
                    if (y.Replace("Models", "s") == x)
                        return true;

                    return false;
                });
            });


            TinyMapper.Bind<Source, Target>();
            TinyMapper.Bind<Target, Source>();
            TinyMapper.Bind<SourceItem, TargetItem>(cfg => cfg.Ignore(x => x.StringModel));
            TinyMapper.Bind<TargetItem, SourceItem>();
            //TinyMapper.Bind<SourceItem, TargetItem>(cfg =>
            //{
            //    cfg.Bind(x => x.String, x => x.String2);
            //    cfg.Bind(x => x.String2, x => x.String);
            //});
            //TinyMapper.Bind<TargetItem, SourceItem>(cfg =>
            //{
            //    cfg.Bind(x => x.String, x => x.String2);
            //    cfg.Bind(x => x.String2, x => x.String);
            //});
            TinyMapper.Save();
            var source = new Source
            {
                Id = 1,
                AuditStatus = 1,
                String = "tiny",
            };

            var item = new SourceItem() { String = "item", String2 = "value", Parent = source };
            var item2 = new SourceItem() { String = "item2", String2 = "value2", Parent = source };

            source.Items.Add(item);
            source.Items.Add(item2);

            source.Item = item;


            var actual = TinyMapper.Map<Target>(source);

            var raw = TinyMapper.Map<Source>(actual);

            Assert.Equal(source.Id, actual.Id);
            Assert.Equal(source.String, actual.String);
            Assert.Equal(source.AuditStatus, actual.AuditStatus);
        }


        public class Source : SourceBase
        {
            private ICollection<SourceItem> items;
            public byte? AuditStatus { get; set; }

            public string String { get; set; }
            public SourceItem Item { get; set; }
            public ICollection<SourceItem> Items
            {
                get { return items ?? (items = new List<SourceItem>()); }
                protected set { items = value; }
            }
        }
        public class SourceItem
        {
            public string String { get; set; }
            public string String2 { get; set; }
            public Source Parent { get; set; }
        }


        public abstract class SourceBase
        {
            public int Id { get; set; }
        }


        public abstract class TargetBase
        {
            public int Id { get; set; }
        }


        public sealed class Target : TargetBase
        {
            public byte? AuditStatus { get; set; }
            public string String { get; set; }
            public TargetItem Item { get; set; }
            public IList<TargetItem> ItemModels { get; set; }
        }
        public sealed class TargetItem
        {
            public string StringModel { get; set; }
            public string String2Model { get; set; }
            public Target ParentModel { get; set; }
        }
    }
}
