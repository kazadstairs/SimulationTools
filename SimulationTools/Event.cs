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
        //   Console.WriteLine(DebugDescription);
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
        SimulationJob J;
        public EJobRelease(double time, Simulation sim, SimulationJob j)
        {
            Time = time;
            Sim = sim;
            J = j;
            DebugDescription = string.Format("Job {0} released at time {1}",J.JobParams.ID,Time);
        }

        override public void Handle()
        {
            if (J.IsAvailableAt(Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, J)); }
            base.Handle();
        }
    }

    class EJobScheduledStart : Event // Triggers whenever a scheduledstart occurs
    {
        SimulationJob J;
        public EJobScheduledStart(double time, Simulation sim, SimulationJob j)
        {
            Time = time;
            Sim = sim;
            J = j;
            DebugDescription = string.Format("Job {0} scheduled for start at time {1}", J.JobParams.ID, Time);
        }

        override public void Handle()
        {
            if (J.IsAvailableAt(Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, J)); }
            base.Handle();
        }
    }

    

    class EJobComplete : Event
    {
        SimulationJob J;
        public EJobComplete(double time, Simulation sim, SimulationJob j)
        {
            Time = time;
            Sim = sim;
            J = j;
            DebugDescription = string.Format("Job {0} was completed at time {1} on machine {2}", J.JobParams.ID, Time, GetMachineForJob(J.JobParams).MachineID );
        }

        public override void Handle()
        {
            // make machine available:
            Sim.EventList.Insert(new EMachineAvailable(Time, Sim, GetMachineForJob(J.JobParams)));
            Sim.PerformanceMeasures.UpdateFinishPunctuality(J.JobParams, Sim.Sched, Time);
            // tell successor jobs this job is finished and check for new available jobs
            foreach(SimulationJob suc in J.Successors)
            {
                suc.PredComplete();
                if (suc.IsAvailableAt(Time)) { Sim.EventList.Insert(new EJobAvailable(Time, Sim, suc)); }
                base.Handle();
            }
           // Console.WriteLine("Job {0} was completed at time {1} on machine {2}", J.JobParams.id, Time, GetMachineForJob(J).id);
        }
    }

    class EJobAvailable : Event
    {
        SimulationJob J;
        public EJobAvailable(double time, Simulation sim, SimulationJob j)
        {
            Time = time;
            Sim = sim;
            J = j;
            DebugDescription = string.Format("Job {0} became available at time {1}. It is assigned to machine {2}", J.JobParams.ID, Time, GetMachineForJob(J.JobParams).MachineID);
        }

        public override void Handle()
        {
            if (GetMachineForJob(J.JobParams).isAvailable) // if the machine for the job is available, then start the job on that machine
            {
                //start job j on the machine it is assigned to:
                // machine is busy:
                Sim.EventList.Insert(new EJobStart(Time, Sim, J));
            }
            else
            {
                GetMachineForJob(J.JobParams).JobsWaitingToStart.Enqueue(J.JobParams); // add to queue
            }
            base.Handle();
        }
    }

    class EJobStart : Event
    {
        SimulationJob J;
        public EJobStart(double time, Simulation sim, SimulationJob j)
        {
            Time = time;
            Sim = sim;
            J = j;
            DebugDescription = string.Format("Job {0} STARTED PROCESSING at time {1} on machine {2}", J.JobParams.ID, Time, GetMachineForJob(J.JobParams).MachineID);
        }

        public override void Handle()
        {
            GetMachineForJob(J.JobParams).isAvailable = false;
            J.RealisedProcessingTime = J.SampleProcessingTime();
            Sim.EventList.Insert(new EJobComplete(Time + J.RealisedProcessingTime, Sim, J));
            Sim.PerformanceMeasures.AddLinearStartDelay(Sim.Sched.GetStartTimeOfJob(J.JobParams), Time);
            Sim.PerformanceMeasures.UpdateStartPunctuality(Sim.Sched.GetStartTimeOfJob(J.JobParams), Time);

        //    J.Sim.DEBUGJobsStarted++;
            J.Sim.Sched.GetStartTimeOfJob(J.JobParams);
            J.RealisedStartTime = Time;
           // Console.WriteLine(DebugDescription);
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
                Sim.EventList.Insert(new EJobStart(Time, Sim, Sim.SimulationJobs[NextJob.ID]));
            }
            base.Handle();
        }

    }
}
