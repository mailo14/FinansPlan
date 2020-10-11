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
    public class TimerTests
    {
        Timer timer;

        MOEX birja;
        [SetUp]
        public void Init()
        {
            var o = new Oblig()
            {InstrCode="obl1",
                StartDat = DateTime.Parse("11.04.2017"),
                EndDat = DateTime.Parse("05.04.2022"),
                Period = 182
            };
            o.PlanKupons = new DatedValueCollection<decimal>(new List<DatedValue<decimal>> {
            new DatedValue<decimal>("10.10.2017", 55.10M),
            new DatedValue<decimal>("09.04.2019", 55.60M),
            new DatedValue<decimal>("06.10.2020", 51.61M),
        });
            o.Prices = new DatedValueCollection<decimal>(new List<DatedValue<decimal>> {
                new DatedValue<decimal>("1.08.10",1000)
            });
             birja = new MOEX();
            birja.Obligs.Add(o);

            timer = new Timer();
var brockerAccState = new BrockerAccState() {Birja=birja, Dat = DateTime.Parse("1.08.10"), RubSum = 100000, States = new List<ObligState>() };
            timer.BrockerAccStates.Add(brockerAccState);
        }

        [Test()]
        [TestCase("11.04.2017", 0)]
        [TestCase("12.04.2017", 0.3)]
        [TestCase("09.04.2019", 0)]
        [TestCase("17.05.2020", 11.34)]
        public void ProcessEventTest(string d,decimal nkd)
        {
            timer.ProcessEvent(new HistEvent() { Dat = DateTime.Parse(d), InstrCode = "obl1", Type = EventType.Buy, Count = 10 });

            var expectedPrice = Math.Round(1000m + nkd, 2);
            expectedPrice += Math.Round(expectedPrice*birja.Commission / 100, 2);
            var expectedOst =  100000m - expectedPrice * 10;

            Assert.AreEqual(2,timer.BrockerAccStates.Count);
            Assert.AreEqual(expectedOst, timer.BrockerAccStates.Last().RubSum);
            Assert.AreEqual(10, timer.BrockerAccStates.Last().States.Last().Count);
        }
    }
}