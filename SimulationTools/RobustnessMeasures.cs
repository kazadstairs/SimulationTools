using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class RobustnessMeasures
    {
        /// <summary>
        /// The free slack of job j in schedule S is the amount of time j can slip without delaying the start of the very next activity.
        /// </summary>
        /// <param name="j">Job j</param>
        /// <param name="S">Schedule s</param>
        /// <returns></returns>
        static double FreeSlack(Job j, Schedule S)
        {
            
        }
        static double SumOfFreeSlacks(Schedule S)
        {
            double sum = 0.0;
            foreach (Job j in S.DAG.Jobs)
            {
                sum += S.DynamicDueDate[j.ID] - j.MeanProcessingTime - S.ESS[j.ID];
            }
            return sum;
        }


    }
}
