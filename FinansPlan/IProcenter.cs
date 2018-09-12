using System;
using System.Collections.Generic;

namespace FinansPlan
{
    public interface IProcenter
    {
        double GetProcentSum(DateTime datFrom, DateTime datTo, double procent);
    }
    public class StandartProcenter:IProcenter
    {
        public double GetProcentSum(DateTime datFrom,DateTime datTo,double procent) {
            if (datTo.Year==datFrom.Year)
            {
                double dayprocent = procent / 100 / (DateTime.IsLeapYear(datTo.Year) ? 366 : 365);
                int days = (datTo - datFrom).Days;
                return dayprocent* days;
            }
            else
            {
                List<DateTime> dats = new List<DateTime>() { datFrom };
                var dat = new DateTime(datFrom.Year , 1, 1);
                do
                {
                    dats.Add(dat);
                    dat = dat.AddYears(1);
                }
                while (dat < datTo);
                dats.Add(datTo);

                double procents = 0;
                for(int i=1;i<dats.Count;i++)
                {
                    double dayprocent = procent / 100 / (DateTime.IsLeapYear(dats[i-1].Year) ? 366 : 365);
                    int days = (dats[i] - dats[i-1]).Days;
                    procents+= dayprocent * days;
                }
                return  procents;
            }            
        }
    }
}