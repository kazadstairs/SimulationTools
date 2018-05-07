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
        public ProblemInstance Problem;
        //public State CurrentState; // not used


        public void Run()
        {
            ;
            SetupSimulation();
            PerformSimulation();
            

        }

        private void SetupSimulation()
        {
            Console.WriteLine("***** Setting Up Simulation...");
            EventList = new PriorityQueue<Event>();
            Problem = new ProblemInstance();
            Problem.InstanciatePinedo();
            foreach(Job J in Problem.JobsList)
            {
                EventList.Insert(new EJobRelease(J.ReleaseDate, this, J));
                EventList.Insert(new EJobScheduledStart(J.ScheduleStartTime, this, J));
            }
            // at the beginning, all machines are available
            foreach(Machine M in Problem.Machines)
            {
                EventList.Insert(new EMachineAvailable(0, this, M));
            }
            EventList.Insert(new EJobComplete(0, this, Problem.JobsList[0])); //Will start the simulation by saying the dummy job has finished.
        }

        private void PerformSimulation()
        {
            Console.WriteLine("***** Performing Simulation...");
            int eventcounter = 0;
            //todo remove eventcounter
            Stopwatch watch = Stopwatch.StartNew();
            while (EventList.Count > 0 && eventcounter < 1000)
            {
                EventList.ExtractMin().Handle();
                eventcounter++;
                if(eventcounter % 10 == 0)
                {
                    Console.WriteLine("{0} events processed", eventcounter);
                }
            }
            Console.WriteLine("***** Simluation Complete. In total {0} events were processed in {1} ms", eventcounter, watch.ElapsedMilliseconds);
        }
    }
}
