using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class FitnessFunctions
    {
        public static double MeanBasedCmax(Schedule S)
        {
            return S.EstimateCmax();
        }

        public static double MeanCmaxBugged(Schedule S)
        {
            throw new Exception("Bugged Code, Do not use");
            int[] TopoOrdering = S.GetJobIdsTopologically();
            double[] CompletionTimes = new double[S.PrecedenceDAG.N+1];
            double Cmax = 0;
            for (int i = 0; i < TopoOrdering.Length; i++)
            {
                Job CurrentJob = S.PrecedenceDAG.GetJobById(TopoOrdering[i]);
                CompletionTimes[CurrentJob.ID] = CurrentJob.MeanProcessingTime;
                foreach (Job parent in CurrentJob.Predecessors)
                {
                    if (CompletionTimes[CurrentJob.ID] < CompletionTimes[parent.ID] + CurrentJob.MeanProcessingTime)
                    {
                        CompletionTimes[CurrentJob.ID] = CompletionTimes[parent.ID] + CurrentJob.MeanProcessingTime;
                    }
                }
                if (S.GetMachinePredecessor(CurrentJob) != null && CompletionTimes[CurrentJob.ID] < CompletionTimes[S.GetMachinePredecessor(CurrentJob).ID] + CurrentJob.MeanProcessingTime)
                {
                    CompletionTimes[CurrentJob.ID] = CompletionTimes[S.GetMachinePredecessor(CurrentJob).ID] + CurrentJob.MeanProcessingTime;
                    Console.WriteLine("MACHINE RELATION UPDATE!");
                }
                Console.WriteLine("Job {0} deterministic completion at: {1}", CurrentJob.ID, CompletionTimes[CurrentJob.ID]);
                if (CompletionTimes[CurrentJob.ID] > Cmax)
                {
                    Cmax = CompletionTimes[CurrentJob.ID];
                    Console.WriteLine("Makespan updated to {0}",CompletionTimes[CurrentJob.ID]);
                }


            }
            Console.WriteLine("Makespan: {0}", Cmax);

            return Cmax;
        }

        
    }
}
