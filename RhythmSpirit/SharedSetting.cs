using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShinenginePlus
{
    public static class SharedSetting
    {
        static public int PrefectCount = 0;
        static public int GreatCount = 0;
        static public int BadCount = 0;
        static public int MissCount = 0;
        static SharedSetting()
        {

        }

        static public string DoubleToRank(double mark)
        {
            if (mark < 0)
            {
                return "00.00%";
            }

            string _mark = (mark * 100d).ToString();
            if (!_mark.Contains('.'))
            {
                if (_mark.Length == 1) return "0" + _mark + ".00%";
                else return _mark + ".00%";
            }

            if (_mark.Split('.')[1].Length == 1)
            {
                if (_mark.Split('.')[0].Length == 2)
                    return _mark + "0%";
                else return "0" + _mark + "0%";
            }
            if (_mark.Split('.')[1].Length == 2)
            {
                if (_mark.Split('.')[0].Length == 2)
                    return _mark + "%";
                else return "0" + _mark + "%";
            }

            if (_mark.Split('.')[0].Length == 2)
                return _mark.Substring(0, 5)+"%";
            else return "0" + _mark.Substring(0,4) + "%";
        }
        static public string GetRank()
        {
            int p = SharedSetting.PrefectCount,  g = SharedSetting.GreatCount,  b = SharedSetting.BadCount,  m = SharedSetting.MissCount;
            //20,10,5,-5
            double mark_all = (p + g + b + m) * 20d;
            double nb_mark = (p * 20d) + (g * 10d) + (b * 5d) - (m * 5d);

            if (mark_all == 0) return "100.00% SSS";

            double mark_rate = nb_mark / mark_all;

            if (mark_rate == 1) return "100.00% SSS";
            else if (mark_rate < 1 && mark_rate >= 0.97d) return DoubleToRank(mark_rate) + "  SS+";
            else if (mark_rate < 0.97d && mark_rate >= 0.93d) return DoubleToRank(mark_rate) + "  SS";
            else if (mark_rate < 0.93d && mark_rate >= 0.88d) return DoubleToRank(mark_rate) + "  S+";
            else if (mark_rate < 0.88d && mark_rate >= 0.83d) return DoubleToRank(mark_rate) + "  S";
            else if (mark_rate < 0.83d && mark_rate >= 0.75d) return DoubleToRank(mark_rate) + "  A+";
            else if (mark_rate < 0.75d && mark_rate >= 0.70d) return DoubleToRank(mark_rate) + "  A";
            else if (mark_rate < 0.70d && mark_rate >= 0.60d) return DoubleToRank(mark_rate) + "  B";
            else if (mark_rate < 0.60d && mark_rate >= 0.50d) return DoubleToRank(mark_rate) + "  C";
            else return DoubleToRank(mark_rate) + "  D";
        }
    }
}
