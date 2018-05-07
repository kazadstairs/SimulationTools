using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Schedule
    {
        Job[] Jobs;
        Machine[] Machines;
        List<Tuple<Job, Machine, double>> Assignments;

        void AssignJobToMachine(Job j, Machine m)
        {
            if (j.isAssigned) { throw new System.Exception("Job already assigned!"); }
            else
            {
                j.isAssigned = true;
                m.AssignedJobs.Add(j);
                j.Machine = m;
            }
        }
    }
}
