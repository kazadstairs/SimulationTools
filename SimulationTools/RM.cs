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
            switch (Name)
            {
                case "FS":
                    Value = RobustnessMeasures.SumOfFreeSlacks(S);
                    break;
                case "BFS":
                    Value = RobustnessMeasures.BinaryFreeSlack(0.25, S);
                    break;
                case "UFS":
                    Value = RobustnessMeasures.UpperboundFreeSlack(0.25, S);
                    break;
                default:
                    throw new Exception("RM name not recognized");
                    break;


            }

        }
    }
}
