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
        double delta; // percentage that start or finish can be delayed by to still be called on time
        int StartOnTimeJobs; // Jobs that start before (1+delta) * their start time (i.e., on time)
        int FinishDelayedJobs; // Jobs that start after (1+delta) * their start time;
        double NJobs;


       // double StartPunctuality; // percentage jobs that start on scheduled time.
       // double FinishPunctuality; // percentage jobs that finish before their LSS.

        bool DEBUGMODE;

        //
        //Statistics of each individual job (completion times etc)
        //

        public SimulationPerformanceMeasures(int id, double PunctualityAllowance, int Njobs)
        {
            RunID = id;
            TotalLinearStartDelay = 0;
            delta = PunctualityAllowance;
            StartOnTimeJobs = 0;
            FinishDelayedJobs = 0;
            DEBUGMODE = true;
            NJobs = (double)Njobs;

        }

        public void WriteToFile(string path)
        {
            using (StreamWriter sw = File.AppendText(path))
            {
                if (DEBUGMODE) { sw.WriteLine("id {0,-6}; Cmax {1,-18}; Delay Sum {2,-18}; Start Pun {3,-18}; Finish Pun {4,-18}", RunID, Cmax, TotalLinearStartDelay, (double)StartOnTimeJobs / NJobs, (double)FinishDelayedJobs / NJobs); }
                else
                {
                    sw.WriteLine("{0}; {1}; {2}; {3}; {4}", RunID, Cmax, TotalLinearStartDelay, (double)StartOnTimeJobs / NJobs, (double)FinishDelayedJobs / NJobs);
                }
            }
        }

        public void AddLinearStartDelay(double ScheduledStart, double RealizedStart)
        {
            if (ScheduledStart > RealizedStart) { throw new Exception("Job started before scheduled time"); }
            TotalLinearStartDelay += RealizedStart - ScheduledStart;
        }

        public void UpdateStartPunctuality(double ScheduledStart, double RealizedStart)
        {
            if (RealizedStart <= (1 + delta) * ScheduledStart)
            {
                StartOnTimeJobs++;
                //Console.WriteLine("On time");
            }
            else { Console.WriteLine("Delayed"); }

        }


        public void UpdateFinishPunctuality(double ScheduledFinish, double RealizedFinish)
        {
            throw new System.NotImplementedException("todo: be precice in how you define finish time punctuality");
            if (RealizedFinish > (1 + delta) * ScheduledFinish) { FinishDelayedJobs++; }
        }

    }
}
