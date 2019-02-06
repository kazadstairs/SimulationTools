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
        }

        public void Calculate(Schedule S)
        {
            double _GAMMA = Settings.RM_UFSandBFS_fraction;
            switch (Name)
            {
                case "FS":
                    Value = RobustnessMeasures.SoFS(S);
                    if (Settings.RMs_USE_MEANS)
                    {
                        Value = Value / S.PrecedenceDAG.N;
                    }
                    break;
                case "BFS":
                    Value = RobustnessMeasures.SoBFS(S, _GAMMA);
                    if (Settings.RMs_USE_MEANS)
                    {
                    Value = Value / S.PrecedenceDAG.N;
                    }
                    break;
                case "UFS":
                    Value = RobustnessMeasures.SoUFS(S, _GAMMA);
                    if (Settings.RMs_USE_MEANS)
                    {
                    Value = Value / S.PrecedenceDAG.N;
                    }
                    break;
                case "wUFS":
                    Value = RobustnessMeasures.SowUFS(S, _GAMMA);
                    if (Settings.RMs_USE_MEANS)
                    {
                        Value = Value / S.PrecedenceDAG.N;
                    }
                    break;
                case "wFS":
                    Value = RobustnessMeasures.SowFS(S);
                    if (Settings.RMs_USE_MEANS)
                    {
                    Value = Value / S.PrecedenceDAG.N;
                    }
                    break;
                case "TS":
                    Value = RobustnessMeasures.SoTS(S);
                    if (Settings.RMs_USE_MEANS)
                    {
                    Value = Value / S.PrecedenceDAG.N;
                    }
                    break;
                case "BTS":
                    Value = RobustnessMeasures.SoBTS(S, _GAMMA);
                    if (Settings.RMs_USE_MEANS)
                    {
                    Value = Value / S.PrecedenceDAG.N;
                    }
                    break;
                case "UTS":
                    Value = RobustnessMeasures.SoUTS(S, _GAMMA);
                    if (Settings.RMs_USE_MEANS)
                    {
                        Value = Value / S.PrecedenceDAG.N;
                    }
                    break;
                case "wTS":
                    Value = RobustnessMeasures.SowTS(S);
                    if (Settings.RMs_USE_MEANS)
                    {
                        Value = Value / S.PrecedenceDAG.N;
                    }
                    break;
                case "wUTS":
                    Value = RobustnessMeasures.SowUTS(S,_GAMMA);
                    if (Settings.RMs_USE_MEANS)
                    {
                        Value = Value / S.PrecedenceDAG.N;
                    }
                    break;
                case "SDR":
                    Value = RobustnessMeasures.SoSDR(S);
                    if (Settings.RMs_USE_MEANS)
                    {
                        Value = Value / S.PrecedenceDAG.N;
                    }
                    break;
                case "DetCmax":
                    Value = S.EstimateCmax();
                    break;
                case "NormalApproxCmax":
                    Value = RobustnessMeasures.NormalBasedEstimatedCmax(S,0.3*0.3).Mean;
                    break;
                case "NormalApprox2Sigma":
                    Distribution NormalEstimation = RobustnessMeasures.NormalBasedEstimatedCmax(S, 0.3*0.3);
                    Value = NormalEstimation.Mean + 2 * Math.Sqrt(NormalEstimation.Variation);
                    break;
                default:
                    throw new Exception("RM name not recognized");
                    break;


            }

        }
    }
}
