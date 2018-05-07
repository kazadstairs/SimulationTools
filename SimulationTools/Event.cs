using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    abstract class Event : IComparable<Event>
    {
        public double Time { get; protected set; }
        protected Simulation Sim;
        protected string DebugDescription;

        protected Event()
        {

        }

        public Event(double time, Simulation sim)
        {
            Time = time;
            Sim = sim;
        }

        virtual public void Handle()
        {
            Console.WriteLine("A {0} Event occured at time {1}", DebugDescription, Time);
        }

        public int CompareTo(Event other)
        {
            return this.Time.CompareTo(other.Time);
        }

    }

    class EJobRelease : Event // Triggers whenever a release date occurs
    {
        Job J;
        public EJobRelease(double time, Simulation sim, Job j)
        {
            Time = time;
            Sim = sim;
            J = j;
            DebugDescription = "Job Release";
        }

        override public void Handle()
        {
            if (J.isAvailableAt(Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, J)); }
            base.Handle();
        }
    }

    class EJobScheduledStart : Event // Triggers whenever a scheduledstart occurs
    {
        Job J;
        public EJobScheduledStart(double time, Simulation sim, Job j)
        {
            Time = time;
            Sim = sim;
            J = j;
            DebugDescription = "Job Scheduled to Start";
        }

        override public void Handle()
        {
            if (J.isAvailableAt(Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, J)); }
            base.Handle();
        }
    }

    

    class EJobComplete : Event
    {
        Job J;
        public EJobComplete(double time, Simulation sim, Job j)
        {
            Time = time;
            Sim = sim;
            J = j;
            DebugDescription = "Job Completed";
        }

        public override void Handle()
        {
            // make machine available:
            Sim.EventList.Insert(new EMachineAvailable(Time, Sim, J.Machine));
            // tell successor jobs this job is finished and check for new available jobs:
            foreach(Job suc in J.Successors)
            {
                suc.PredComplete();
                if (suc.isAvailableAt(Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, suc)); }
                base.Handle();
            }
        }
    }

    class EJobAvailable : Event
    {
        Job J;
        public EJobAvailable(double time, Simulation sim, Job j)
        {
            Time = time;
            Sim = sim;
            J = j;
            DebugDescription = "Job becomes Available";
        }

        public override void Handle()
        {
            if (J.Machine.isAvailable) // if the machine for the job is available, then start the job on that machine
            {
                //start job j on the machine it is assigned to:
                // machine is busy:
                J.Machine.isAvailable = false;
                Sim.EventList.Insert(new EJobComplete(Time + J.GetProcessingTime(), Sim, J));
            }
            else
            {
                J.Machine.JobsWaitingToStart.Enqueue(J); // add to queue
            }
            base.Handle();
        }
    }

    class EMachineAvailable : Event
    {
        Machine M;
        public EMachineAvailable(double time, Simulation sim, Machine m)
        {
            Time = time;
            Sim = sim;
            M = m;
            DebugDescription = "Machine becomes Available";
        }

        public override void Handle()
        {
            M.isAvailable = true;

            if(M.JobsWaitingToStart.Count > 0)
            {
                // start the next job that is ready on this machine
                Sim.EventList.Insert(new EJobComplete(Time, Sim, M.JobsWaitingToStart.Dequeue()));
                M.isAvailable = false;
            }
            base.Handle();
        }

    }
}
