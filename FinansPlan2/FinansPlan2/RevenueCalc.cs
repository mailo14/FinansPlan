using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class RevenueCalc
    {
        public decimal Calc(List<RevenueDiap> diaps)
        {
            var planedDiaps = PlaneDiaps(diaps);
            //HashSet<DateTime> dats=
            var percentage =CombineDiaps(planedDiaps)*100;
          /*  var totalSum = planedDiaps.Sum(pp => pp.InputSum);
            var totalDays = planedDiaps.Sum(pp => pp.Days);//TODO last end -first start
            
            var percentage = planedDiaps.Sum(pp => pp.Days / totalDays * pp.InputSum / totalSum * pp.Percentage / 100)*100;*/
            return percentage;
        }

        public List<(DateTime startDat, DateTime endDat)> SplitDiaps(List<(DateTime startDat, DateTime endDat)> diaps)
        {
            var startDat = diaps.Min(pp => pp.startDat);
            var endDat = diaps.Max(pp => pp.endDat);

            var starts = diaps.Select(pp => pp.startDat).ToList();
            var ends = diaps.Select(pp => pp.endDat).ToList();

            var extraStarts = new List<DateTime>();
            var extraEnds = new List<DateTime>();            
            foreach (var d in diaps)
            {
                 extraStarts.AddRange(ends.Where(pp => pp >= d.startDat && pp <= d.endDat && pp!=endDat).Select(pp => pp.AddDays(1)));
                extraEnds.AddRange(starts.Where(pp => pp >= d.startDat && pp <= d.endDat && pp!=startDat).Select(pp => pp.AddDays(-1)));
            }

             starts= starts.Union(extraStarts).OrderBy(pp=>pp).ToList();
             ends = ends.Union(extraEnds).OrderBy(pp => pp).ToList();

            if (starts.Count != ends.Count)
                throw new Exception("split diaps ends/starts not equal");

            var outDiaps = new List<(DateTime, DateTime)>();
            var lastEnd = startDat.AddDays(-1); //to detect blank gaps
            for (int i = 0; i < starts.Count; i++)
            {
                var start = starts[i];
                var end = ends[i];
                if (start > lastEnd.AddDays(1))
                {
                    outDiaps.Add((lastEnd.AddDays(1), start.AddDays(-1))); //blank diap
                }
                outDiaps.Add((start,end));
                lastEnd = end;
            }

            return outDiaps;
        }

        /// <summary>
        /// Схлопнуть пересекающиеся диапазоны в плоский набор последовательных диапазонов. Добавить пустые диапазоны если нужно
        /// </summary>
        public List<RevenueDiap> PlaneDiaps(List<RevenueDiap> diaps)
        {
            List<RevenueDiap> outDiaps = new List<RevenueDiap>();
            var datDiaps = SplitDiaps(diaps.Select(pp => (pp.StartDat, pp.EndDat)).ToList());
            foreach (var d in datDiaps)
            {
                var newDiap = new RevenueDiap { StartDat = d.startDat, EndDat = d.endDat, InputSum = 0, OutputSum = 0 };

                var items = diaps.Where(pp => d.startDat >= pp.StartDat && d.startDat <= pp.EndDat).ToList();
                if (items.Any())
                    foreach (var i in items)
                    {
                        var dayPercent = (i.OutputSum - i.InputSum) / (i.Days + 1);
                        var outPutShare =i.InputSum+ dayPercent*(newDiap.Days + 1);
                        newDiap.InputSum += i.InputSum;
                        newDiap.OutputSum += outPutShare;// i.OutputSum;
                    }

                outDiaps.Add(newDiap);
            }

            return outDiaps;
        }

        public decimal CombineDiaps(List<RevenueDiap> diaps)
        {
            var outDiap = new RevenueDiap();
            
            var totalDaysX = diaps.Sum(pp => pp.Days + 1);
            var totalInputSums = diaps.Sum(pp => pp.InputSum);

            decimal commonProcent = 0;
            foreach(var d in diaps)
            {
                var daysX = d.Days + 1;
                var dayProcentX = (d.InputSum > 0) 
                    ? (d.OutputSum - d.InputSum) / d.InputSum / daysX 
                    : 0;
                var share = (d.InputSum / totalInputSums) * ((decimal)daysX / totalDaysX) * dayProcentX;
                commonProcent += share;
            }
         //   commonProcent *= diaps.Count;
            commonProcent = commonProcent * totalDaysX / (totalDaysX - 1); //привести по аналогии с вкладом
            return commonProcent;
        }
        public RevenueDiap MergeDiaps(List<RevenueDiap> diaps)
        {
            if (diaps.Count == 1)
                return diaps[0];

            var startDat = diaps[0].StartDat;
            var endDat = diaps[0].EndDat;
            if (diaps.Any(pp=>pp.StartDat!=startDat || pp.EndDat!=endDat)) throw new Exception("diaps borders not same");

            /*var totalSum = diaps.Sum(pp => pp.InputSum);
            var totalDays = (int)(endDat - startDat).TotalDays;
            var percentage = diaps.Sum(pp => pp.InputSum * pp.Percentage / 100) * 100;
            percentage = percentage / totalSum;*/
            return new RevenueDiap { StartDat=startDat,EndDat=endDat,InputSum= diaps.Sum(pp => pp.InputSum) ,OutputSum= diaps.Sum(pp => pp.OutputSum) };
        }



    }

    public class RevenueDiap
    {
        public DateTime StartDat;
        public DateTime EndDat;
        public int Days { get { return (int)(EndDat - StartDat).TotalDays; } }
        public decimal InputSum;
        public decimal OutputSum;
        //public decimal SumDiff { get { return OutputSum - InputSum; } }
        public decimal Percentage
        {
            get
            {
                return (OutputSum - InputSum) / InputSum
                    / Days// * 365
                    * 100;
            }
        }
    }
}