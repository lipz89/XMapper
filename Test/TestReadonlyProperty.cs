using System.Collections.Generic;

namespace Test
{
    public class TestReadonlyProperty
    {
        public void Test()
        {
            XMapper.Mapper.Init(cfg =>
            {
                cfg.Map<Order, OrderModel>().Bind(x => x.OrderDetailModels, x => x.OrderDetails);
                cfg.Map<OrderModel, Order>().Bind(x => x.OrderDetails, x => x.OrderDetailModels);
                cfg.Map<OrderDetail, OrderDetailModel>();
                cfg.Map<OrderDetailModel, OrderDetail>();
            });


            var order = new OrderModel
            {
                ID = 1,
                OrderNo = "Order1",
                OrderDetailModels = new List<OrderDetailModel>
                {
                    new OrderDetailModel {ID = 1, OrderNo = "Order1", ProductName = "钢笔", UnitPrice = 3, Count = 10}, new OrderDetailModel {ID = 2, OrderNo = "Order1", ProductName = "橡皮", UnitPrice = 1, Count = 100}
                }
            };


            var model = XMapper.Mapper.Map<OrderModel, Order>(order);

            var raw = XMapper.Mapper.Map<Order, OrderModel>(model);
        }
    }

    public class Order
    {
        public int ID { get; set; }
        public string OrderNo { get; set; }
        /// <summary>
        /// 一对多关联订单明细（延迟加载用接口访问方式）
        /// </summary>      
        private ICollection<OrderDetail> details;
        public virtual ICollection<OrderDetail> OrderDetails
        {
            get { return details ?? (details = new List<OrderDetail>()); }
            //protected set { details = value; }
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
        public ICollection<OrderDetailModel> OrderDetailModels { get; set; }
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