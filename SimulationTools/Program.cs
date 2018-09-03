using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
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
            //string INSTANCEFOLDER = string.Format(@"C:\Users\Gebruiker\Documents\UU\MSc Thesis\Code\probleminstances\"); //laptop folder
            
            BASEPATH = string.Format(@"C: \Users\3496724\Source\Repos\SimulationTools\");
            //BASEPATH = string.Format(@"C:\Users\Gebruiker\Documents\UU\MSc Thesis\Code\Simulation\SimulationTools\");
            Constants.OUTPATH = string.Format(@"{0}Results\RMs\allresults.txt", Program.BASEPATH);
            INSTANCEFOLDER = string.Format(@"{0}probleminstances\",BASEPATH);

            //
            // End of Setup
            //
            Console.WriteLine("REMOVING OLD DATA...");
            System.IO.File.Delete(Constants.OUTPATH);

            bool DEBUG = true;
            if (DEBUG)
            {
                ProblemInstance Ins = new ProblemInstance();
                Ins.InstanciateLSTest();
                //string InstanceName = "100j-100r-12m.ms";
                //Ins.ReadFromFile(string.Format(@"{0}\{1}",INSTANCEFOLDER, InstanceName), InstanceName);
                Schedule Sched = null; 
                //Make 50 Random schedules, try to improve with LS.
                for (int i = 0; i < 50; i++)
                {
                    Sched = NewSchedule(Ins, "Random", "ESS");
                    Sched.Print();
                    LocalSearch.SwapHillClimb(ref Sched, FitnessFunctions.MeanBasedCmax);
                }
                //new Simulation(50, Sched, "N(p,0.1p)").Perform();

            }
            else
            {
                INSTANCENAMES = System.IO.Directory.GetFiles(INSTANCEFOLDER);
                for (int i = 0; i < INSTANCENAMES.Length; i++)
                {
                    string[] temp = INSTANCENAMES[i].Split('\\');
                    INSTANCENAMES[i] = temp[temp.Length - 1];
                }

                Simulation[] Sims = new Simulation[Constants.NRuns];

                //For each instance, for each schedule generation heuristic, perform a simulation.
                List<Schedule> SchedulesToSimulate = new List<Schedule>();

                ProblemInstance[] Instances = new ProblemInstance[INSTANCENAMES.Length + 1];
                for (int i = 0; i < INSTANCENAMES.Length; i++)
                {
                    Instances[i] = new ProblemInstance();
                    Instances[i].ReadFromFile(INSTANCEFOLDER + INSTANCENAMES[i], INSTANCENAMES[i]);
                    SchedulesToSimulate.Add(NewSchedule(Instances[i], "RMA", "ESS"));
                    SchedulesToSimulate.Add(NewSchedule(Instances[i], "GLB", "ESS"));
                    SchedulesToSimulate.Add(NewSchedule(Instances[i], "Random", "ESS"));
                    SchedulesToSimulate.Add(NewSchedule(Instances[i], "RMA", "LSS"));
                    SchedulesToSimulate.Add(NewSchedule(Instances[i], "GLB", "LSS"));
                    SchedulesToSimulate.Add(NewSchedule(Instances[i], "Random", "LSS"));
                }

                ProblemInstance Pinedo = new ProblemInstance();
                Pinedo.InstanciatePinedo();
                Instances[INSTANCENAMES.Length] = Pinedo;
                SchedulesToSimulate.Add(NewSchedule(Pinedo, "RMA", "ESS"));
                SchedulesToSimulate.Add(NewSchedule(Pinedo, "GLB", "ESS"));
                SchedulesToSimulate.Add(NewSchedule(Pinedo, "Random", "ESS"));
                SchedulesToSimulate.Add(NewSchedule(Pinedo, "RMA", "LSS"));
                SchedulesToSimulate.Add(NewSchedule(Pinedo, "GLB", "LSS"));
                SchedulesToSimulate.Add(NewSchedule(Pinedo, "Random", "LSS"));


                /*
                            Parallel.ForEach(SchedulesToSimulate, (currentSched) =>
                            {
                               new Simulation(Nruns, currentSched).Perform();
                            });
                            */
                //     ProblemInstance Ptest = new ProblemInstance();
                //    Ptest.ReadFromFile(string.Format("{0}100j-100r-12m.ms", INSTANCEFOLDER), "test");
                //   new Simulation(100, NewSchedule(Ptest, "GLB", "LSS")).Perform();
                foreach (string distribution in Constants.DISTRIBUTION)
                {
                    foreach (Schedule currentSched in SchedulesToSimulate)
                    {
                        new Simulation(Constants.NRuns, currentSched, distribution).Perform();
                    }
                    Console.WriteLine("Finished simulations for {0} distribution", distribution);
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
                    Sched.AssignByRolling();
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
            Console.WriteLine("*** RMS CALCULATED BASED ON {0} SCHED! ***",StartTimeType);
            Sched.Print();
            Sched.EstimateCmax();
            Sched.CalcRMs();
            return Sched;
        }
    }


    
}
