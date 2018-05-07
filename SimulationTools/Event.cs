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
           Console.WriteLine(DebugDescription);
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
            DebugDescription = string.Format("Job {0} released at time {1}",J.id,Time);
        }

        override public void Handle()
        {
            if (J.IsAvailableAt(Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, J)); }
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
            DebugDescription = string.Format("Job {0} scheduled for start at time {1}", J.id, Time);
        }

        override public void Handle()
        {
            if (J.IsAvailableAt(Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, J)); }
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
            DebugDescription = string.Format("Job {0} was completed at time {1} on machine {2}", J.id, Time, J.Machine.id);
        }

        public override void Handle()
        {
            // make machine available:
            Sim.EventList.Insert(new EMachineAvailable(Time, Sim, J.Machine));
            // tell successor jobs this job is finished and check for new available jobs:
            foreach(Job suc in J.Successors)
            {
                suc.PredComplete();
                if (suc.IsAvailableAt(Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, suc)); }
                base.Handle();
            }
           // Console.WriteLine("Job {0} was completed at time {1} on machine {2}", J.id, Time, J.Machine.id);
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
            DebugDescription = string.Format("Job {0} became available at time {1}. It is assigned to machine {2}", J.id, Time, J.Machine.id);
        }

        public override void Handle()
        {
            if (!J.IsAssigned) { throw new Exception("Unassigned job became available"); }
            if (J.Machine.isAvailable) // if the machine for the job is available, then start the job on that machine
            {
                //start job j on the machine it is assigned to:
                // machine is busy:
                Sim.EventList.Insert(new EJobStart(Time, Sim, J));
            }
            else
            {
                J.Machine.JobsWaitingToStart.Enqueue(J); // add to queue
            }
            base.Handle();
        }
    }

    class EJobStart : Event
    {
        Job J;
        public EJobStart(double time, Simulation sim, Job j)
        {
            Time = time;
            Sim = sim;
            J = j;
            DebugDescription = string.Format("Job {0} started processing at time {1} on machine {2}", J.id, Time, J.Machine.id);
        }

        public override void Handle()
        {
            J.Machine.isAvailable = false;
            Sim.EventList.Insert(new EJobComplete(Time + J.GetProcessingTime(), Sim, J));
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
            DebugDescription = string.Format("Machine {0} became available at time {1} ", M.id, Time); ;
        }

        public override void Handle()
        {
            M.isAvailable = true;

            if(M.JobsWaitingToStart.Count > 0)
            {
                // start the next job that is ready on this machine
                Job NextJob = M.JobsWaitingToStart.Dequeue();
                Sim.EventList.Insert(new EJobStart(Time, Sim, NextJob));
            }
            base.Handle();
        }

    }
}
