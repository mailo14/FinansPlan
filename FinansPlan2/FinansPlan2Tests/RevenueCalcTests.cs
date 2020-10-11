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
    public class RevenueCalcTests
    {
        [Test()]
        public void CalcTest()
        {
            var c = new RevenueCalc();
            //var ret=c.Calc(new List<RevenueDiap> { new RevenueDiap { StartDat = DateTime.Parse("1.01.2001"), EndDat = DateTime.Parse("31.12.2001"), InputSum = 100, OutputSum = 110 } });
            var ret = c.Calc(new List<RevenueDiap> { new RevenueDiap { StartDat = DateTime.Parse("16.06.2019"), EndDat = DateTime.Parse("15.06.2020"), InputSum = 100, OutputSum = 110 } });
            //var days=(int)(EndDat - StartDat).TotalDays
            Assert.AreEqual(10, (double)ret * 365, 0.001);
            //Assert.AreEqual(10, ret * 365);
        }
        [Test()]
        [TestCase(100, 110, 100, 110,10)]
        [TestCase(100, 100, 100, 110, 5)]
        [TestCase(100, 100, 100, 90, -5)]
        [TestCase(100, 90, 100, 110, 0)]
        public void CalcMultipleDiapsTest(decimal input1, decimal output1, decimal input2, decimal output2,decimal expected)
        {
            var c = new RevenueCalc();
            var actual = c.Calc(new List<RevenueDiap> {
                new RevenueDiap { StartDat = DateTime.Parse("01.01"), EndDat = DateTime.Parse("5.01"), InputSum = input1, OutputSum = output1 },
                new RevenueDiap { StartDat = DateTime.Parse("06.01"), EndDat = DateTime.Parse("10.01"), InputSum = input2, OutputSum = output2 },
            });
            
            Assert.AreEqual((double)expected, (double)actual * 9, 0.001);
        }

        [Test()]
        public void SplitDiapsTest()
        {

            var c = new RevenueCalc();
            var ret = c.SplitDiaps(new List<(DateTime startDat, DateTime endDat)> { (DateTime.Parse("1.01"), DateTime.Parse("2.01")) });
            Assert.AreEqual(1, ret.Count);

            ret = c.SplitDiaps(new List<(DateTime startDat, DateTime endDat)> {
     (DateTime.Parse("1.01"), DateTime.Parse("2.01")),
     (DateTime.Parse("3.01"), DateTime.Parse("4.01")) });
            Assert.AreEqual(2, ret.Count);

            ret = c.SplitDiaps(new List<(DateTime startDat, DateTime endDat)> {
     (DateTime.Parse("1.01"), DateTime.Parse("1.01")),
     (DateTime.Parse("3.01"), DateTime.Parse("4.01")) });
            Assert.AreEqual(3, ret.Count);

            ret = c.SplitDiaps(new List<(DateTime startDat, DateTime endDat)> {
     (DateTime.Parse("1.01"), DateTime.Parse("2.01")),
     (DateTime.Parse("2.01"), DateTime.Parse("4.01")) });
            Assert.AreEqual(3, ret.Count);

            ret = c.SplitDiaps(new List<(DateTime startDat, DateTime endDat)> {
     (DateTime.Parse("1.01"), DateTime.Parse("2.01")),
     (DateTime.Parse("1.01"), DateTime.Parse("4.01")) });
            Assert.AreEqual(2, ret.Count);
        }

        [Test()]
        public void PlaneDiapsTest()
        {
            var c = new RevenueCalc();
            var ret = c.PlaneDiaps(new List<RevenueDiap> {
                new RevenueDiap { StartDat = DateTime.Parse("01.01"), EndDat = DateTime.Parse("03.01"), InputSum = 100, OutputSum = 130 },
                new RevenueDiap { StartDat = DateTime.Parse("01.01"), EndDat = DateTime.Parse("02.01"), InputSum = 100, OutputSum = 120 },
            });

            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(200, ret[0].InputSum); Assert.AreEqual(240, ret[0].OutputSum);
            Assert.AreEqual(100, ret[1].InputSum); Assert.AreEqual(110, ret[1].OutputSum);
        }
    }
}