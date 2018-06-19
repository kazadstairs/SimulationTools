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
           //Console.WriteLine(DebugDescription);
        }

        public int CompareTo(Event other)
        {
            return this.Time.CompareTo(other.Time);
        }


        protected Machine GetMachineForJob(Job j)
        {
            return Sim.Sched.GetMachineByJobID(j.ID);
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
            DebugDescription = string.Format("Job {0} released at time {1}",J.ID,Time);
        }

        override public void Handle()
        {
            if (Sim.IsAvailableAt(J,Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, J)); }
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
            DebugDescription = string.Format("Job {0} scheduled for start at time {1}", J.ID, Time);
        }

        override public void Handle()
        {
            if (Sim.IsAvailableAt(J, Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, J)); }
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
            DebugDescription = string.Format("Job {0} was completed at time {1} on machine {2}", J.ID, Time, GetMachineForJob(J).MachineID );
        }

        public override void Handle()
        {
            // make machine available:
            Sim.EventList.Insert(new EMachineAvailable(Time, Sim, GetMachineForJob(J)));
            Sim.PerformanceMeasures.UpdateFinishPunctuality(J, Sim.Sched, Time);
            // tell successor jobs this job is finished and check for new available jobs
            foreach(Job suc in J.Successors)
            {
                suc.PredComplete();
                if (Sim.IsAvailableAt(suc, Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, suc)); }
                base.Handle();
            }
           // Console.WriteLine("Job {0} was completed at time {1} on machine {2}", J.id, Time, GetMachineForJob(J).id);
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
            DebugDescription = string.Format("Job {0} became available at time {1}. It is assigned to machine {2}", J.ID, Time, GetMachineForJob(J).MachineID);
        }

        public override void Handle()
        {
            // todo: check elsewhere if (!J.IsAssigned) { throw new Exception("Unassigned job became available"); }
            if (GetMachineForJob(J).isAvailable) // if the machine for the job is available, then start the job on that machine
            {
                //start job j on the machine it is assigned to:
                // machine is busy:
                Sim.EventList.Insert(new EJobStart(Time, Sim, J));
            }
            else
            {
                GetMachineForJob(J).JobsWaitingToStart.Enqueue(J); // add to queue
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
            DebugDescription = string.Format("Job {0} STARTED PROCESSING at time {1} on machine {2}", J.ID, Time, GetMachineForJob(J).MachineID);
        }

        public override void Handle()
        {
            GetMachineForJob(J).isAvailable = false;
            Sim.EventList.Insert(new EJobComplete(Time + J.SampleProcessingTime(), Sim, J));
            Sim.PerformanceMeasures.AddLinearStartDelay(Sim.Sched.GetStartTimeOfJob(J), Time);
            Sim.PerformanceMeasures.UpdateStartPunctuality(Sim.Sched.GetStartTimeOfJob(J), Time);
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
            DebugDescription = string.Format("Machine {0} became available at time {1} ", M.MachineID, Time); ;
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
