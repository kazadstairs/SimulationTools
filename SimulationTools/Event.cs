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

            throw new System.NotImplementedException();
        }

        public int CompareTo(Event other)
        {
            return this.Time.CompareTo(other.Time);
        }

    }

    class EGeneric : Event // Does nothing, for debugging only
    {
        public EGeneric(double time, Simulation sim) : base(time, sim) { }

        override public void Handle()
        {
            Console.WriteLine("An EGeneric event happenned at time: {0}", Time);
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
        }

        override public void Handle()
        {
            if (Time >= J.ScheduleStartTime)
            {
                // Currentime = r_j >= s_j
                if (J.allPredComplete)
                {
                    // todo: make job available
                }
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
        }

        public override void Handle()
        {
            base.Handle();
        }

    }
}
