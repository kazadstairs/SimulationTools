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
                    Console.WriteLine("CALCULATING MEANS instead of sums!");
                    Value = Value / S.PrecedenceDAG.N;
                    break;
                case "BFS":
                    Value = RobustnessMeasures.SoBFS(S, _GAMMA);
                    Console.WriteLine("CALCULATING MEANS instead of sums!");
                    Value = Value / S.PrecedenceDAG.N;
                    break;
                case "UFS":
                    Value = RobustnessMeasures.SoUFS(S, _GAMMA);
                    Console.WriteLine("CALCULATING MEANS instead of sums!");
                    Value = Value / S.PrecedenceDAG.N;
                    break;
                case "wFS":
                    Value = RobustnessMeasures.SowFS(S);
                    Console.WriteLine("CALCULATING MEANS instead of sums!");
                    Value = Value / S.PrecedenceDAG.N;
                    break;
                case "TS":
                    Value = RobustnessMeasures.SoTS(S);
                    Console.WriteLine("CALCULATING MEANS instead of sums!");
                    Value = Value / S.PrecedenceDAG.N;
                    break;
                case "BTS":
                    Value = RobustnessMeasures.SoBTS(S, _GAMMA);
                    Console.WriteLine("CALCULATING MEANS instead of sums!");
                    Value = Value / S.PrecedenceDAG.N;
                    break;
                case "UTS":
                    Value = RobustnessMeasures.SoUTS(S, _GAMMA);
                    Console.WriteLine("CALCULATING MEANS instead of sums!");
                    Value = Value / S.PrecedenceDAG.N;
                    break;
                case "wTS":
                    Value = RobustnessMeasures.SowTS(S);
                    Console.WriteLine("CALCULATING MEANS instead of sums!");
                    Value = Value / S.PrecedenceDAG.N;
                    break;
                case "SDR":
                    Value = RobustnessMeasures.SoSDR(S);
                    Console.WriteLine("CALCULATING MEANS instead of sums!");
                    Value = Value / S.PrecedenceDAG.N;
                    break;
                case "DetCmax":
                    Value = S.EstimateCmax();
                    break;
                case "NormalApproxCmax":
                    Value = RobustnessMeasures.NormalBasedEstimatedCmax(S,0.3);
                    break;
                default:
                    throw new Exception("RM name not recognized");
                    break;


            }

        }
    }
}
