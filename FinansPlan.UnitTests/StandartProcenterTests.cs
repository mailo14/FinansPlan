using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan.UnitTests
{
    [TestFixture]
    class StandartProcenterTests
    {
        [Test]
        public void StandartProcenter_FullMonth_CorrectReturn()
        {
            var p = new StandartProcenter();
            DateTime d1 = DateTime.Parse("1.01.2018"), d2=DateTime.Parse("31.01.2018");
            Assert.That(p.GetProcentSum(d1, d2, 7),
                Is.EqualTo(0.07 *(d2-d1).Days/365).Within(0.001));
        }

        [Test]
        public void StandartProcenter_MidMonth_CorrectReturn()
        {
            var p = new StandartProcenter();
            DateTime d1 = DateTime.Parse("31.01.2018"), d2=DateTime.Parse("2.02.2018");
            Assert.That(p.GetProcentSum(d1, d2, 7),
                Is.EqualTo(0.07 *2/365 ).Within(0.001));
        }
        [Test]
        public void StandartProcenter_MidYear_CorrectReturn()
        {
            var p = new StandartProcenter();
            DateTime d1 = DateTime.Parse("31.12.2004"), d2=DateTime.Parse("3.01.2005");//2004-IsLeapYear             
            Assert.That(p.GetProcentSum(d1, d2, 7),
                Is.EqualTo(0.07 *(1.0/366+2.0/365)).Within(0.001));
        }

    }
}
