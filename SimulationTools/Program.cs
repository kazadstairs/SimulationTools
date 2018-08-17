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
            string[] output = new string [] { "hello", "world" };
            //
            // SETUP
            //
            int Nruns = 10;
            //string INSTANCEFOLDER = string.Format(@"C:\Users\Gebruiker\Documents\UU\MSc Thesis\Code\probleminstances\"); //laptop folder
            INSTANCEFOLDER = string.Format(@"C: \Users\3496724\Source\Repos\SimulationTools\probleminstances\");
            BASEPATH = string.Format(@"C: \Users\3496724\Source\Repos\SimulationTools\");

            //
            // End of Setup
            //

            INSTANCENAMES = System.IO.Directory.GetFiles(INSTANCEFOLDER);
            for (int i = 0; i < INSTANCENAMES.Length; i++)
            {
                string[] temp = INSTANCENAMES[i].Split('\\');
                INSTANCENAMES[i] = temp[temp.Length-1];
            }

            Simulation [] Sims = new Simulation[Nruns];

            //For each instance, for each schedule generation heuristic, perform a simulation.
            List<Schedule> SchedulesToSimulate = new List<Schedule>();

            ProblemInstance[] Instances = new ProblemInstance[INSTANCENAMES.Length+1];
            for (int i = 0; i < INSTANCENAMES.Length; i++)
            {
                Instances[i] = new ProblemInstance();
                Instances[i].ReadFromFile(INSTANCEFOLDER+INSTANCENAMES[i],INSTANCENAMES[i]);
                SchedulesToSimulate.Add(NewSchedule(Instances[i], "RMA"));
                SchedulesToSimulate.Add(NewSchedule(Instances[i], "GLB"));
                SchedulesToSimulate.Add(NewSchedule(Instances[i], "Random"));
            }

            ProblemInstance Pinedo = new ProblemInstance();
            Pinedo.InstanciatePinedo();
            Instances[INSTANCENAMES.Length] = Pinedo;
            SchedulesToSimulate.Add(NewSchedule(Pinedo, "RMA"));
            SchedulesToSimulate.Add(NewSchedule(Pinedo, "GLB"));
            SchedulesToSimulate.Add(NewSchedule(Pinedo, "Random"));

            
          
            /*
            SchedulesToSimulate.Add(Sched);
            */
            /*
            for (int i = 0; i < 10; i++)
            {
                Schedule LSSched = NewSchedule(Pinedo, "Random");
                LocalSearch.SwapHillClimb(ref LSSched, RobustnessMeasures.DebugNumberOfJobsInIndexOrder);
            }
            */
            //Console.WriteLine(RobustnessMeasures.SumOfFreeSlacks(Sched));


            //Sched.SetDeadlines(Sched.EstimatedCmax);
            //Sched.SetLSS();
            //Sched.MakeHTMLImage("Nonoptimal LSS schedule for Pinedo Instance");
            //SchedulesToSimulate.Add(Sched);

/*
            Parallel.ForEach(SchedulesToSimulate, (currentSched) =>
            {
               new Simulation(Nruns, currentSched).Perform();
            });
            */
            foreach (Schedule currentSched in SchedulesToSimulate)
            {
                new Simulation(Nruns, currentSched).Perform();
            }







            //PinedoSched.SetReleaseDates();
            //PinedoSched.SetDeadlines(32);




            //Console.WriteLine("All operations complete.");
            //Console.ReadLine();
        }

        // todo: Expand to allow chooosing of starttime decissions.
        static Schedule NewSchedule(ProblemInstance Ins,string AssignmentType)
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
            Sched.Print();
            Sched.CalcESS();
            Sched.SetESS();
            Sched.EstimateCmax();
            Sched.CalcRMs();
            Sched.MakeHTMLImage(string.Format("ESS {0} schedule for {1}",Sched.Description,Ins.Description) );
            return Sched;
        }
    }


    
}
