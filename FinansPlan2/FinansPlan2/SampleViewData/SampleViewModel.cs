using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.SampleViewData
{
    public class SampleViewModel: ViewModel
    {
        public SampleViewModel()
        {
            var start = DateTime.Parse("1.03.21");

            var cash = new CashVallet()
            {
                Name = "cash",
                Start = start,
                InitState = new CashValletState() { Amount = 1000000m, Dat = start }
            };
            var halva = new HalvaCard()
            {
                Name = "halva",
                Start = start,
                InitState = new HalvaCardState() { Amount = 177710.92m, SobstvAmount = 177710.92m, Dat = start, Procents = 0 }
            };

            Accounts = new List<IAccount> { cash, halva };

            DatElems = new List<DatElem>
            {
                new DatElem{Dat=DateTime.Parse("1.03.21"),Ops=new List<OpElem>{
                    new OpElem {OrderNum=0,Sum=100,Text="halva get cash",AccStateInfos=new List<OpDogElem> { new OpDogElem { OrderNum = 0, Value = "0 => 100" }, new OpDogElem { OrderNum = 1, Value = "15900 => 15800" } } },
                    new OpElem {OrderNum=0,Sum=100,Text="halva get cash",AccStateInfos=new List<OpDogElem> { new OpDogElem { OrderNum = 0, Value = "0 => 100" }, new OpDogElem { OrderNum = 1, Value = "15900 => 15800" } } },
                    //new OpElem {OrderNum=0,Sum=1600,Text="halva get cash2",Errors="no enough money",AccStateInfos=new List<string> {"100","15800 => -200" } }
                    new OpElem {OrderNum=0,Sum=1600,Text="halva get cash2",Errors="no enough money",AccStateInfos=new List<OpDogElem> { new OpDogElem { OrderNum = 1, Value = "15800 => -200" } } }
                } }
            };

        }
    }
    public class ViewModel
    {
        public List<IAccount> Accounts { get; set; }
        public List<DatElem> DatElems { get; set; }
    }
    public class DatElem {
        public DateTime Dat { get; set; }
        public List<OpElem> Ops{ get; set; }
    }
    public class OpElem
    {
        public decimal Sum{ get; set; }
        public string Text { get; set; }
        public int OrderNum { get; set; }
        public string Errors { get; set; }
        public List<OpDogElem> AccStateInfos { get; set; }
    }
    public class OpDogElem
    {
        public int OrderNum { get; set; }
        public string Value { get; set; }
    }
}
