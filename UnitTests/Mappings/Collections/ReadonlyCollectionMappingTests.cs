using System;
using System.Collections.Generic;
using Nelibur.ObjectMapper;
using Xunit;

namespace UnitTests.Mappings.Collections
{
    public sealed class ReadonlyCollectionMappingTests
    {
        //public static void Main()
        //{
        //    var test = new ReadonlyCollectionMappingTests();
        //    test.Map_Collections_Success();
        //}
        [Fact]
        public void Map_Collections_Success()
        {
            TinyMapper.Bind<OrderDetail, OrderDetailModel>();
            TinyMapper.Bind<OrderDetailModel, OrderDetail>();
            TinyMapper.Bind<OrderModel, Order>(cfg => cfg.Bind(x => x.OrderDetails, x => x.OrderDetailModels));
            TinyMapper.Bind<Order, OrderModel>(cfg => cfg.Bind(x => x.OrderDetailModels, x => x.OrderDetails));
            TinyMapper.Save();

            var order = new OrderModel
            {
                ID = 1,
                OrderNo = "Order1",
                OrderDetailModels = new List<OrderDetailModel>
                {
                    new OrderDetailModel {
                        ID = 1,
                        OrderNo = "Order1",
                        ProductName = "钢笔",
                        UnitPrice = 3,
                        Count = 10
                    },
                    new OrderDetailModel {
                        ID = 2,
                        OrderNo = "Order1",
                        ProductName = "橡皮",
                        UnitPrice = 1,
                        Count = 100
                    }
                }
            };
            var actual = TinyMapper.Map<OrderModel, Order>(order);

            var m2 = TinyMapper.Map<Order, OrderModel>(actual);

            order.OrderNo += "22";

            var actual2 = TinyMapper.Map<OrderModel, Order>(order);
        }

        public class Order
        {
            public int ID { get; set; }
            public string OrderNo { get; set; }
            private IList<OrderDetail> details;
            public virtual IList<OrderDetail> OrderDetails
            {
                get { return details ?? (details = new List<OrderDetail>()); }
                protected set { details = value; }
            }
        }

        public class OrderDetail
        {
            public int ID { get; set; }
            public string OrderNo { get; set; }
            public string ProductName { get; set; }
            public decimal UnitPrice { get; set; }
            public int Count { get; set; }
        }


        public class OrderModel
        {
            public int ID { get; set; }
            public string OrderNo { get; set; }
            public IList<OrderDetailModel> OrderDetailModels { get; set; }
        }

        public class OrderDetailModel
        {
            public int ID { get; set; }
            public string OrderNo { get; set; }
            public string ProductName { get; set; }
            public decimal UnitPrice { get; set; }
            public int Count { get; set; }
        }
    }
}
