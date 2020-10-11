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
    public class ObligTests
    {
        Oblig o;

        [SetUp]
        public void Init()
        {
            o = new Oblig()
            {
                StartDat = DateTime.Parse("11.04.2017"),
                EndDat = DateTime.Parse("05.04.2022"),
                Period = 182
            };
            o.PlanKupons = new DatedValueCollection<decimal>(new List<DatedValue<decimal>> {
            new DatedValue<decimal>("10.10.2017", 55.10M),
            new DatedValue<decimal>("09.04.2019", 55.60M),
            new DatedValue<decimal>("06.10.2020", 51.61M),
        });
        }

        [Test()]
        [TestCase("11.04.2017", 0)]
        [TestCase("12.04.2017", 0.3)]
        [TestCase("09.04.2019", 0)]
        [TestCase("17.05.2020", 11.34)]
        public void GetNKDTest(string d, decimal expected)
        {
            var dat = DateTime.Parse(d);
            var actual = o.GetNKD(dat);

            Assert.AreEqual(expected, actual);
        }
    }
}