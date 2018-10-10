using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    // This file contains methods for printing schedules.
    partial class Schedule
    {
        public void MakeHTMLImage(string title)
        {
            System.IO.Directory.CreateDirectory(string.Format(@"{0}Results\Schedules\", Program.BASEPATH));
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(string.Format(@"{0}Results\Schedules\{1}.html", Program.BASEPATH, title)))
            {
                file.WriteLine(@"<!DOCTYPE html>");
                file.WriteLine(@"<head>");
                file.WriteLine(@"<style>");

                // css part:
                int scale = 15;
                double top, left, width;
                string background_color;
                foreach (Job j in PrecedenceDAG.Jobs)
                {
                    top = 50 + 50 * GetMachineByJobID(j.ID).MachineID;
                    left = Starttimes[j.ID] * scale;
                    width = j.MeanProcessingTime * scale;
                    if (IsCritical(j)) { background_color = "#ff5500"; }
                    else if (IsAlmostCritical(j, 0.1)) { background_color = "#ffff99"; }
                    else { background_color = "#d9ffb3"; }
                    file.WriteLine("div.j{0}", j.ID);
                    file.WriteLine("{position: fixed;");
                    file.WriteLine("top: {1}px; left: {2}px; width: {3}px;  Background-color:{4};", j.ID, top, left, width,background_color);
                    if (GetStartTimeOfJob(j) - j.EarliestReleaseDate < 0.001)
                    { file.WriteLine("text-decoration: underline;"); }
                    file.WriteLine(@"height: 20px; border: 1px solid #73AD21; text-align: center; vertical-align: middle;}");
                }
                //CmaxBlok:
                top = 50;
                CalcLSS();
                left = EstimatedCmax * scale;
                width = 2 * scale;
                double height = 50 * Machines.Count + 100;
                file.WriteLine("div.CMAX");
                file.WriteLine("{position: fixed;");
                file.WriteLine("top: {0}px; left: {1}px; width: {2}px;height: {3}px;", top, left, width, height);
                file.WriteLine(@" border: 1px solid #73AD21; text-align: center; vertical-align: middle; colour: red;}");


                file.WriteLine(@"</style></head><body>");
                file.WriteLine(@"<div>");
                file.WriteLine(title);
                foreach (RM rm in RMs)
                {
                    file.Write("| RM: {0} = {1} |", rm.Name, rm.Value);
                }
                file.WriteLine(@"</div>");
                foreach (Job j in PrecedenceDAG.Jobs)
                {
                    file.WriteLine("<div class=\"j{0}\"> J{0}; </div>", j.ID);
                }
                file.WriteLine("<div class=\"CMAX\"> Cmax = {0}; </div>", EstimatedCmax);
                file.WriteLine(@"</body></html>");
            }
        }

        /// <summary>
        /// Prints a summary of the current schedule
        /// </summary>
        public void Print()
        {
            Console.WriteLine("*** Schedule information: *** ");
            Console.WriteLine("Problem instance: {0}", this.Problem.Description);
            Console.WriteLine("* Machine info:");
            int AssignedJobsCount = 0;
            foreach (Machine m in Machines)
            {
                Console.Write("*");
                Console.Write("  M {0}: ", m.MachineID);
                foreach (Job j in m.AssignedJobs)
                {

                    Console.Write("{0} ,", j.ID);
                    AssignedJobsCount++;
                   // Console.Write("{0} (index{1}), ", j.ID,m.GetJobIndex(j));
                }
                Console.Write(" *");
                Console.Write(Environment.NewLine);
            }
            Console.WriteLine("{0}/{1} jobs assigned",AssignedJobsCount,PrecedenceDAG.N);
            if (AssignedJobsCount < PrecedenceDAG.N - 1)
            {
                foreach (Job J in PrecedenceDAG.Jobs)
                {
                    string MachineInfo;
                    if (AssignedMachine(J) == null) { MachineInfo = "NULL"; }
                    else { MachineInfo = string.Format("{0}",AssignedMachine(J).MachineID); }
                    Console.WriteLine("J{0} on M{1}", J.ID, MachineInfo);
                }
                throw new Exception("More than 1 job unassigned");
            }
            Console.WriteLine("*****************************");

        }

        public void PrintJobInfo()
        {
            Console.WriteLine("Printing job information........");
            Console.WriteLine("| id  | px  | rx | sx  | tx  | Sum | Estimated Cmax (check) |");
            Job CurrentJob;
            for (int JobID = 0; JobID < PrecedenceDAG.N; JobID++)
            {
                CurrentJob = PrecedenceDAG.GetJobById(JobID);
                Console.WriteLine("| {0,-3} | {1,-3} | {6,-3} | {2,-3} | {3,-3} | {4,-3} | {5,-3} |", JobID, CurrentJob.MeanProcessingTime, GetEarliestStart(CurrentJob), CalcTailTime(CurrentJob), CurrentJob.MeanProcessingTime + GetEarliestStart(CurrentJob) + CalcTailTime(CurrentJob),EstimatedCmax,CurrentJob.EarliestReleaseDate);

            }


        }

        public void CreateDotFile()
        {
            string OutputPath = string.Format("{0}\\probleminstances\\INS_{1}_SCHED_{2}.dotin", Program.BASEPATH,Problem.Description,AssignmentDescription );
            using (StreamWriter sw = File.CreateText(OutputPath))
            {
                sw.WriteLine("digraph G {");

                foreach (Machine M in Machines)
                {
                    sw.Write("subgraph cluster_{0}", M.MachineID);
                    sw.WriteLine(" {");
                    for(int index =0; index < M.AssignedJobs.Count; index++)
                    {
                        if (index < M.AssignedJobs.Count - 1)
                        {
                            if (M.AssignedJobs[index].ID != 0) { sw.Write("{0} -> ", M.AssignedJobs[index].ID); }
                        }
                        else
                        {
                            sw.Write(M.AssignedJobs[index].ID);
                        }
                    }
                    sw.WriteLine("[style=invis];");
                    sw.WriteLine("label = \"M{0}\";", M.MachineID);
                    sw.WriteLine("}");

                }

                foreach (Job u in Problem.DAG.Jobs)
                {
                    foreach (Job v in u.Successors)
                    {
                        sw.WriteLine("{0} -> {1}", u.ID, v.ID);
                    }

                }
                sw.WriteLine("}");
            }
        }

    }
}
