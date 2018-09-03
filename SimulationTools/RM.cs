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
                    Value = RobustnessMeasures.FS(S);
                    break;
                case "BFS":
                    Value = RobustnessMeasures.BFS(S, _GAMMA);
                    break;
                case "UFS":
                    Value = RobustnessMeasures.UFS(S, _GAMMA);
                    break;
                case "wFS":
                    Value = RobustnessMeasures.wFS(S);
                    break;
                case "TS":
                    Value = RobustnessMeasures.TS(S);
                    break;
                case "BTS":
                    Value = RobustnessMeasures.BTS(S, _GAMMA);
                    break;
                case "UTS":
                    Value = RobustnessMeasures.UTS(S, _GAMMA);
                    break;
                case "wTS":
                    Value = RobustnessMeasures.wTS(S);
                    break;
                case "SDR":
                    Value = RobustnessMeasures.SDR(S);
                    break;
                default:
                    throw new Exception("RM name not recognized");
                    break;


            }

        }
    }
}
