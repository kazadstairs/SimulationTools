using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class LocalSearch
    {
        /// <summary>
        /// Returns the best schedule found after creating and performing LS on NRun schedules.
        /// </summary>
        /// <param name="NRuns">Number of schedules to create and improve to LO</param>
        /// <param name="Prob">Problem Instance</param>
        /// <param name="MakeAssignment">Method by which to create a new Schedule</param>
        /// <param name="FitnessFunction"></param>
        /// <param name="NeighborhoodOperator"></param>
        /// <returns></returns>
        static public Schedule MLS(int NRuns,ProblemInstance Prob,string AssignmentType, Func <Schedule,double> FitnessFunction, Func<Schedule,Func<Schedule,double>,Schedule> NeighborhoodOperator)
        {
            Console.WriteLine("Running MLS {0} for schedules with assingment type {1}...", NRuns, AssignmentType);
            double BestFitness = -double.MaxValue;
            double CurrentFitness = -double.MaxValue;
            Schedule MLSOptimumSched = null;
            for (int i = 0; i < NRuns; i++)
            {
                // create random solution
                Schedule CurrentSchedule = new Schedule(Prob);
                switch (AssignmentType)
                {
                    case "Random":
                        CurrentSchedule.MakeRandomAssignment();
                        break;
                    case "GLB":
                        CurrentSchedule.MakeGreedyLoadAssignment();
                        break;
                    case "RMA":
                        CurrentSchedule.MakeRollingMachineAssignment();
                        break;
                    default:
                        throw new Exception("AssignmentType not recognized");
                }
                CurrentSchedule.CalcESS();
                CurrentSchedule.SetESS();

                //optimize it
                HillClimb(CurrentSchedule, NeighborhoodOperator, FitnessFunction);
                CurrentFitness = FitnessFunction(CurrentSchedule);

                if (CurrentFitness > BestFitness)
                {
                    Console.WriteLine("MLS opt updated");
                    BestFitness = CurrentFitness;
                    MLSOptimumSched = new Schedule(CurrentSchedule); // create a copy
                }
                if (i % 20 == 0) { Console.WriteLine("MLS at {0}/{1}", i, NRuns); }
            }

            Console.WriteLine("MLS{0} optimal schedule with fitness {1}(check {2}):", NRuns, BestFitness, FitnessFunction(MLSOptimumSched));
            MLSOptimumSched.Print();
            return MLSOptimumSched;
            //return the best
        }

        static public Schedule NeighborSwapHillClimb(Schedule Original, Func<Schedule,double> FitnessFunction)
        {
            return HillClimb(Original, NeighborhoodFunctions.NeighborSwaps, FitnessFunction);
        }

        static public Schedule MastrolilliHC(Schedule Original, Func<Schedule, double> FitnessFunction)
        {
            return HillClimb(Original, NeighborhoodFunctions.RemoveAndReinstert, FitnessFunction);
        }


        static public Schedule HillClimb(Schedule Original, Func<Schedule,Func<Schedule,double>,Schedule> ExploreNeighborhood, Func<Schedule,double> FitnessFunction)
        {
            Schedule CurrentSchedule = new Schedule(Original);
            Schedule ImprovedSchedule = null;
            bool LocalOptimum = false;
            double CurrentFitness = -double.MaxValue;
            
       //     Console.WriteLine("Starting HillClimb from fitness = {0}...",FitnessFunction(Original));
      

            //Pick random job and or random machine:
            int Debug_numberofLocalOptima = 0;
            while (!LocalOptimum)
            {
                CurrentFitness = FitnessFunction(CurrentSchedule);
                
                ImprovedSchedule = ExploreNeighborhood(CurrentSchedule,FitnessFunction); // Loop through all neighbours, return null if no improvement, Schedule if improvement found
                

                if (ImprovedSchedule == null)
                {
                    //All neighbours explored without improvement: Local Optimum
                    LocalOptimum = true;
                }
                else
                {
                    // Continue Search from the better schedule
                    CurrentSchedule = ImprovedSchedule;
                    Debug_numberofLocalOptima++;
                    LocalOptimum = false;
                }
                Debug_numberofLocalOptima++;
                
            }
       //     Console.WriteLine("...Found LO with fitness {0} in {1} improvements", FitnessFunction(CurrentSchedule), Debug_numberofLocalOptima);
            return CurrentSchedule;
        }

        

      /*  static private void SameMachineNeighbourhood(Sched CurrentSched, int JobIndex, int MachineIndex)
        {
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

        }*/

        /// <summary>
        /// If feasible, will perform a swap and return true. If not feasible will not perform sawp. Does not consider fitness.
        /// </summary>
        /// <param name="J1">Left Job to be swapped</param>
        /// <param name="J2">Right Job to be swapped</param>
        /// <param name="M">Machine to swap on</param>
        /// <param name="Sched">Schedule that is being updated</param>
        /// <returns></returns>
 

        static private bool ToMachineAt(Job J, Machine NewM, int JindexOnM, Schedule Sched)
        {
            // check feasibility:
            for (int i = JindexOnM; i < NewM.AssignedJobs.Count; i++)
            {
                if (Sched.PrecedenceDAG.PrecPathExists(Sched.PrecedenceDAG.GetJobById(i), J)) { return false; }
            }

            Sched.DeleteJobFromMachine(J);
            Sched.InsertJobOnMachineAtIndex(J, NewM, JindexOnM);

            return true;

        }

    }


}


