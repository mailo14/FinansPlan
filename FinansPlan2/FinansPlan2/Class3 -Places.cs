using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.New
{
    public class Atm
    {
        //public string Name { get; set; }
        public  Place Place { get; set; }
       public  Banks Bank { get; set; }

        /// <summary>
        /// С приемом наличных
        /// </summary>
       public  bool CanInput{ get; set; }

        /// <summary>
        /// Выдача максимум 40тыс за раз
        /// </summary>
       public  bool Max40{ get; set; }

        /// <summary>
        /// Прием купюр штучно (не пачкой)
        /// </summary>
       public  bool InputByEd{ get; set; }

    }

    public enum Place
    {
        Dom,
        UbrirMarks30,
        AlfaMarks51,
        TcAmsterdam,
        SovkomMarks13,
        TcGorskii,
        VtbVatutina21,
        VtbGeodez,
    }
    public static class AtmPlaceInfo
    {
        public static List<Atm> Atms { get; set; } = new List<Atm> {
            new Atm { Bank=Banks.Ubrir,Place=Place.UbrirMarks30,CanInput=true, Max40 =true},

            new Atm {Bank=Banks.Alfa,Place=Place.AlfaMarks51,CanInput=true},
            new Atm { Bank=Banks.Alfa,Place=Place.TcAmsterdam,CanInput=true},
            new Atm { Bank=Banks.Alfa,Place=Place.TcGorskii,CanInput=true},

            new Atm { Bank=Banks.Tinkof,Place=Place.TcAmsterdam,CanInput=true},

            new Atm { Bank=Banks.Sovcom,Place=Place.SovkomMarks13,CanInput=true,InputByEd=true},

            new Atm { Bank=Banks.Vtb,Place=Place.VtbGeodez,CanInput=true},
            new Atm { Bank=Banks.Vtb,Place=Place.VtbVatutina21,CanInput=true},
        };

        public static List<PlaceDist> PlaceDists = new List<PlaceDist>
        {
            new PlaceDist{Place1=Place.Dom,Place2=Place.UbrirMarks30,Dist=7},
            new PlaceDist{Place1=Place.Dom,Place2=Place.TcGorskii,Dist=6},

            new PlaceDist{Place1=Place.UbrirMarks30,Place2=Place.TcAmsterdam,Dist=7},
            new PlaceDist{Place1=Place.UbrirMarks30,Place2=Place.AlfaMarks51,Dist=3},

            new PlaceDist{Place1=Place.AlfaMarks51,Place2=Place.TcAmsterdam,Dist=6},

            new PlaceDist{Place1=Place.TcAmsterdam,Place2=Place.VtbGeodez,Dist=4},
            new PlaceDist{Place1=Place.TcAmsterdam,Place2=Place.SovkomMarks13,Dist=9},
            new PlaceDist{Place1=Place.TcAmsterdam,Place2=Place.TcGorskii,Dist=10},

            new PlaceDist{Place1=Place.SovkomMarks13,Place2=Place.VtbVatutina21,Dist=4},
        };
        //private static Lazy<decimal[,]> _someVariable = new Lazy<decimal[,]>(GetShortDists);

        private static List<Place> Places = Enum.GetValues(typeof(Place)).Cast<Place>().ToList();
        public static decimal[,] AllShortDists => new Lazy<decimal[,]>(GetShortDists).Value;

        private static decimal[,] GetShortDists()
        {
            var n = Places.Count;
            var d = new decimal[n, n];
            var INF = decimal.MaxValue;
            for (int k = 0; k < n; k++)
                for (int i = 0; i < n; i++)
                {
                    d[i, k] =i==k?0: INF;
                }
            foreach (var pd in PlaceDists)
            {
                var i = Places.IndexOf(pd.Place1); var j = Places.IndexOf(pd.Place2);
                d[i, j] = pd.Dist; d[j, i] = pd.Dist;
            }


            for (int k = 0; k < n; ++k)
                for (int i = 0; i < n; ++i)
                    for (int j = 0; j < n; ++j)
                        if (d[i, k] < INF && d[k, j] < INF)
                            d[i, j] = Math.Min(d[i, j], d[i, k] + d[k, j]);
            return d;
        }
        public static decimal GetDist(Place place1, Place place2)
        {
            return AllShortDists[Places.IndexOf(place1), Places.IndexOf(place2)];
        }

        public static List<PlaceWeighted> GetPlacesToOperateCash(Banks bank, bool isSnyat, decimal sum)
        {
            var banks = new List<Banks> { bank };

            if (!isSnyat)
            {
                return (from a in Atms
                        where banks.Contains(a.Bank) && a.CanInput
                        group a by a.Place into gr
                        let existsNoInputByEd = gr.Any(x => !x.InputByEd)
                        select new PlaceWeighted
                        {
                            Place = gr.Key,
                            OpTime = existsNoInputByEd ? 0 : (sum <= 50000 ? 5 : 10),
                            Atms = existsNoInputByEd ? gr.Where(x => !x.InputByEd).ToList() : gr.ToList()
                        }).ToList();
            }
            else {
                if (bank == Banks.Vtb) banks = Enum.GetValues(typeof(Banks)).Cast<Banks>().ToList();

                return (from a in Atms
                        where banks.Contains(a.Bank) 
                        group a by a.Place into gr
                        let existsMax40 = gr.Any(x => !x.Max40)
                        select new PlaceWeighted
                        {
                            Place = gr.Key,
                            OpTime = existsMax40 ? 0 : (sum <= 40000 ? 5 : 10),
                            Atms = existsMax40 ? gr.Where(x => !x.Max40).ToList() : gr.ToList()
                        }).ToList();
        }
        }
        /*public Dictionary<Place, List<Atm>> AtmPlaced { get; set; }=new Dictionary<Place, List<Atm>> {
   { Place.UbrirMarks30,new List<Atm> { new Atm { Name="Убрир на Маркса",Bank=Banks.Ubrir,Max40=true,} } },
   { Place.AlfaMarks51,new List<Atm> { new Atm { Name="Убрир на Маркса"} } },
}*/
    }

    public class PlaceDist
    {
        public Place Place1 { get; internal set; }
        public Place Place2 { get; internal set; }
        public decimal Dist { get; internal set; }
    }


    public class PlaceWeighted
    {
        public Place Place { get; set; }
        public List<Atm> Atms { get; set; }
        /// <summary>
        /// Трудоемкость самой операции на месте
        /// </summary>
        public decimal OpTime { get; set; }
    }
}
