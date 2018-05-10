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
        public DirectedAcyclicGraph DAG;
        public List<Machine> Machines;

        public Schedule()
        {
            DAG = new DirectedAcyclicGraph();
            Machines = new List<Machine>();
        }

        public void InstanciatePinedo()
        {
            DAG.AddJob(new Job(1, 4, -1));
            DAG.AddJob(new Job(2, 9, -1));
            DAG.AddJob(new Job(3, 3, -1));
            DAG.AddJob(new Job(4, 3, -1));
            DAG.AddJob(new Job(5, 6, -1));
            DAG.AddJob(new Job(6, 8, -1));
            DAG.AddJob(new Job(7, 12, -1));
            DAG.AddJob(new Job(8, 8, -1));
            DAG.AddJob(new Job(9, 6, -1));


            // precedence arcs
            DAG.AddArcById(1, 2);
            DAG.AddArcById(2, 6);
            DAG.AddArcById(3, 4);
            DAG.AddArcById(4, 5);
            DAG.AddArcById(5, 6);
            DAG.AddArcById(5, 7);
            DAG.AddArcById(6, 8);
            DAG.AddArcById(7, 8);
            DAG.AddArcById(7, 9);

            // machine arcs all match prec arcs in this instance
            Machines.Add(new Machine(0));
            Machines.Add(new Machine(1));

            AssignJobToMachineById(1, 0);
            AssignJobToMachineById(2, 0);
            AssignJobToMachineById(6, 0);
            AssignJobToMachineById(8, 0);

            AssignJobToMachineById(3, 1);
            AssignJobToMachineById(4, 1);
            AssignJobToMachineById(5, 1);
            AssignJobToMachineById(7, 1);
            AssignJobToMachineById(9, 1);
        }





        /*
        public void PinedoInstanceSchedule(ProblemInstance Problem)
        {
            
            AssignJobToMachine(Problem.JobsList[0], Problem.Machines[0]); //Dummy job

            AssignJobToMachine(Problem.JobsList[1], Problem.Machines[0]);
            AssignJobToMachine(Problem.JobsList[2], Problem.Machines[0]);
            AssignJobToMachine(Problem.JobsList[6], Problem.Machines[0]);
            AssignJobToMachine(Problem.JobsList[8], Problem.Machines[0]);

            AssignJobToMachine(Problem.JobsList[3], Problem.Machines[1]);
            AssignJobToMachine(Problem.JobsList[4], Problem.Machines[1]);
            AssignJobToMachine(Problem.JobsList[5], Problem.Machines[1]);
            AssignJobToMachine(Problem.JobsList[7], Problem.Machines[1]);
            AssignJobToMachine(Problem.JobsList[9], Problem.Machines[1]);


        }
        */

        // todo, algorithm to make feasible schedules.

        void AssignJobToMachine(Job j, Machine m)
        {
            if (j.IsAssigned) { throw new System.Exception("Job already assigned!"); }
            else
            {
                m.AssignedJobs.Add(j);
                j.AssignToMachine(m);
            }
        }

        void AssignJobToMachineById(int Jid, int Mid)
        {
            AssignJobToMachine(DAG.GetJobById(Jid), Machines[Mid]);
        }
    }
}
