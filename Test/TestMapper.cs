﻿using System;
using System.Collections.Generic;
using System.Linq;

using Nelibur.ObjectMapper;

using Test.Common;

using Xunit;

namespace Test
{
    public class TestMapper
    {

        [Fact]
        public void TestXMapper()
        {
            string n = "___";
            XMapper.Mapper.Init(cfg =>
            {
                cfg.Map<Simple, SimpleModel>()
                    .Bind(x => x.NickName, x => x.FirstName)
                    .Bind(x => x.FullName, x => x.FirstName + "." + x.Name + n);
                cfg.Map<SimpleModel, Simple>()
                    .Bind(x => x.FirstName, x => x.NickName);

                cfg.Map<Item, ItemModel>();
                cfg.Map<ItemModel, Item>();
            });

            var simle = new Simple
            {
                Name = "1213",
                ID = 256,
                FirstName = "FirstName",
                LastName = "LastName",
                Item = new Item
                {
                    Name = "Item",
                    ID = 5
                },
                Items = new List<Item>
                {
                    new Item
                    {
                        Name = "Item1",
                        ID = 1
                    }
                }
            };

            //simle.Item.Parent = simle;

            var maps = XMapper.Mapper.Config.List;

            var model = XMapper.Mapper.Map<Simple, SimpleModel>(simle);
            var raw = XMapper.Mapper.Map<SimpleModel, Simple>(model);
        }

        [Fact]
        public void TestList()
        {
            InitConfig();
            var list = new MyList<Simple> { Index = 4, NextIndex = 7 };
            list.AddRange(CreateData(5));

            List<SimpleModel> result = XMapper.Mapper.Map<MyList<Simple>, List<SimpleModel>>(list);
            List<SimpleModel> result2 = XMapper.Mapper.Map<MyList<Simple>, MyList<SimpleModel>>(list);

            var l = XMapper.Mapper.Map<List<SimpleModel>, List<Simple>>(result2);
        }

        [Fact]
        public void Test()
        {
            CodeTimer.Initialize();

            //TestSimple();
            Func<Simple, SimpleModel, SimpleModel> map = (s, r) =>
            {
                r = r ?? new SimpleModel();
                r.Name = s.Name;
                r.ID = s.ID;
                r.LastName = s.LastName;
                r.NickName = s.FirstName;
                r.Items = s.Items.Select(x => new ItemModel() { Name = x.Name, ID = x.ID }).ToList();
                return r;
            };
            Func<SimpleModel, Simple, Simple> map2 = (s, r) =>
            {
                r = r ?? new Simple();
                r.Name = s.Name;
                r.ID = s.ID;
                r.LastName = s.LastName;
                r.FirstName = s.NickName;
                r.Items = ((List<ItemModel>)s.Items).Select(x => new Item() { Name = x.Name, ID = x.ID }).ToList();
                return r;
            };

            var count = 100;

            var source = CreateData(100);

            CodeTimer.Time("AutoMapper", count, () =>
            {
                foreach (var simple in source)
                {
                    TestUse<Simple, SimpleModel>(simple, AutoMapper.Mapper.Map, AutoMapper.Mapper.Map);
                }
            });

            CodeTimer.Time("XMapper", count, () =>
            {
                foreach (var simple in source)
                {
                    TestUse<Simple, SimpleModel>(simple, XMapper.Mapper.Map, XMapper.Mapper.Map);
                }
            });

            //CodeTimer.Time("TinyMapper", count, () =>
            //{
            //    foreach (var simple in source)
            //    {
            //        TestUse<Simple, SimpleModel>(simple, TinyMapper.Map, TinyMapper.Map);
            //    }
            //});

            CodeTimer.Time("CodeMapper", count, () =>
            {
                foreach (var simple in source)
                {
                    TestUse<Simple, SimpleModel>(simple, map, map2);
                }
            });

            Console.Read();
        }
        [Fact]
        public void Test2()
        {
            CodeTimer.Initialize();

            //TestSimple();
            Func<Simple2, SimpleModel2, SimpleModel2> map = (s, r) =>
            {
                r = r ?? new SimpleModel2();
                r.Name = s.Name;
                r.ID = s.ID.ToCharArray();
                r.LastName = s.LastName;
                r.NickName = s.FirstName;
                return r;
            };
            Func<SimpleModel2, Simple2, Simple2> map2 = (s, r) =>
            {
                r = r ?? new Simple2();
                r.Name = s.Name;
                r.ID = s.ID.ToString();
                r.LastName = s.LastName;
                r.FirstName = s.NickName;
                return r;
            };

            var count = 100;

            var source = CreateData2(100);

            CodeTimer.Time("AutoMapper", count, () =>
            {
                foreach (var simple in source)
                {
                    TestUse<Simple2, SimpleModel2>(simple, AutoMapper.Mapper.Map, AutoMapper.Mapper.Map);
                }
            });

            CodeTimer.Time("XMapper", count, () =>
            {
                foreach (var simple in source)
                {
                    TestUse<Simple2, SimpleModel2>(simple, XMapper.Mapper.Map, XMapper.Mapper.Map);
                }
            });

            //CodeTimer.Time("TinyMapper", count, () =>
            //{
            //    foreach (var simple in source)
            //    {
            //        TestUse<Simple, SimpleModel>(simple, TinyMapper.Map, TinyMapper.Map);
            //    }
            //});

            CodeTimer.Time("CodeMapper", count, () =>
            {
                foreach (var simple in source)
                {
                    TestUse<Simple2, SimpleModel2>(simple, map, map2);
                }
            });

            Console.Read();
        }

