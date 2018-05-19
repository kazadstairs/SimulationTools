using System;
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
        int NRuns;
        public string OutPutPath { get; private set; }
        //public State CurrentState; // not used

        public Simulation(int _Nruns, Schedule _sched)
        {
            NRuns = _Nruns;
            Sched = _sched;
            BuildPath();
            
        }

        private void BuildPath()
        {
            OutPutPath = string.Format(@"C:\Users\Gebruiker\Documents\UU\MSc Thesis\Code\OutPut\");
            string InstanceName = Sched.Problem.Description;
            OutPutPath += string.Format("Instace_{0}_Runs_{1}.txt", InstanceName,NRuns);
        }


        public void Perform()
        {
                SetupSimulation();
                PerformSimulation();         

        }

        private void SetupSimulation()
        {
            Console.WriteLine("***** Setting Up Simulation...");
            EventList = new PriorityQueue<Event>();
            // Todo: A simulation should just be given a schedule and a problem instance, not create them.
            //Problem = new ProblemInstance();

            foreach(Job J in Sched.DAG.Jobs)
            {
                EventList.Insert(new EJobRelease(J.EarliestReleaseDate, this, J));
                EventList.Insert(new EJobScheduledStart(J.ScheduleStartTime, this, J));
            }
            // at the beginning, all machines are available
            foreach(Machine M in Sched.Machines)
            {
                EventList.Insert(new EMachineAvailable(0, this, M));
            }
        }

        private void PerformSimulation()
        {
            for (int runnr = 0; runnr < NRuns; runnr++)
            {
                PerformanceMeasures = new SimulationPerformanceMeasures(runnr);
                Console.WriteLine("***** Performing Simulation {0}...",runnr);
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
                        Console.WriteLine("{0} events processed", eventcounter);
                    }
                }
                Console.WriteLine("***** Simluation Complete. In total {0} events were processed in {1} ms", eventcounter, watch.ElapsedMilliseconds);


            }
        }
    }
}
