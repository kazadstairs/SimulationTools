using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class SimulationJob
    {
        // job and schedule info;
        public Job JobParams;
        public List<SimulationJob> Predecessors;
        public List<SimulationJob> Successors;
        public Simulation Sim;

        // simulation vars (these all need reset in ResetSimulation):
        public int nPredComplete { get; private set; }
        public bool HasBeenMadeAvailable;

        public SimulationJob(Job OriginalJob, Simulation _Sim)
        {
            JobParams = OriginalJob;
            Predecessors = new List<SimulationJob>(JobParams.Predecessors.Count + 1);
            Successors = new List<SimulationJob>(JobParams.Successors.Count + 1);
            Sim = _Sim;
        }

        public void SetPredAndSucc()
        {
            if (Sim.Sched.GetMachinePredecessor(JobParams) != null)
            {
                Job Mpred;
                Mpred = Sim.Sched.GetMachinePredecessor(JobParams);
                Predecessors.Add(Sim.SimulationJobs[Mpred.ID]);
                foreach (Job p in JobParams.Predecessors)
                {
                    if (p != Mpred) { Predecessors.Add(Sim.SimulationJobs[p.ID]); }
                    if (Sim.SimulationJobs[p.ID] == null) throw new Exception("Added a null job");
                }
            }
            else
            {
                foreach (Job p in JobParams.Predecessors)
                {
                    Predecessors.Add(Sim.SimulationJobs[p.ID]);
                    if (Sim.SimulationJobs[p.ID] == null) throw new Exception("Added a null job");
                }

            }


            if (Sim.Sched.GetMachineSuccessor(JobParams) != null)
            {
                Job MSucc = Sim.Sched.GetMachineSuccessor(JobParams);
                Successors.Add(Sim.SimulationJobs[MSucc.ID]);

                foreach (Job succ in JobParams.Successors)
                {
                    if (succ != MSucc) { Successors.Add(Sim.SimulationJobs[succ.ID]); }
                    if (Sim.SimulationJobs[succ.ID] == null) throw new Exception("Added a null job");
                }
            }
            else
            {
                foreach (Job succ in JobParams.Successors)
                {
                    Successors.Add(Sim.SimulationJobs[succ.ID]);
                    if (Sim.SimulationJobs[succ.ID] == null) throw new Exception("Added a null job");
                }
            }


        }

        public void PredComplete()
        {
            nPredComplete++;
        }

        public bool IsAvailableAt(double time)
        {
            if (HasBeenMadeAvailable) { return false; } // throw new Exception("Job Available for a second time!"); }
            if ((JobParams.EarliestReleaseDate <= time)
                    && (AllPredComplete()) //|| Predecessors.Count == 0
                    && (Sim.Sched.GetStartTimeOfJob(JobParams) <= time)
                    )
            {
                HasBeenMadeAvailable = true;
                return true;
            }
            return false;
        }

        public bool AllPredComplete() // todo, just return comparison
        {
            return nPredComplete >= Predecessors.Count;
            if (nPredComplete >= Predecessors.Count)
            {
                Console.WriteLine("ALL PREDECESSORS COMPLETE FOR JOB {0}", JobParams.ID);
                return true;
            }
            return false;
        }

        public double SampleProcessingTime()
        {
            //todo: this is arbitrary std dev
            return Distribution.SampleNormal(JobParams.MeanProcessingTime, 1.0);
        }

        public void ResetSimulationVars()
        {
            nPredComplete = 0;
            HasBeenMadeAvailable = false;
        }
    }
}
