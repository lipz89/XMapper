using System;

using XMapper;
using XMapper.Core;

using Xunit;

namespace Test
{
    public class TestXMapper
    {
        public TestXMapper()
        {
            Mapper.Init(x => { });
        }

        private void TestExcetionOrNot(Type exType, Action action)
        {
            if (exType == null)
            {
                action();
            }
            else
            {
                Assert.Throws(exType, action);
            }
        }


        //[InlineData(5, (byte)5)]
        //[InlineData(265, (byte)0, typeof(OverflowException))]
        //[Theory]
        //public void TestGMapper<T1, T2>(T1 a, T2 b, Type exType = null)
        //{
        //    TestExcetionOrNot(exType, () =>
        //    {
        //        var result = MapperRoute.Map<T1, T2>(a);
        //        Assert.Equal(result, b);
        //    });
        //}

        [InlineData(5, 5)]
        [InlineData(265, 0, typeof(OverflowException))]
        [Theory]
        public void TestInt2ByteMapper(int a, byte b, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<int, byte>(a);
                Assert.Equal(result, b);
            });
        }

        [InlineData(5, 5)]
        [InlineData(265, 0, typeof(OverflowException))]
        [InlineData(null, 0, typeof(InvalidCastException))]
        [Theory]
        public void TestNInt2ByteMapper(int? a, byte b, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<int?, byte>(a);
                Assert.Equal(result, b);
            });
        }

        [InlineData(5, (byte)5)]
        [InlineData(265, (byte)0, typeof(OverflowException))]
        [InlineData(null, null)]
        [Theory]
        public void TestNInt2NByteMapper(int? a, byte? b, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<int?, byte?>(a);
                Assert.Equal(result, b);
            });
        }

        [InlineData(5, (byte)5)]
        [InlineData(265, (byte)0, typeof(OverflowException))]
        [Theory]
        public void TestInt2NByteMapper(int a, byte? b, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<int, byte?>(a);
                Assert.Equal(result, b);
            });
        }

        [InlineData(E1.A, E2.A)]
        [InlineData(E1.D, E2.A, typeof(OverflowException))]
        [InlineData(E1.C, E2.A, typeof(ArgumentException))]
        [Theory]
        public void TestEnumMapper(E1 e1, E2 e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<E1, E2>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(E1.A, "A")]
        [InlineData(E1.D, "D")]
        [Theory]
        public void TestEnum2StringMapper(E1 e1, string e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<E1, string>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData("B", E1.B)]
        [InlineData("1", E1.B)]
        [InlineData("10", E1.B, typeof(ArgumentException))]
        [InlineData("E", E1.B, typeof(ArgumentException))]
        [Theory]
        public void TestString2EnumMapper(string e1, E1 e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<string, E1>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(1, E1.B)]
        [InlineData(63, E1.C)]
        [InlineData(-1, E1.B, typeof(ArgumentException))]
        [Theory]
        public void TestInt2EnumMapper(int e1, E1 e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<int, E1>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(null, null)]
        [InlineData(E1.A, E2.A)]
        [InlineData(E1.D, E2.A, typeof(OverflowException))]
        [InlineData(E1.C, E2.A, typeof(ArgumentException))]
        [Theory]
        public void TestNEnumNMapper(E1? e1, E2? e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<E1?, E2?>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(E1.A, E2.A)]
        [InlineData(E1.D, E2.A, typeof(OverflowException))]
        [InlineData(E1.C, E2.A, typeof(ArgumentException))]
        [Theory]
        public void TestEnumNMapper(E1 e1, E2? e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<E1, E2?>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(null, null, typeof(InvalidCastException))]
        [InlineData(E1.A, E2.A)]
        [InlineData(E1.D, E2.A, typeof(OverflowException))]
        [InlineData(E1.C, E2.A, typeof(ArgumentException))]
        [Theory]
        public void TestNEnumMapper(E1? e1, E2 e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<E1?, E2>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(null, null)]
        [InlineData(E1.A, "A")]
        [InlineData(E1.D, "D")]
        [Theory]
        public void TestNEnum2StringMapper(E1? e1, string e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<E1?, string>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(null, null)]
        [InlineData("B", E1.B)]
        [InlineData("1", E1.B)]
        [InlineData("10", E1.B, typeof(ArgumentException))]
        [InlineData("E", E1.B, typeof(ArgumentException))]
        [Theory]
        public void TestString2NEnumMapper(string e1, E1? e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<string, E1?>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(1, E1.B)]
        [InlineData(63, E1.C)]
        [InlineData(-1, E1.B, typeof(ArgumentException))]
        [Theory]
        public void TestInt2NEnumMapper(int e1, E1? e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<int, E1?>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(1, E1.B)]
        [InlineData(63, E1.C)]
        [InlineData(-1, E1.B, typeof(ArgumentException))]
        [Theory]
        public void TestNInt2NEnumMapper(int e1, E1? e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<int, E1?>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(null, null)]
        [InlineData(E1.A, 0)]
        [Theory]
        public void TestNEnum2NIntMapper(E1? e1, int? e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<E1?, int?>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(5, "5")]
        [Theory]
        public void TestInt2StringMapper(int e1, string e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<int, string>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(5, "5")]
        [InlineData(null, null)]
        [Theory]
        public void TestNInt2StringMapper(int? e1, string e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<int?, string>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData("5", 5)]
        [Theory]
        public void TestString2IntMapper(string e1, int e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<string, int>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData("5", 5)]
        [InlineData(null, null)]
        [Theory]
        public void TestString2NIntMapper(string e1, int? e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<string, int?>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData("5", (byte)5)]
        [InlineData("256", (byte)0, typeof(Exception))]
        [InlineData(null, (byte)0, typeof(InvalidCastException))]
        [Theory]
        public void TestString2ByteMapper(string e1, byte e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<string, byte>(e1);
                Assert.Equal(result, e2);
            });
        }
        [InlineData(null, null)]
        [InlineData("5", (byte)5)]
        [InlineData("256", (byte)0, typeof(Exception))]
        [Theory]
        public void TestString2NByteMapper(string e1, byte? e2, Type exType = null)
        {
            TestExcetionOrNot(exType, () =>
            {
                var result = MapperRoute.Map<string, byte?>(e1);
                Assert.Equal(result, e2);
            });
        }

        [Fact]
        public void TestGuidMapper()
        {
            TestExcetionOrNot(null, () =>
            {
                var e2 = MapperRoute.Map<Guid?, string>(null);
                Assert.Equal(e2, null);
            });
            TestExcetionOrNot(null, () =>
            {
                var g = Guid.NewGuid();
                var e2 = MapperRoute.Map<Guid?, string>(g);
                Assert.Equal(e2, g.ToString());
            });
            TestExcetionOrNot(null, () =>
            {
                var g = Guid.NewGuid();
                var e2 = MapperRoute.Map<string, Guid>(g.ToString());
                Assert.Equal(e2, g);
            });
            TestExcetionOrNot(null, () =>
            {
                var g = Guid.NewGuid();
                var e2 = MapperRoute.Map<byte[], Guid>(g.ToByteArray());
                Assert.Equal(e2, g);
            });
            TestExcetionOrNot(null, () =>
            {
                var g = Guid.NewGuid();
                var e2 = MapperRoute.Map<Guid, byte[]>(g);
                Assert.Equal(e2, g.ToByteArray());
            });
        }
    }

    public enum E1 : int
    {
        A,
        B,
        C = 63,
        D = 256
    }
    public enum E2 : byte
    {
        A,
        B,
        C,
        D = 64
    }
}
