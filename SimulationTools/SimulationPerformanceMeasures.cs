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
        int FinishOnTimeJobs; // Jobs that complete before (1+delta) * successor start times
        double NJobs;
        Simulation Sim;


       // double StartPunctuality; // percentage jobs that start on scheduled time.
       // double FinishPunctuality; // percentage jobs that finish before their LSS.
       
        //
        //Statistics of each individual job (completion times etc)
        //

        public SimulationPerformanceMeasures(int id, double PunctualityAllowance, int Njobs, Simulation sim)
        {
            RunID = id;
            TotalLinearStartDelay = 0;
            delta = PunctualityAllowance;
            if (delta < 0 || delta > 1)
            {
                throw new Exception("Delta too large. Delta is the permitted relative increase. Reasonable numbers are probably in [0,0.1] range");
            }
            else if(delta > 0.1)
            {
                    Console.WriteLine("WARNING, Delta very large. Delta is the permitted relative increase. Reasonable numbers are probably in [0,0.1] range");
            }
            StartOnTimeJobs = 0;
            FinishOnTimeJobs = 0;
            NJobs = (double)Njobs;
            Sim = sim;
            

        }

        public void WriteToFile(string path)
        {
            using (StreamWriter sw = File.AppendText(path))
            {
                if (new FileInfo(path).Length == 0)
                {
                    sw.Write("Distribution Type;");
                    sw.Write("Instance Name; Schedule AssignType; Schedule StartTimeType;");
                    foreach (RM rm in Sim.Sched.RMs)
                    {
                        sw.Write("{0};", rm.Name);
                    }
                    sw.Write("Run nr");
                    foreach (string qmname in Constants.QMNames)
                    {
                        sw.Write(";{0}", qmname);
                    }
                    if (Constants.INCLUDEJOBINFO)
                    {
                        foreach (SimulationJob j in Sim.SimulationJobs)
                        {
                            sw.Write(";ScheduledStartTime{0};RealisedStartTime{0};RealisedProcessingTime{0};Machine{0}", j.JobParams.ID);
                        }
                    }
                    sw.Write(Environment.NewLine);
                }

                sw.Write("{0};", Sim.DistributionType);
                sw.Write("{0};{1};{2};", Sim.Sched.Problem.Description, Sim.Sched.AssignmentDescription,Sim.Sched.StartTimeDescription);
                //RMs:
                foreach (RM rm in Sim.Sched.RMs)
                {
                    sw.Write("{0};", rm.Value);
                }
                //QMs:
                sw.Write("{0};{1};{2};{3};{4}",
                RunID,
                Cmax,
                TotalLinearStartDelay,
                (double)StartOnTimeJobs / NJobs,
                (double)FinishOnTimeJobs / NJobs);
                if (Constants.INCLUDEJOBINFO)
                {
                    foreach (SimulationJob j in Sim.SimulationJobs)
                    {
                        sw.Write(";{0};{1};{2};{3}", j.Sim.Sched.GetStartTimeOfJob(j.JobParams), j.RealisedStartTime, j.RealisedProcessingTime, j.Sim.Sched.GetMachineByJobID(j.JobParams.ID).MachineID);
                        if (j.Sim.Sched.GetStartTimeOfJob(j.JobParams) > j.RealisedStartTime)
                        {
                            throw new Exception("Job started early");
                        }
                    }
                }
                sw.Write(Environment.NewLine);
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
            //else { Console.WriteLine("Delayed"); }

        }


        public void UpdateFinishPunctuality(Job finishedJob, Schedule S, double RealizedFinish)
        {
            foreach (Job suc in finishedJob.Successors) // todo: doets successors include machine successors?
            {
                if (RealizedFinish <= (1 + delta) * S.GetStartTimeOfJob(suc))
                {
                    // on time
                    continue;
                }
                else
                {
                    // job not on time
                    return;                    
                }
            }
            // all successors not delayed:
            FinishOnTimeJobs++;
        }

    }
}
