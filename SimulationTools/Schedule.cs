using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Schedule
    {
       // Job[] Jobs;
       // Machine[] Machines;
       // List<Tuple<Job, Machine, double>> Assignments;

        public void PinedoInstanceSchedule(ProblemInstance Problem)
        {
            
            AssignJobToMachine(Problem.JobsList[0], Problem.Machines[1]); //Dummy job

            AssignJobToMachine(Problem.JobsList[1], Problem.Machines[1]);
            AssignJobToMachine(Problem.JobsList[2], Problem.Machines[1]);
            AssignJobToMachine(Problem.JobsList[6], Problem.Machines[1]);
            AssignJobToMachine(Problem.JobsList[8], Problem.Machines[1]);

            AssignJobToMachine(Problem.JobsList[3], Problem.Machines[2]);
            AssignJobToMachine(Problem.JobsList[4], Problem.Machines[2]);
            AssignJobToMachine(Problem.JobsList[5], Problem.Machines[2]);
            AssignJobToMachine(Problem.JobsList[7], Problem.Machines[2]);
            AssignJobToMachine(Problem.JobsList[9], Problem.Machines[2]);


        }

        // todo, algorithm to make feasible schedules.

        void AssignJobToMachine(Job j, Machine m)
        {
            if (j.isAssigned) { throw new System.Exception("Job already assigned!"); }
            else
            {
                m.AssignedJobs.Add(j);
                j.AssignToMachine(m);
            }
        }
    }
}
