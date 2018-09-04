using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class NeighborhoodFunctions
    {
        public static Schedule SameMachineSwap(Schedule CurrentSchedule, Func<Schedule, double> FitnessFunction)
        {
            int StartMachineId = Distribution.UniformInt(CurrentSchedule.Problem.NMachines - 1) + 1; // 1 to 11 (incl)
            int CurrentMachineId = StartMachineId;
            int NMachinesTried = 0;
           // bool improvementFound = false;
            Machine CurrentMachine = null;  
            //Console.WriteLine("TODO LAZY: MAKE above assignment a CALL A FUNCTION IN SCHEDULE!");
            while (NMachinesTried < CurrentSchedule.Problem.NMachines)
            {
                CurrentMachine = CurrentSchedule.Machines[CurrentMachineId - 1];
                int JobsOnMachine = CurrentMachine.AssignedJobs.Count;
                if (JobsOnMachine <= 1)
                {
                    // swap no good, proceed to update counters
                }
                else
                {
                    // try all potential swaps
                    Job J1 = null, J2 = null;
                    for (int J1index = 0; J1index < JobsOnMachine - 1; J1index++)
                    {

                        J1 = CurrentMachine.AssignedJobs[J1index];

                        for (int J2index = J1index + 1; J2index < JobsOnMachine; J2index++)
                        {
                            J2 = CurrentMachine.AssignedJobs[J2index];
                            // try the swap
                            double OriginalFitness = FitnessFunction(CurrentSchedule);
                            if (SwapJobPair(J1, J2, CurrentMachine, CurrentSchedule))
                            {

                                double NewFitness = FitnessFunction(CurrentSchedule); // current schedule is updated during the SameMachineSwap function. So this works.
                                if (NewFitness > OriginalFitness) // Trying to minimize here, so maybe VD more appropriate than HC! This maximizes CMAX!
                                {
                                   // improvementFound = true;
                                    //  Console.WriteLine("OPTIMIZING MOVE FOUND (Swap J{0},J{1}, on M{2}). Schedule fitness: {3}",J1.ID,J2.ID,CurrentMachine.MachineID, NewFitness);
                                    //  CurrentSchedule.Print();
                                    return CurrentSchedule;
                                    // keep it
                                }
                                else
                                {
                                    // undo swap
                                    // Console.WriteLine("No improvement, undoing..");
                                    SwapJobPair(J1, J2, CurrentMachine, CurrentSchedule);
                                    //undoing should always be feasible
                                }
                            }
                        }
                    }
                }

                NMachinesTried++;
                CurrentMachineId++;
                if (CurrentMachineId > CurrentSchedule.Problem.NMachines)
                { CurrentMachineId = 1; }

            }
            //No improving schedule found
            return null;
        }

        static private bool SwapJobPair(Job J1, Job J2, Machine M, Schedule Sched)
        {

            //todo: Check feasibility of the swaps. Give each job a dictionary of all its transitive descendants. Check in almost O(1) if swap is feasible. (MUCH BETTER THAN BFS).
            int OldJ1Index = Sched.MachineArcPointers[J1.ID].ArrayIndex;
            int OldJ2Index = Sched.MachineArcPointers[J2.ID].ArrayIndex;
            int LeftIndex = OldJ1Index;
            int RightIndex = OldJ2Index;
            if (OldJ2Index < OldJ1Index) { LeftIndex = OldJ2Index; RightIndex = OldJ1Index; }
            // check feasibility of the swap, Assuming an already feasible setup.
            for (int i = LeftIndex; i <= RightIndex - 1; i++)
            {

                for (int j = i + 1; j <= RightIndex; j++)
                {
                    // Between [J1 and J2] (inlusive) the arcs get reversed. Check all combos.
                    //Do they though? TODO BUG!
                    if (Sched.PrecedenceDAG.PrecPathExists(M.AssignedJobs[i], M.AssignedJobs[j]))
                    {
                       // Console.WriteLine("BUG?");
                        Console.WriteLine("Swap J{0},J{1} on M{2} denied due to cycle from J{3} to J{4}",J1.ID,J2.ID,M.MachineID,M.AssignedJobs[i].ID,M.AssignedJobs[j].ID);
                        return false;
                    } // infeasible.
                }
            }
            Console.WriteLine("Swap J{0}, J{1} on M{2}", J1.ID, J2.ID, M.MachineID);

            //update the position of the jobs in the list.
            M.AssignedJobs[OldJ1Index] = J2;
            M.AssignedJobs[OldJ2Index] = J1;
            //update the pointers:
            Sched.MachineArcPointers[J1.ID].ArrayIndex = OldJ2Index;
            Sched.MachineArcPointers[J2.ID].ArrayIndex = OldJ1Index;

            return true;
        }

    }
}
