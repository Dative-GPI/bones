using System.Collections.Generic;
using System.Linq;
using Bones;
using Xunit;

namespace UnitTest
{
    public class TestExpressionBuilder
    {
        private List<int> l0 = new List<int>
        {
            0,1,2,3,4,5,6,7,8,9
        };

        private List<int> l1 = new List<int>()
        {
            0,2,4,6,8
        };

        private List<int> l2 = new List<int>()
        {
            0,3,6,9
        };

        private List<int> common1 = new List<int>()
        {
            0,6
        };

        private class Obj
        {
            public int Val;
            public bool Prop;

            public override bool Equals(object obj)
            {
                if (obj is Obj)
                {
                    var o = obj as Obj;
                    return this.Val == o.Val && this.Prop == o.Prop;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
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

        private List<Obj> common2 = new List<Obj>()
        {
            new Obj {Val = 0, Prop = true},
            new Obj {Val = 1, Prop = false},
            new Obj {Val = 5, Prop = false},
            new Obj {Val = 6, Prop = true},
            new Obj {Val = 7, Prop = false},
        };

        [Fact]
        public void In_IsIdentityWithSameInput_1()
        {
            var result = l0.Where(ExpressionExtensions.In<int, int>(l0, e => e).Compile());
            Assert.Equal(l0, result);
        }

        [Fact]
        public void In_FindCommonElements_1()
        {
            var result = l1.Where(ExpressionExtensions.In<int, int>(l2, e => e).Compile());
            Assert.Equal(common1, result);
        }

        [Fact]
        public void In_FindCommonElements_2()
        {
            var result = l2.Where(ExpressionExtensions.In<int, int>(l1, e => e).Compile());
            Assert.Equal(common1, result);
        }

        [Fact]
        public void In_IsIdentityWithSameInput_2()
        {
            var result = l3.Where(ExpressionExtensions.In<Obj, int>(l0, e => e.Val).Compile());
            Assert.Equal(l3, result);
        }

        [Fact]
        public void In_FindCommonElements_3()
        {
            var result = l3.Where(ExpressionExtensions.In<Obj, int>(l1, e => e.Val).Compile());
            Assert.Equal(l3.Where(o => o.Prop), result);
        }

        [Fact]
        public void In_FindCommonElements_4()
        {
            var result = l4.Where(ExpressionExtensions.In<Obj, int>(l2, e => e.Val).Compile());
            Assert.Equal(l4.Where(o => o.Prop), result);
        }

        [Fact]
        public void Or_BehaveLikeInWithEquality()
        {
            var result = l3.Where(ExpressionExtensions.Or<Obj, int>(l1, (e, i) => e.Val == i).Compile());
            var expected = l3.Where(ExpressionExtensions.In<Obj, int>(l1, e => e.Val).Compile());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Or_FilterAccordingToPredicate_1()
        {
            var result = l3.Where(ExpressionExtensions.Or<Obj, Obj>(l4, (e1, e2) => e1.Val == e2.Val).Compile());
            Assert.Equal(l3, result);
        }

        [Fact]
        public void Or_FilterAccordingToPredicate_2()
        {
            var result = l4.Where(ExpressionExtensions.Or<Obj, Obj>(l3, (e1, e2) => e1.Val == e2.Val && e1.Prop == e2.Prop).Compile());
            Assert.Equal(common2, result);
        }
    }
}