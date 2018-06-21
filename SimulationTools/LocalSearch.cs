using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class LocalSearch
    {
        static public Schedule SwapHillClimb(ref Schedule Original, Func<Schedule, double> FitnessFunction)
        {
            Schedule CurrentSchedule = Original;
            bool LocalOptimum = false;

            Console.WriteLine("DEBUG: Starting LS *********************");
            Console.WriteLine("Original Schedule with fitness {0}", FitnessFunction(CurrentSchedule));
            CurrentSchedule.Print();

            while (!LocalOptimum)
            {
                int StartMachineId = Distribution.UniformInt(CurrentSchedule.Problem.NMachines);
                int CurrentMachineId = StartMachineId;
                int NMachinesTried = 0;
                bool improvementFound = false;
                while (!improvementFound && NMachinesTried < CurrentSchedule.Problem.NMachines)
                {
                    int JobsOnMachine = CurrentSchedule.Machines[CurrentMachineId].AssignedJobs.Count;
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
                            if (improvementFound) break;

                            J1 = CurrentSchedule.Machines[CurrentMachineId].AssignedJobs[J1index];

                            for (int J2index = J1index + 1; J2index < JobsOnMachine; J2index++)
                            {
                                J2 = CurrentSchedule.Machines[CurrentMachineId].AssignedJobs[J2index];
                                // try the swap
                                double OriginalFitness = FitnessFunction(CurrentSchedule);
                                if (SameMachineSwap(J1, J2, CurrentSchedule.Machines[CurrentMachineId], CurrentSchedule))
                                {
                                    double NewFitness = FitnessFunction(CurrentSchedule);
                                    if (NewFitness > OriginalFitness)
                                    {
                                        improvementFound = true;
                                        Console.WriteLine("OPTIMIZING MOVE FOUND, resulting schedule with fitness {0} is:", NewFitness);
                                        CurrentSchedule.Print();

                                        break;
                                        // keep it
                                    }
                                    else
                                    {
                                        // undo swap
                                        Console.WriteLine("No improvement, undoing..");
                                        SameMachineSwap(J1, J2, CurrentSchedule.Machines[CurrentMachineId], CurrentSchedule);
                                        //undoing should always be feasible
                                    }
                                }
                            }
                        }
                    }

                    NMachinesTried++;
                    CurrentMachineId++;
                    if (CurrentMachineId == CurrentSchedule.Problem.NMachines) { CurrentMachineId = 0; }


                }// end of While over machines
                // all possible swaps tried on all possible machines, no improvement found.
                LocalOptimum = true;


                // update the number of tries and move to next machine, which may require resetting index to 0 if it goes out of bounds
                
            }

            return CurrentSchedule;
        }

        /// <summary>
        /// If feasible, will perform a swap and return true. If not feasible will not perform sawp. Does not consider fitness.
        /// </summary>
        /// <param name="J1">Left Job to be swapped</param>
        /// <param name="J2">Right Job to be swapped</param>
        /// <param name="M">Machine to swap on</param>
        /// <param name="Sched">Schedule that is being updated</param>
        /// <returns></returns>
        static private bool SameMachineSwap(Job J1, Job J2, Machine M, Schedule Sched)
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
                
                for (int j = i+1; j <= RightIndex; j++)
                {
                    // Between [J1 and J2] (inlusive) the arcs get reversed. Check all combos.
                    if (Sched.PrecedenceDAG.PrecPathExists(M.AssignedJobs[i],M.AssignedJobs[j])) { return false; } // infeasible.
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


