using System;
using NUnit.Framework;
using NSubstitute;
using FinansPlan;

namespace FinansPlan.UnitTests
{
    [TestFixture]
    public class BinbankDepVelikolepnayaSemerkaTests
    {
        [SetUp]
       public void Setup()
        {
            App.PlanHorizont = DateTime.Parse("1.02.2001");
        }

        [Test]
        public void GetTotal_InitSum_AddProcents()
        {
            double sum = 100000;
            var dep = new BinbankDepVelikolepnayaSemerka(DateTime.Parse("1.01.2001"), 3, 7.3, sum, 10000);
            dep.Recalc();
            Assert.That(dep.GetTotal(DateTime.Parse("2.01.2001"), true), Is.EqualTo(sum));

            var dayprocent = 7.3 / 100 / 365;
            sum =Math.Round(sum+ sum * dayprocent,2);
            Assert.That(dep.GetTotal(DateTime.Parse("2.01.2001")), Is.EqualTo(sum));
                        

            dep.Transactions.Add(DateTime.Parse("2.01.2001"), -sum, 1, TranCat.getCash);
            dep.Recalc();

            Assert.That(dep.GetTotal(DateTime.Parse("3.01.2001")), Is.EqualTo(0));
        }

        [Test]
        public void GetTotal_OnCloseVSrok_InEndGetWithProcents()
        {
            double sum = 100000;
            var dep = new BinbankDepVelikolepnayaSemerka(DateTime.Parse("1.01.2001"), 3, 7.3, sum, 10000);
            dep.Recalc();
            
            Assert.That(dep.GetTotal(DateTime.Parse("4.01.2001")), Is.EqualTo(0), "endTran fail");
        }
        [Test]
        public void GetTotal_OnCloseNeVSrok_InEndDoesNotGetProcents()
        {
            double sum = 100000;
            var dep = new BinbankDepVelikolepnayaSemerka(DateTime.Parse("1.01.2001"), 3, 7.3, sum, 10000);
            dep.Recalc();

            dep.CloseDep(DateTime.Parse("3.01.2001"));

            Assert.That(dep.GetTotal(DateTime.Parse("3.01.2001")), Is.EqualTo(0));
        }
    }
       
}