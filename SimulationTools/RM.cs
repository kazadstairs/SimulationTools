using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class RM
    {
        public string Name;
        public double Value;

        public RM(string Descr)
        {
            Name = Descr;
            Value = Constants.DEFAULT_RM;
        }
        public RM(string Descr, double Val)
        {
            Name = Descr;
            Value = Val;
        }

        public void Calculate(Schedule S)
        {
            double _GAMMA = 0.25;
            switch (Name)
            {
                case "FS":
                    Value = RobustnessMeasures.SoFS(S);
                    break;
                case "BFS":
                    Value = RobustnessMeasures.SoBFS(S, _GAMMA);
                    break;
                case "UFS":
                    Value = RobustnessMeasures.SoUFS(S, _GAMMA);
                    break;
                case "wFS":
                    Value = RobustnessMeasures.SowFS(S);
                    break;
                case "TS":
                    Value = RobustnessMeasures.SoTS(S);
                    break;
                case "BTS":
                    Value = RobustnessMeasures.SoBTS(S, _GAMMA);
                    break;
                case "UTS":
                    Value = RobustnessMeasures.SoUTS(S, _GAMMA);
                    break;
                case "wTS":
                    Value = RobustnessMeasures.SowTS(S);
                    break;
                case "SDR":
                    Value = RobustnessMeasures.SoSDR(S);
                    break;
                default:
                    throw new Exception("RM name not recognized");
                    break;


            }
            Console.WriteLine("CALCULATING MEANS instead of sums!");
            Value = Value / S.PrecedenceDAG.N;

        }
    }
}
