using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;

namespace PD.AnalysisModel
{
    public class CurveFittingResultModel
    {
        public virtual double Max_X { get; set; } = 10;
        public virtual double Min_X { get; set; } = 0;

        public List<double> BestCoeffs { get; set; } = new List<double>();

        public virtual double Best_X { get; set; } = 0;
        public virtual double Best_Y { get; set; } = 0;
        public virtual bool isCurfitSuccess { get; set; } = true;
        public virtual double DrawLine_Gap_Seperation { get; set; } = 100;

        List<DataPoint> curfitResults = new List<DataPoint>();


        public CurveFittingResultModel(List<DataPoint> list_datapoint)
        {
            list_datapoint.OrderBy(o => o.X).ToList();
            Max_X = list_datapoint.Last().X;
            Min_X = list_datapoint.First().X;
        }

        public List<DataPoint> GetDrawLinePoints()
        {
            // 繪圖- CurveFit曲線
            double x_gap = (Max_X - Min_X) / DrawLine_Gap_Seperation;

            for (double x = Min_X; x <= Max_X; x = x + x_gap)
            {
                curfitResults.Add(new DataPoint(x, (BestCoeffs[2] * Math.Pow(x, 2) + BestCoeffs[1] * x + BestCoeffs[0])));
            }

            return curfitResults;
        }
    }
}
