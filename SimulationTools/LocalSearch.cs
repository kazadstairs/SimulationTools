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
                                SameMachineSwap(J1, J2, CurrentSchedule.Machines[CurrentMachineId], CurrentSchedule);
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
                                }

                                // if it improves keep it

                                // if it does not, reset it
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


        static private void SameMachineSwap(Job J1, Job J2, Machine M, Schedule Sched)
        {
            Console.WriteLine("Swap J{0}, J{1} on M{2}", J1.ID, J2.ID, M.MachineID);
            if (Sched.Problem.DAG.PrecPathExists(J2, J1)) { Console.WriteLine("Infeasible Swap"); }
            int OldJ1Index = Sched.MachineArcPointers[J1.ID].ArrayIndex;
            int OldJ2Index = Sched.MachineArcPointers[J2.ID].ArrayIndex;
            //update the position of the jobs in the list.
            M.AssignedJobs[OldJ1Index] = J2;
            M.AssignedJobs[OldJ2Index] = J1;
            //update the pointers:
            Sched.MachineArcPointers[J1.ID].ArrayIndex = OldJ2Index;
            Sched.MachineArcPointers[J2.ID].ArrayIndex = OldJ1Index;
        }

    }


}


