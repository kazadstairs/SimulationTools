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

        public double RealisedStartTime;
        public double RealisedProcessingTime;

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
        }

        /// <summary>
        /// Samples from a distribution, according to the type determined in the simulation.
        /// </summary>
        /// <returns></returns>
        public double SampleProcessingTime()
        {
            if (JobParams.ID == 0) { return 0; }
            switch (Sim.DistributionType)
            {
                case "N(p,1)":
                    return DistributionFunctions.SampleNormal(JobParams.MeanProcessingTime, 1.0);
                case "N(p,0.01p)":
                    return DistributionFunctions.SampleNormal(JobParams.MeanProcessingTime, 0.01 * JobParams.MeanProcessingTime);
                case "N(p,0.1p)":
                    return DistributionFunctions.SampleNormal(JobParams.MeanProcessingTime, 0.1 * JobParams.MeanProcessingTime);
                case "N(p,0.25p)":
                    return DistributionFunctions.SampleNormal(JobParams.MeanProcessingTime, 0.25 * JobParams.MeanProcessingTime);
                case "Exp(p)":
                    return DistributionFunctions.SampleExponential(JobParams.MeanProcessingTime);
                case "LN(p,0.01p)":
                    return DistributionFunctions.SampleLogNormal(JobParams.MeanProcessingTime, 0.01 * JobParams.MeanProcessingTime);
                case "LN(p,0.1p)":
                    return DistributionFunctions.SampleLogNormal(JobParams.MeanProcessingTime, 0.1 * JobParams.MeanProcessingTime);
                case "LN(p,0.25p)":
                    return DistributionFunctions.SampleLogNormal(JobParams.MeanProcessingTime, 0.25 * JobParams.MeanProcessingTime);

                default:
                    throw new Exception("Distribution not recognized");

            }
            
        }

        public void ResetSimulationVars()
        {
            nPredComplete = 0;
            HasBeenMadeAvailable = false;
        }
    }
}
