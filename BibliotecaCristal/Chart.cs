using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaCristal
{
    public class PruebaChart
    {
        public double timeChart { get; set; }
        public double casillasT { get; set; }
        public double casillasP { get; set; }

    }

    public class ViewModel
    {
        public List<PruebaChart> Data { get; set; }
        public ViewModel()
        {
            Data = new List<PruebaChart>()
            {
                new PruebaChart {timeChart=1,casillasT=0,casillasP=0},
            };
        }
        public ViewModel(List<PruebaChart> oldList)
        {
            Data = oldList;
        }

        public void AddNew(double T, double P)
        {
            this.Data.Add(new PruebaChart { timeChart = Data.Count() + 1, casillasT = T, casillasP = P });
        }
    }
}
