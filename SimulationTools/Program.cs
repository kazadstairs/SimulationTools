﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine("Welcome to this Super Sweet Simulation Software");

            //
            // SETUP
            //
            int Nruns = 1000;
            //string INSTANCEFOLDER = string.Format(@"C:\Users\Gebruiker\Documents\UU\MSc Thesis\Code\probleminstances\"); //laptop folder
            string INSTANCEFOLDER = string.Format(@"C: \Users\3496724\Source\Repos\SimulationTools\probleminstances\");
            //
            // End of Setup
            //

            Simulation [] Sims = new Simulation[Nruns];
            ProblemInstance Pinedo = new ProblemInstance();
            Pinedo.InstanciatePinedo();

            ProblemInstance ProbIns = new ProblemInstance();
            string instancename = "30j-15r-4m.ms";
            ProbIns.ReadFromFile(INSTANCEFOLDER+instancename);

            
            Schedule Sched = new Schedule(ProbIns);
            Sched.AssignByRolling();
            Sched.Print();
            Sched.SetReleaseDates();
            Sched.SetESS();

            Sched.EstimateCmax();
            Sched.MakeHTMLImage(string.Format("Nonoptimal ESS schedule for {0} Instance",instancename));

            /*
            SchedulesToSimulate.Add(Sched);
            */
            for (int i = 0; i < 10; i++)
            {
                Schedule LSSched = NewSchedule(Pinedo, "Random");
                LocalSearch.SwapHillClimb(ref LSSched, RobustnessMeasures.DebugNumberOfJobsInIndexOrder);
            }
            //List<Schedule> SchedulesToSimulate = new List<Schedule>();
            //SchedulesToSimulate.Add(NewSchedule(Pinedo, "Random"));
            //SchedulesToSimulate.Add(NewSchedule(Pinedo, "RMA"));
            //SchedulesToSimulate.Add(NewSchedule(Pinedo, "GLB"));


            //Console.WriteLine(RobustnessMeasures.SumOfFreeSlacks(Sched));


            //Sched.SetDeadlines(Sched.EstimatedCmax);
            //Sched.SetLSS();
            //Sched.MakeHTMLImage("Nonoptimal LSS schedule for Pinedo Instance");
            //SchedulesToSimulate.Add(Sched);


            //Parallel.ForEach(SchedulesToSimulate, (currentSched) =>
            //{
            //   new Simulation(Nruns, currentSched).Perform();
            //});





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
            Sched.MakeHTMLImage(string.Format("ESS {0} schedule for Pinedo Instance",Sched.Description) );
            return Sched;
        }
    }


    
}
