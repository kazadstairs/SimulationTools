using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    partial class Schedule
    {
        public void MakeScheduleForBlockInstance()
        {
            int MachineID;
            foreach (Job J in PrecedenceDAG.Jobs)
            {
                MachineID = (J.ID % 4) + 1;
                AssignJobToMachine(J, GetMachineByID(MachineID));
            }

            AssignmentDescription = string.Format("BlockInstance_{0}_JobsPerMachine", PrecedenceDAG.N / Machines.Count);
            CalcESS();
            SetESS();
        }

    }
}