        public void InitConfig()
        {
            AutoMapper.Mapper.Initialize(cfg =>
                                         {
                                             cfg.CreateMap<Simple, SimpleModel>()
                                                .ForMember(x => x.NickName, x => x.MapFrom(y => y.FirstName))
                                                .ForMember(x => x.FullName, x => x.MapFrom(y => y.FirstName + "." + y.Name));
                                             cfg.CreateMap<SimpleModel, Simple>()
                                                .ForMember(x => x.FirstName, x => x.MapFrom(y => y.NickName));

                                             cfg.CreateMap<Simple2, SimpleModel2>()
                                               .ForMember(x => x.ID, x => x.MapFrom(y => y.ID.ToCharArray()))
                                               .ForMember(x => x.NickName, x => x.MapFrom(y => y.FirstName))
                                               .ForMember(x => x.FullName, x => x.MapFrom(y => y.FirstName + "." + y.Name));
                                             cfg.CreateMap<SimpleModel2, Simple2>()
                                               .ForMember(x => x.ID, x => x.MapFrom(y => string.Join("",y)))
                                                .ForMember(x => x.FirstName, x => x.MapFrom(y => y.NickName));

                                             cfg.CreateMap<Item, ItemModel>();
                                             cfg.CreateMap<ItemModel, Item>();
                                         });

            //TinyMapper.Bind<Simple, SimpleModel>(cfg =>
            //                                     {
            //                                         cfg.Bind(x => x.FirstName, x => x.NickName);
            //                                     });
            //TinyMapper.Bind<SimpleModel, Simple>(cfg =>
            //                                     {
            //                                         cfg.Bind(x => x.NickName, x => x.FirstName);
            //                                     });

            //TinyMapper.Bind<Item, ItemModel>();
            //TinyMapper.Bind<ItemModel, Item>();

            XMapper.Mapper.Init(cfg =>
                                {
                                    cfg.Map<string, char[]>(x => x.ToCharArray());
                                    cfg.Map<char[],string>(x=>string.Join("",x));

                                    cfg.Map<Simple, SimpleModel>()
                                       .Bind(x => x.NickName, x => x.FirstName)
                                       .Bind(x => x.FullName, x => x.FirstName + "." + x.Name);
                                    cfg.Map<SimpleModel, Simple>()
                                       .Bind(x => x.FirstName, x => x.NickName);

                                    cfg.Map<Simple2, SimpleModel2>()
                                       .Bind(x => x.NickName, x => x.FirstName)
                                       .Bind(x => x.FullName, x => x.FirstName + "." + x.Name);
                                    cfg.Map<SimpleModel2, Simple2>()
                                       .Bind(x => x.FirstName, x => x.NickName);

                                    cfg.Map<Item, ItemModel>();
                                    cfg.Map<ItemModel, Item>();
                                });
        }

        private List<Simple> CreateData(int times = 1000)
        {
            var list = new List<Simple>();
            for (int i = 0; i < times; i++)
            {
                list.Add(new Simple()
                {
                    Name = "Name" + i,
                    ID = i,
                    FirstName = "FirstName" + i,
                    LastName = "LastName" + i,
                    Items = new List<Item>
                             {
                                 new Item()
                                 {
                                     Name = i.ToString() + i,
                                     ID = i,
                                 },
                                 new Item()
                                 {
                                     Name = i.ToString() + i * 2,
                                     ID = i + 1
                                 }
                             }
                });
            }
            return list;
        }

        private List<Simple2> CreateData2(int times = 1000)
        {
            var list = new List<Simple2>();
            for (int i = 0; i < times; i++)
            {
                list.Add(new Simple2()
                {
                    Name = "Name" + i,
                    ID = Guid.NewGuid().ToString(),
                    FirstName = "FirstName" + i,
                    LastName = "LastName" + i,
                });
            }
            return list;
        }

        private void TestUse<TSource, TResult>(TSource source, Func<TSource, TResult, TResult> map, Func<TResult, TSource, TSource> map2)
            where TSource : class, new()
            where TResult : class, new()
        {
            TResult result = null;
            result = map(source, result);

            TSource result2 = null;
            result2 = map2(result, result2);
        }
    }

    public class MyList<T> : List<T>
    {
        public int Index { get; set; }
        public int NextIndex { get; set; }
    }

    public class Item
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Simple Parent { get; set; }
    }

    public class ItemModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public SimpleModel Parent { get; set; }
    }


    public class Simple
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public Item Item { get; set; }
        public List<Item> Items { get; set; }
    }

    public class SimpleModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string FullName { get; set; }
        public ItemModel Item { get; set; }
        public List<ItemModel> Items { get; set; }
    }



    public class Simple2
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
    }

    public class SimpleModel2
    {
        public char[] ID { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string FullName { get; set; }
    }
}