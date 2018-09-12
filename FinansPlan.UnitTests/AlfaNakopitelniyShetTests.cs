using System;
using NUnit.Framework;
using NSubstitute;
using FinansPlan;
using System.Linq;

namespace FinansPlan.UnitTests
{
    [TestFixture]
    public class AlfaNakopitelniyShetTests
    {
        AlfaNakopitelniyShet alfaNakopit;

        [Test]
        public void GetTotal_InitSum_AddProcents()
        {
            App.PlanHorizont = DateTime.Parse("1.01.2002");
            IProcenter procenter= Substitute.For<IProcenter>();
            procenter.GetProcentSum(Arg.Any<DateTime>(), Arg.Any<DateTime>()
                ,Arg.Any<double>()).Returns(aa=>(double)aa[2]/100/12);
            //procenter.GetProcentSum(NSubstitute.Arg.Any<DateTime>(), NSubstitute.Arg.Any<DateTime>(), 4).Returns(0.04 / 12);

            double sum = 100000;
            alfaNakopit = new AlfaNakopitelniyShet(DateTime.Parse("1.01.2001"), sum);
            alfaNakopit.procenter = procenter;
            alfaNakopit.Recalc();

            Assert.That(alfaNakopit.GetTotal(DateTime.Parse("1.02.2001"), true), Is.EqualTo(sum));
            sum += sum * 0.07 / 12;
            Assert.That(alfaNakopit.GetTotal(DateTime.Parse("1.02.2001"), false), Is.EqualTo(sum));

            sum += sum * 0.07 / 12;
            Assert.That(alfaNakopit.GetTotal(DateTime.Parse("1.03.2001"), false), Is.EqualTo(sum));
            sum += sum * 0.07 / 12;
            Assert.That(alfaNakopit.GetTotal(DateTime.Parse("1.04.2001"), false), Is.EqualTo(sum));

            sum += sum * 0.04 / 12;
            Assert.That(alfaNakopit.GetTotal(DateTime.Parse("1.05.2001"), false), Is.EqualTo(sum));
        }

    }
}
