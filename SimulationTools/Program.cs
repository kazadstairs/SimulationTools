using System;
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
        static public string INSTANCEFOLDER;
        static public string BASEPATH;
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
            Constants.OUTPATH = string.Format(@"{0}Results\RMs\allresults.txt", Program.BASEPATH);
            INSTANCEFOLDER = string.Format(@"{0}probleminstances\",BASEPATH);

            //
            // End of Setup
            //
            Console.WriteLine("REMOVING OLD DATA...");
            System.IO.File.Delete(Constants.OUTPATH);

            bool DEBUG = false;
            if (DEBUG)
            {
                ProblemInstance[] Instances = AllBlokInstances();
                // string InstanceName = "30j-15r-4m.ms";
                //Ins.ReadFromFile(string.Format(@"{0}\{1}",INSTANCEFOLDER, InstanceName), InstanceName);

                List<Schedule> SchedulesToSimulate = new List<Schedule>();
                foreach (ProblemInstance Ins in Instances)
                {
                    SchedulesToSimulate.Add(new Schedule(Ins));
                    SchedulesToSimulate[SchedulesToSimulate.Count - 1].MakeScheduleForBlockInstance();
                }
                //SchedulesToSimulate.Add(LocalSearch.MLS(10, Ins, "Random", FitnessFunctions.MeanBasedCmax, NeighborhoodFunctions.VNHC));


                foreach (Schedule currentSched in SchedulesToSimulate)
                {
                    currentSched.CalcRMs();
                    currentSched.MakeHTMLImage(currentSched.AssignmentDescription);
                    currentSched.CreateDotFile();
                    foreach (string distribution in Constants.DISTRIBUTION)
                    {
                        new Simulation(Constants.NRuns, currentSched, distribution).Perform();
                    }

                }

                //Schedule TestSched = NewSchedule(Ins, "Random", "ESS");

                //Schedule MLSSched = LocalSearch.MLS(40, Ins, "Random", FitnessFunctions.MeanBasedCmax, NeighborhoodFunctions.VNHC);
                //MLSSched.CalcLSS();
                //MLSSched.MakeHTMLImage("DebugMLS");
                //LocalSearch.MastrolilliHC(TestSched, FitnessFunctions.MeanBasedCmax);
                
                //MLSSched.PrintJobInfo();
               // MLSSched.MakeHTMLImage("DebugMastrolilliMLSfor30J");
                
        //       Schedule MLSOpt = LocalSearch.MLS(200, Ins,Schedule.MakeGreedyLoadAssignment, FitnessFunctions.MeanBasedCmax, NeighborhoodFunctions.NeighborSwaps);

            }
            else
            {
                throw new NotImplementedException() // bug when the instancs are made. Something silly like reading in the box cases.
                INSTANCENAMES = System.IO.Directory.GetFiles(INSTANCEFOLDER);
                for (int _i = 0; _i < INSTANCENAMES.Length; _i++)
                {
                    string[] temp = INSTANCENAMES[_i].Split('\\');
                    INSTANCENAMES[_i] = temp[temp.Length - 1];
                }

                Simulation[] Sims = new Simulation[Constants.NRuns];

                //For each instance, for each schedule generation heuristic, perform a simulation.
                List<Schedule> SchedulesToSimulate = new List<Schedule>();

                ProblemInstance[] Instances = new ProblemInstance[INSTANCENAMES.Length + 1];
                  for (int i = 0; i < INSTANCENAMES.Length; i++)
                  {
                //int i = 8;
                    Instances[i] = new ProblemInstance();
                    Instances[i].ReadFromFile(INSTANCEFOLDER + INSTANCENAMES[i], INSTANCENAMES[i]);
                SchedulesToSimulate.Add(LocalSearch.MLS(5, Instances[i], "Random", FitnessFunctions.MeanBasedCmax, NeighborhoodFunctions.VNHC));
                SchedulesToSimulate.Add(LocalSearch.MLS(20, Instances[i], "Random", FitnessFunctions.MeanBasedCmax, NeighborhoodFunctions.VNHC));

                SchedulesToSimulate.Add(LocalSearch.MLS(80, Instances[i], "Random", FitnessFunctions.MeanBasedCmax, NeighborhoodFunctions.VNHC));

                    SchedulesToSimulate.Add(LocalSearch.MLS(200, Instances[i], "Random", FitnessFunctions.MeanBasedCmax, NeighborhoodFunctions.VNHC));

                SchedulesToSimulate.Add(LocalSearch.MLS(500, Instances[i], "Random", FitnessFunctions.MeanBasedCmax, NeighborhoodFunctions.VNHC));

                //          SchedulesToSimulate.Add(NewSchedule(Instances[i], "RMA", "LSS"));
                //          SchedulesToSimulate.Add(NewSchedule(Instances[i], "GLB", "LSS"));
                //          SchedulesToSimulate.Add(NewSchedule(Instances[i], "Random", "LSS"));
                                }


                /*ProblemInstance Pinedo = new ProblemInstance();
                Pinedo.InstanciatePinedo();
                Instances[INSTANCENAMES.Length] = Pinedo;
                SchedulesToSimulate.Add(NewSchedule(Pinedo, "RMA", "ESS"));
                SchedulesToSimulate.Add(NewSchedule(Pinedo, "GLB", "ESS"));
                SchedulesToSimulate.Add(NewSchedule(Pinedo, "Random", "ESS"));
                Schedule PinedoMLS = LocalSearch.MLS(200, Pinedo, FitnessFunctions.MeanBasedCmax, NeighborhoodFunctions.NeighborSwaps);
                SchedulesToSimulate.Add(PinedoMLS);
                PinedoMLS.EstimateCmax();
                */
                //       SchedulesToSimulate.Add(NewSchedule(Pinedo, "RMA", "LSS"));
                //       SchedulesToSimulate.Add(NewSchedule(Pinedo, "GLB", "LSS"));
                //       SchedulesToSimulate.Add(NewSchedule(Pinedo, "Random", "LSS"));


                /*
                            Parallel.ForEach(SchedulesToSimulate, (currentSched) =>
                            {
                               new Simulation(Nruns, currentSched).Perform();
                            });
                            */
                //     ProblemInstance Ptest = new ProblemInstance();
                //    Ptest.ReadFromFile(string.Format("{0}100j-100r-12m.ms", INSTANCEFOLDER), "test");
                //   new Simulation(100, NewSchedule(Ptest, "GLB", "LSS")).Perform();
                foreach (Schedule currentSched in SchedulesToSimulate)
               {
                    Console.WriteLine("******* Starting all distribution sims for SCHED {0}", currentSched.AssignmentDescription);
                    currentSched.CalcRMs();
                    foreach (string distribution in Constants.DISTRIBUTION)
                    {                        
                        new Simulation(Constants.NRuns, currentSched, distribution).Perform();
                    }
                    Console.WriteLine("******* Finished all distributions for SCHED {0}",currentSched.AssignmentDescription);
                }


            }


            Console.WriteLine("All operations complete. Output written to:");
            Console.WriteLine("{0}",Constants.OUTPATH);
            Console.ReadLine();
        }

        //************************ END OF MAIN *******************************************************************//


        // todo: Expand to allow chooosing of starttime decissions.
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
