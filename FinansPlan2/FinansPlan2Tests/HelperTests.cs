using NUnit.Framework;
using FinansPlan2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.Tests
{
    [TestFixture()]
    public class HelperTests
    {
        [Test()]
        public void GetFactSellsTest()
        {
            var helper = new Helper();
            var osts = new List<CountPricePair> { new CountPricePair(50, 100) };
            var actual=helper.GetFactSells(osts, 10);
            Assert.AreEqual(40, osts[0].Count);
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(10, actual[0].Count); Assert.AreEqual(100, actual[0].Price);
        }

        [Test()]
        public void GetFactSellsTest2()
        {
            var helper = new Helper();
            var osts = new List<CountPricePair> {new CountPricePair(50, 100), new CountPricePair(50, 80)};
            
            var actual = helper.GetFactSells(osts, 10);
            Assert.AreEqual(40, osts[0].Count); Assert.AreEqual(50, osts[1].Count);
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(10, actual[0].Count); Assert.AreEqual(100, actual[0].Price);

            var actual2 =helper.GetFactSells(osts, 60);
            Assert.AreEqual(30, osts[0].Count); Assert.AreEqual(1, osts.Count);
            Assert.AreEqual(2, actual2.Count);
            Assert.AreEqual(40, actual2[0].Count); Assert.AreEqual(100, actual2[0].Price);
            Assert.AreEqual(20, actual2[1].Count); Assert.AreEqual(80, actual2[1].Price);
        }
    }
}