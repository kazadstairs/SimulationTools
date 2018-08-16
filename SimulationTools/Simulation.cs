using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SimulationTools
{
    class Simulation
    {
        public PriorityQueue<Event> EventList;
        public Schedule Sched;
        public SimulationPerformanceMeasures PerformanceMeasures;
        public SimulationJob [] SimulationJobs;
        int NRuns;
        private string OutPutPath; // { get; private set; }
        public string SimulationSettingsOutPutPath { get; private set; }
        public string SimulationResultsOutPutPath { get; private set; }
        bool[] HasBeenMadeAvailable;
        //public State CurrentState; // not used

        public Simulation(int _Nruns, Schedule _sched)
        {
            NRuns = _Nruns;
            Sched = _sched;
            BuildPath();
            SimulationJobs = new SimulationJob[Sched.PrecedenceDAG.N];
            for (int i = 0; i < Sched.PrecedenceDAG.N; i++)
            {
                SimulationJobs[i] = new SimulationJob(Sched.PrecedenceDAG.Jobs[i],this);
            }
            for (int i = 0; i < Sched.PrecedenceDAG.N; i++)
            {
                SimulationJobs[i] = new SimulationJob(Sched.PrecedenceDAG.Jobs[i], this);
            }
            HasBeenMadeAvailable = new bool[Sched.PrecedenceDAG.N];

        }

        private void BuildPath()
        {
            OutPutPath = string.Format(@"{0}Results\RMs\",Program.BASEPATH);
            string InstanceName = Sched.Problem.Description;
            SimulationSettingsOutPutPath = OutPutPath + string.Format("Instance_{0}_Schedule_{1}_Runs_{2}_SimSettings.txt", InstanceName, Sched.Description, NRuns);
            SimulationResultsOutPutPath = OutPutPath + string.Format("Instance_{0}_Schedule_{1}_Runs_{2}_QMs.txt", InstanceName, Sched.Description, NRuns);
            //OutPutPath += string.Format("Instance_{0}_Schedule_{1}_Runs_{2}.txt", InstanceName,Sched.Description,NRuns);
            using (StreamWriter swQMs = File.CreateText(SimulationResultsOutPutPath))
            {
                
            }
            // Write the header info
            using (StreamWriter swRMs = File.CreateText(SimulationSettingsOutPutPath))
            {
                swRMs.WriteLine(InstanceName);
                swRMs.WriteLine(Sched.Description);
                swRMs.WriteLine(RobustnessMeasures.RMCount);
                swRMs.WriteLine("Sum Of Free Slacks");
                swRMs.Write(RobustnessMeasures.SumOfFreeSlacks(Sched));
                swRMs.Write(Environment.NewLine);
                swRMs.WriteLine(NRuns);
            }
        }


        public void Perform()
        {
            for (int runnr = 0; runnr < NRuns; runnr++)
            {
                PerformanceMeasures = new SimulationPerformanceMeasures(runnr,100,Sched.PrecedenceDAG.N,this);
             //   Console.WriteLine("***** Performing Simulation {0}...", runnr);
                SetupSimulation();
                PerformSimulation();
                PerformanceMeasures.WriteToFile(SimulationResultsOutPutPath);
                CleanJobs();
            }
            Console.WriteLine("Simulation completed");

        }

        private void SetupSimulation()
        {
         //   Console.WriteLine("***** Setting Up Simulation...");
            EventList = new PriorityQueue<Event>();
            CreateSimulationJobDAG();
            //Problem = new ProblemInstance();

            foreach(SimulationJob J in SimulationJobs)
            {
                EventList.Insert(new EJobRelease(J.JobParams.EarliestReleaseDate, this, J));
                EventList.Insert(new EJobScheduledStart(Sched.GetStartTimeOfJob(J.JobParams), this, J));
            }
            // at the beginning, all machines are available
            foreach(Machine M in Sched.Machines)
            {
                EventList.Insert(new EMachineAvailable(0, this, M));
            }
        }

        private void CreateSimulationJobDAG()
        {
            Sched.ForeachJobInPrecOrderDo((Job j) => SimulationJobs[j.ID] = new SimulationJob(j, this));
            Sched.ForeachJobInPrecOrderDo((Job j) => SimulationJobs[j.ID].SetPredAndSucc());
        }

        private void PerformSimulation()
        {
             
                int eventcounter = 0;
                //todo remove eventcounter
                Stopwatch watch = Stopwatch.StartNew();
                while (EventList.Count > 0 && eventcounter < 1000)
                {
                    Event NextEvent = EventList.ExtractMin();
                    NextEvent.Handle();
                    eventcounter++;
                    if (eventcounter % 10 == 0)
                    {
                        //Console.WriteLine("{0} events processed", eventcounter);
                    }
                    PerformanceMeasures.Cmax = NextEvent.Time;
                }
                
               // Console.WriteLine("***** Run Complete. In total {0} events were processed in {1} ms", eventcounter, watch.ElapsedMilliseconds);
                           
        }

        private void CleanJobs()
        {
            foreach (SimulationJob j in SimulationJobs)
            {
                j.ResetSimulationVars();
                j.HasBeenMadeAvailable = false; // todo put this in reset simulationvars
            }
        }
        
    }
}
