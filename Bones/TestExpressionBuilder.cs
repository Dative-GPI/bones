using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bones
{
    public class TestExpressionBuilder
    {
        private List<int> l1 = new List<int>()
        {
            0,2,4,6,8
        };

        private List<int> l2 = new List<int>()
        {
            0,3,6,9
        };

        private List<int> common = new List<int>()
        {
            0,6
        };

        private class Obj
        {
            public int Val;
            public bool Prop;
        }

        private List<Obj> l3 = new List<Obj>()
        {
            new Obj {Val = 0, Prop = true},
            new Obj {Val = 1, Prop = false},
            new Obj {Val = 2, Prop = true},
            new Obj {Val = 3, Prop = false},
            new Obj {Val = 4, Prop = true},
            new Obj {Val = 5, Prop = false},
            new Obj {Val = 6, Prop = true},
            new Obj {Val = 7, Prop = false},
            new Obj {Val = 8, Prop = true},
            new Obj {Val = 9, Prop = false},
        };

        private List<Obj> l4 = new List<Obj>()
        {
            new Obj {Val = 0, Prop = true},
            new Obj {Val = 1, Prop = false},
            new Obj {Val = 2, Prop = false},
            new Obj {Val = 3, Prop = true},
            new Obj {Val = 4, Prop = false},
            new Obj {Val = 5, Prop = false},
            new Obj {Val = 6, Prop = true},
            new Obj {Val = 7, Prop = false},
            new Obj {Val = 8, Prop = false},
            new Obj {Val = 9, Prop = true},
        };

        [Fact]
        public void In_IsIdentityWithSameInput()
        {
            var result = l1.Where(ExpressionBuilder.In<int, int>(l1, e => e).Compile());
            Assert.Equal(l1, result);
        }

        [Fact]
        public void In_FindCommonElement_1()
        {
            var result = l1.Where(ExpressionBuilder.In<int, int>(l2, e => e).Compile());
            Assert.Equal(common, result);
        }

        [Fact]
        public void In_FindCommonElement_2()
        {
            var result = l2.Where(ExpressionBuilder.In<int, int>(l1, e => e).Compile());
            Assert.Equal(common, result);
        }
    }
}