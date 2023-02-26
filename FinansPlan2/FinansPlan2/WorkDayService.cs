using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public interface IWorkDayService
    {
        DateTime GetWorkDayOrBefore(DateTime d);
        DateTime GetWorkDayOrAfter(DateTime d);
        int GetWorkDaysBetween(DateTime start, DateTime end);
    }
    public class WorkDayService : IWorkDayService
    {
        private readonly IWorkDayProvider _workDayProvider;
        public WorkDayService(IWorkDayProvider workDayProvider)
        {
            _workDayProvider = workDayProvider;
        }

        public DateTime GetWorkDayOrBefore(DateTime d)
        {
            var ret = d.Date;
            while (!_workDayProvider.IsWorkDay(ret))
                ret = ret.AddDays(-1);

            return ret;
        }

        public DateTime GetWorkDayOrAfter(DateTime d)
        {
            var ret = d.Date;
            while (!_workDayProvider.IsWorkDay(ret))
                ret = ret.AddDays(1);

            return ret;
        }

        public int GetWorkDaysBetween(DateTime start, DateTime end)
        {
            int ret = 0;
            for (var d = start; d <= end; d = d.AddDays(1))
                if (_workDayProvider.IsWorkDay(d))
                    ret++;

            return ret;
        }
    }

    public interface IWorkDayProvider
    {
        bool IsWorkDay(DateTime d);
    }

    public class WorkDayProvider : IWorkDayProvider
    {
        private HashSet<int> _loadedYears = new HashSet<int>();
        private HashSet<DateTime> _NonWorkingDates = new HashSet<DateTime>();

        public bool IsWorkDay(DateTime d)
        {
            if (!_loadedYears.Contains(d.Year))
                LoadYear(d.Year);

            return !_NonWorkingDates.Contains(d.Date);
        }

        private void LoadYear(int year)
        {
            //using (var httpClient = new HttpClient()){ var json = await httpClient.GetStringAsync("url");
            using (var webClient = new System.Net.WebClient())
            {
                var json = webClient.DownloadString($@"http://xmlcalendar.ru/data/ru/{year}/calendar.json");
                var results = JObject.Parse(json)["months"].Children();

                foreach (var result in results)
                {
                    var month = (int)result["month"];
                    var days = ((string)result["days"]).Split(new char[] { '*', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var day in days)
                        _NonWorkingDates.Add(new DateTime(year, month, int.Parse(day)));
                }

                _loadedYears.Add(year);
            }
        }
    }
}
