using System;
using System.Collections.Generic;
using System.IO;
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
        static public List<Schedule> MLS(int NRuns,int NBestSchedules,ProblemInstance Prob,string AssignmentType, Func <Schedule,double> FitnessFunction, Func<Schedule,Func<Schedule,double>,Schedule> NeighborhoodOperator)
        {
            Console.WriteLine("Running MLS {0} for schedules with assingment type {1}...", NRuns, AssignmentType);
            List<Schedule> Top_N_Schedules = new List<Schedule>(NBestSchedules);
            //double BestFitness = -double.MaxValue;
            double CurrentFitness = -double.MaxValue;
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
                if (!CurrentSchedule.IsCritical(CurrentSchedule.PrecedenceDAG.GetJobById(0)))
                {
                    throw new Exception("Weird stuff");
                }
                CurrentFitness = FitnessFunction(CurrentSchedule);
                CurrentSchedule.LSFitness = CurrentFitness;

                //insert into list of best schedules if applicable
                int InsertPosition = Top_N_Schedules.Count;
                for (int listindex = Top_N_Schedules.Count-1; listindex >= 0; listindex--)
                {
                    //move the current schedule left until its fitness equals or is worse than that of its left neighbor
                    if (Top_N_Schedules[listindex].LSFitness < CurrentFitness)
                    {
                        InsertPosition--;
                    }
                    else
                    {
                        break;
                    }
                }
                //if the new schedule is one of the best NBest, add it to the list:
                if (InsertPosition < NBestSchedules)
                {
                    Top_N_Schedules.Insert(InsertPosition, CurrentSchedule); //insert does not remove.
                    //Remove the last schedule if list overflows
                    if (Top_N_Schedules.Count > NBestSchedules) { Top_N_Schedules.RemoveAt(Top_N_Schedules.Count - 1); } 
                } 
                /*
                if (CurrentFitness > BestFitness)
                {
                    Console.WriteLine("MLS opt updated. New fitness: {0}",CurrentFitness);
                    BestFitness = CurrentFitness;
                    MLSOptimumSched = new Schedule(CurrentSchedule); // create a copy
                }*/
                if (i % 20 == 0) { Console.WriteLine("MLS at {0}/{1} ", i, NRuns); }
                Console.Write(".");
            }
            
            Console.WriteLine("MLS{0} optimal schedule with fitness {1}(check {2}):", NRuns, Top_N_Schedules[0].LSFitness, FitnessFunction(Top_N_Schedules[0]));
            Console.WriteLine("Nruns: {0}, List.Count: {1}, NBest: {2}",NRuns,Top_N_Schedules.Count,NBestSchedules);

            Top_N_Schedules[0].Print();
            return Top_N_Schedules;
            //return the best
        }

        public static void WriteScheduleListToFile(List<Schedule> ListToWrite, int Nruns, string HeuristicFunctionName)
        {
            string FileName = String.Format("MLSSCheds_For_{0}_{1}_Top_{3}{4}.txt",
                    ListToWrite[0].Problem.Description,
                    ListToWrite[0].AssignmentDescription,
                    Nruns,
                    ListToWrite.Count,
                    HeuristicFunctionName);

            string path = String.Format("{0}{1}", Program.SCHEDULEFOLDER, FileName);      
            
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("{0} {1} {2} {3} {4}",
                    ListToWrite[0].Problem.Description,
                    ListToWrite[0].AssignmentDescription,
                    Nruns,
                    ListToWrite.Count,
                    HeuristicFunctionName);
                foreach (Schedule S in ListToWrite)
                {
                    sw.WriteLine(S.LSFitness);
                    foreach (Machine m in S.Machines)
                    {
                        sw.Write("{0} ", m.MachineID);
                        foreach (Job j in m.AssignedJobs)
                        {
                            sw.Write("{0} ", j.ID);
                        }
                        sw.Write(Environment.NewLine);
                    }
                }
            }
        }

        public static List<Schedule> ReadMLSSchedsFor(ProblemInstance PI, string AH, int Nruns, int NBest, string HF)
        {
            // check if file exists:
            string FileName = String.Format("MLSSCheds_For_{0}_{1}_Top_{3}{4}.txt",
                    PI.Description,
                    string.Format("{0}MLS{1}",AH,Nruns),
                    Nruns,
                    NBest,
                    HF);
            string path = String.Format("{0}{1}", Program.SCHEDULEFOLDER, FileName);
            using (StreamReader rf = new StreamReader(File.OpenRead(path)))
            {
                //check Params:
                string[] Params = rf.ReadLine().Split();
                if (Params[0] == PI.Description &&
                    Params[1] == (AH + "MLS" + Nruns) &&
                    int.Parse(Params[2]) == Nruns &&
                    int.Parse(Params[3]) == NBest &&
                    Params[4] == HF)
                {
                    //Loaded correct file
                    int CurrentMachineID;
                    List<Schedule> MLSScheduleList = new List<Schedule>(NBest);
                    for (int i = 0; i < NBest; i++)
                    {
                        Schedule CurrentSchedule = new Schedule(PI);
                        CurrentSchedule.LSFitness = double.Parse(rf.ReadLine());
                        for (int j = 0; j < PI.NMachines; j++)
                        {
                            string[] Line = rf.ReadLine().Split();
                            CurrentMachineID = int.Parse(Line[0]);
                            for (int k = 1; k < Line.Length - 1; k++)
                            {
                                CurrentSchedule.AssignJobToMachineById(int.Parse(Line[k]), CurrentMachineID);
                            }
                        }
                        CurrentSchedule.AssignmentDescription = AH;
                        CurrentSchedule.CalcESS();
                        CurrentSchedule.SetESS();
                        CurrentSchedule.CalcRMs();
                        MLSScheduleList.Add(CurrentSchedule);
                    }

                    return MLSScheduleList;
                }
                else
                {
                    Console.WriteLine("ERROR: File Parameters do not match file name for file:");
                    Console.WriteLine(Params[0] == PI.Description);
                    Console.WriteLine(Params[1] == (AH+"MLS"+Nruns));
                    Console.WriteLine(int.Parse(Params[2]) == Nruns);
                    Console.WriteLine(int.Parse(Params[3]) == NBest);
                    Console.WriteLine(Params[4] == HF);
                    Console.WriteLine(path);
                    throw new Exception("File Paramaters do not match file name!");
                }
            }


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
                    //DEBUG: I Calc LSS here to get all the ESS and LSS times set up correctly.
                    CurrentSchedule.CalcLSS();
                    Debug_numberofLocalOptima++;
                    LocalOptimum = false;
                }
                
            }  return CurrentSchedule;
        }

        
        /// <summary>
        /// If feasible, will perform a swap and return true. If not feasible will not perform sawp. Does not consider fitness.
        /// </summary>
        /// <param name="J1">Left Job to be swapped</param>
        /// <param name="J2">Right Job to be swapped</param>
        /// <param name="M">Machine to swap on</param>
        /// <param name="Sched">Schedule that is being updated</param>
        /// <returns></returns>
 
/*
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
*/
    }


}


