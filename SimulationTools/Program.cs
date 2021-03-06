﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SimulationTools
{
//newmachine Git commit test
    class Program
    {
        static public string INSTANCEFOLDER; // path to problem instance folder
        static public string BASEPATH; // Path to base directory
        static public string SCHEDULEFOLDER; // Path to Schedule Folder
        static public string [] INSTANCENAMES;



        static void Main(string[] args)
        {

            Console.WriteLine("Welcome to this Super Sweet Simulation Software");
            //
            // SETUP
            //

            // BASEPATH = string.Format(@"C:\Users\TEMP\Source\Repos\SimulationTools\"); //Geo Landschap
            //  BASEPATH = string.Format(@"C: \Users\3496724\Source\Repos\SimulationTools\"); //KBG pcs
            BASEPATH = string.Format(@"C:\Users\Gebruiker\Documents\UU\MSc Thesis\Code\Simulation\SimulationTools\"); //Laptop

            Constants.ALLRESULTSPATH = string.Format(@"{0}Results\RMs\{1}", Program.BASEPATH,Settings.RESULTSFILENAME);
            INSTANCEFOLDER = string.Format(@"C: \Users\Gebruiker\Documents\UU\MSc Thesis\Code\probleminstances\"); //string.Format(@"{0}probleminstances\", BASEPATH);
            SCHEDULEFOLDER = string.Format(@"{0}\Results\Schedules\", BASEPATH);

            //
            // End of Setup
            //

            


            if (!Settings.DEBUG)
            {
                Console.WriteLine("Starting Main Execution");
                ProblemInstance[] Instances = SelectProblemInstances();
                Console.WriteLine("Instances Loaded...");

                if (Settings.PERFORM_MLS)
                {
                    Console.WriteLine("Performing MLS{0} on {1} instances, to wit:", Settings.MLS_RUNS, Instances.Length);
                    foreach (ProblemInstance PI in Instances)
                    {
                        Console.WriteLine(PI.Description);
                    }
                    Console.WriteLine();
                    foreach (ProblemInstance PI in Instances)
                    {
                        Console.WriteLine("Performing MLS{0} on PI {1}...", Settings.MLS_RUNS, PI.Description);
                        Func<Schedule, double> fitnessfunction;
                        switch (Settings.MLS_HF)
                        {
                            case "DetCmax":
                                fitnessfunction = FitnessFunctions.MeanBasedCmax;
                                break;
                            case "NormalApproxCmax":
                                fitnessfunction = FitnessFunctions.NormalApproxCmax;
                                break;
                            default:
                                throw new Exception(string.Format("Settings.MLS_HF ({0}) did not match pattern \"DetCmax\" or \"NormalApproxCmax\"", Settings.MLS_HF));
                        }
                        List<Schedule> Top_MLS_Scheds = LocalSearch.MLS(Settings.MLS_RUNS, Settings.MLS_SCHEDS, PI, "Random", fitnessfunction, NeighborhoodFunctions.VNHC);
                        Console.WriteLine("Completed MLS{0} on PI {1}, writing to File...", Settings.MLS_RUNS, PI.Description);
                        LocalSearch.WriteScheduleListToFile(Top_MLS_Scheds, Settings.MLS_RUNS, Settings.MLS_HF);
                        Console.WriteLine("Completed MLS{0} on PI {1}, written to file.", Settings.MLS_RUNS, PI.Description);
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("All MLS runs complete.");
                    Console.WriteLine();
                    Console.WriteLine();
                }

                if (Settings.PERFORM_SIMULATIONS)
                {
                    Console.WriteLine("REMOVING OLD DATA...");
                    System.IO.File.Delete(Constants.ALLRESULTSPATH);

                    Console.WriteLine("Performing Simulations of {0} runs on {1} instances with {2} schedules each. Using {3} distributions. Total RUNS: {4}",
                        Settings.N_SIM_RUNS,
                        Instances.Length,
                        Settings.MLS_SCHEDS,
                        Settings.DISTRIBUTION.Length,
                        Settings.N_SIM_RUNS * Instances.Length * Settings.MLS_SCHEDS * Settings.DISTRIBUTION.Length);
                    Console.WriteLine("PIs:");
                    foreach (ProblemInstance PI in Instances)
                    {
                        Console.WriteLine(PI.Description);
                    }
                    Console.WriteLine("Distros:");
                    foreach (string Distr in Settings.DISTRIBUTION)
                    {
                        Console.WriteLine(Distr);
                    }

                    int PIcounter = 0;
                    foreach(ProblemInstance PI in Instances)
                    {
                        Console.WriteLine("Starting Sims for PI: {0}", PI.Description);
                        Console.WriteLine("CALCULATING RM MEANS instead of sums!");

                        Console.Write("Reading schedules from file...");
                        List<Schedule> Top_MLS_Scheds = LocalSearch.ReadMLSSchedsFor(PI, Settings.MLS_AH, Settings.MLS_RUNS, Settings.MLS_SCHEDS, Settings.MLS_HF, false);

                        Console.WriteLine("   ... Completed.");
                        int SchedID = 0;
                        foreach (Schedule CurrentSched in Top_MLS_Scheds)
                        {
                            CurrentSched.AssignmentDescription += string.Format("MLSID{0}", SchedID);
                            Console.WriteLine("Starting Simulations for Sched with id: {0}", SchedID);
                            foreach (string distribution in Settings.DISTRIBUTION)
                            {
                                new Simulation(Settings.N_SIM_RUNS, CurrentSched, distribution).Perform();
                            }
                            SchedID++;
                        }
                        PIcounter++;
                        Console.WriteLine("Completed Sims for PI: {0} ({1}/{2})", PI.Description, PIcounter, Instances.Length);
                    }
                    Console.WriteLine("All Simulation operations complete. Output written to:");
                    Console.WriteLine("{0}", Constants.ALLRESULTSPATH);
                }
                if (Settings.PAUSE_ON_COMPLETION) { Console.WriteLine("Press Any Key to Exit..."); Console.ReadKey(); }
            }
            else
            {
               // ProblemInstance Pinedo = new ProblemInstance();
               // Pinedo.InstanciatePinedo();
               // Schedule TestSched = new Schedule(Pinedo);
               // TestSched.PinedoSchedule();
               // RobustnessMeasures.NormalBasedEstimatedCmax(TestSched, 0.3);
                // DEBUG MODE HERE
                ProblemInstance[] Instances = AllBlokInstances();


                List<Schedule> SchedulesToSimulate = new List<Schedule>();
                foreach (ProblemInstance Ins in Instances)
                {
                    SchedulesToSimulate.Add(new Schedule(Ins));
                    SchedulesToSimulate[SchedulesToSimulate.Count - 1].MakeScheduleForBlockInstance();
                }


                foreach (Schedule currentSched in SchedulesToSimulate)
                {
                    currentSched.CalcRMs();
                    //currentSched.MakeHTMLImage(currentSched.AssignmentDescription);
                    //currentSched.CreateDotFile();
                    foreach (string distribution in Settings.DISTRIBUTION)
                    {
                        new Simulation(Settings.N_SIM_RUNS, currentSched, distribution).Perform();
                    }

                }

            }

        }
        //************************ END OF MAIN *******************************************************************//

        static public void TestEdit(int[] T)
        {
            T[2] = 42;
        }
        static Schedule NewSchedule(ProblemInstance Ins,string AssignmentType,string StartTimeType)
        {
            Schedule Sched = new Schedule(Ins);
            switch (AssignmentType)
            {
                case "Random":
                    Sched.MakeRandomAssignment();
                    break;
                case "RMA":
                    Sched.MakeRollingMachineAssignment();
                    break;
                case "GLB":
                    Sched.MakeGreedyLoadAssignment();
                    break;
                default:
                    throw new Exception("AssignmentType string not one of allowed strings");
            }
            Sched.StartTimeDescription = StartTimeType;
            switch (StartTimeType)
            {
                case "ESS":
                    Sched.CalcESS();
                    Sched.SetESS();
                    Sched.MakeHTMLImage(string.Format("ESS {0} schedule for {1}", Sched.AssignmentDescription, Ins.Description));
                    break;
                case "LSS":
                    Sched.CalcLSS();
                    Sched.SetLSS();
                    Sched.MakeHTMLImage(string.Format("LSS {0} schedule for {1}", Sched.AssignmentDescription, Ins.Description));
                    break;
                default:
                    throw new Exception("StartTimeType string not one of the allowed strings");



            }
            Sched.EstimateCmax();
     //       LocalSearch.MLS(200, Sched.Problem, FitnessFunctions.MeanBasedCmax, NeighborhoodFunctions.NeighborSwaps);
            return Sched;
        }

        static ProblemInstance[] SelectProblemInstances()
        {
            ProblemInstance[] Instances;
            if (Settings.PROBLEM_INSTANCES_TO_USE.Length > 0)
            {
                // Load instance names from the files
              //  INSTANCENAMES = System.IO.Directory.GetFiles(INSTANCEFOLDER);
              //  for (int _i = 0; _i < INSTANCENAMES.Length; _i++)
              //  {
              //      string[] temp = INSTANCENAMES[_i].Split('\\');
              //      INSTANCENAMES[_i] = temp[temp.Length - 1];
              //  }

                Instances = new ProblemInstance[Settings.PROBLEM_INSTANCES_TO_USE.Length];

                //int InstancesFilledCounter = 0;
                for (int PI_id = 0; PI_id < Settings.PROBLEM_INSTANCES_TO_USE.Length; PI_id++)
                {
                    Instances[PI_id] = new ProblemInstance();
                    Instances[PI_id].ReadFromFile(INSTANCEFOLDER + Settings.PROBLEM_INSTANCES_TO_USE[PI_id], Settings.PROBLEM_INSTANCES_TO_USE[PI_id]);

                }
                /*
                for (int i = 0; i < Settings.PROBLEM_INSTANCES_TO_USE.Length + 1; i++)
                {
                    foreach (int ID in Settings.PI_IDS_TO_USE)
                    {
                        if (i == ID)
                        {
                            Instances[InstancesFilledCounter] = new ProblemInstance();
                            Instances[InstancesFilledCounter].ReadFromFile(INSTANCEFOLDER + INSTANCENAMES[i], INSTANCENAMES[i]);
                            InstancesFilledCounter++;
                        }
                    }
                }
                */
            }
            else
            {
                //Load block instances
                Instances = AllBlokInstances();
            }
            return Instances;
        }

        static ProblemInstance[] AllBlokInstances()
        {            
            ProblemInstance[] Instances = new ProblemInstance[12];
            Instances[0] = new ProblemInstance();
            Instances[0].Instanciate1CycleP10Blok();
            Instances[1] = new ProblemInstance();
            Instances[1].Instanciate1CycleP1Blok();
            Instances[2] = new ProblemInstance();
            Instances[2].Instanciate4CycleP10Blok();
            Instances[3] = new ProblemInstance();
            Instances[3].Instanciate4CycleP1Blok();
            Instances[4] = new ProblemInstance();
            Instances[4].InstanciateFullP10Blok();
            Instances[5] = new ProblemInstance();
            Instances[5].InstanciateFullP1Blok();
            Instances[6] = new ProblemInstance();
            Instances[6].InstanciateDiamondP10Blok();
            Instances[7] = new ProblemInstance();
            Instances[7].InstanciateDiamondP1Blok();
            Instances[8] = new ProblemInstance();
            Instances[8].InstanciateNoInterMachineP10Blok();
            Instances[9] = new ProblemInstance();
            Instances[9].InstanciateNoInterMachineP1Blok();
            Instances[10] = new ProblemInstance();
            Instances[10].InstanciateRollingDiamondP10Blok();
            Instances[11] = new ProblemInstance();
            Instances[11].InstanciateRollingDiamondP1Blok();

            return Instances;

        }
    }


    
}
