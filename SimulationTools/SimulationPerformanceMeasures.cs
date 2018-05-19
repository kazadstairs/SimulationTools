using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class SimulationPerformanceMeasures
    {
        int RunID;
        //
        //Measures pertaining to the ENTIRE RUN:
        //
        public double Cmax;
        //Quality Robustness:

        // Solution Robustness
        public double TotalLinearStartDelay { get; private set; } // actual start - scheduled start
        double StartPunctuality; // percentage jobs that start on scheduled time.
        double FinishPunctuality; // percentage jobs that finish before their LSS.

        //
        //Statistics of each individual job (completion times etc)
        //

        public SimulationPerformanceMeasures(int id)
        {
            RunID = id;
            TotalLinearStartDelay = 0;

        }

        public void WriteToFile(string path)
        {
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("{0}, {1}, {2}", RunID, Cmax, TotalLinearStartDelay);
            }
        }

        public void AddLinearStartDelay(double ScheduledStart, double RealizedStart)
        {
            if (ScheduledStart > RealizedStart) { throw new Exception("Job started before scheduled time"); }
            TotalLinearStartDelay += RealizedStart - ScheduledStart;
        }
    }
}
