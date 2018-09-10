using System;
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
                foreach (Job j in PrecedenceDAG.Jobs)
                {
                    top = 50 + 50 * GetMachineByJobID(j.ID).MachineID;
                    left = Starttimes[j.ID] * scale;
                    width = j.MeanProcessingTime * scale;
                    file.WriteLine("div.j{0}", j.ID);
                    file.WriteLine("{position: fixed;");
                    file.WriteLine("top: {1}px; left: {2}px; width: {3}px;", j.ID, top, left, width);
                    file.WriteLine(@"height: 20px; border: 1px solid #73AD21; text-align: center; vertical-align: middle;}");
                }
                //CmaxBlok:
                top = 50;
                EstimateCmax();
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
            Console.WriteLine("* Machine info:");

            foreach (Machine m in Machines)
            {
                Console.Write("*");
                Console.Write("  M {0}: ", m.MachineID);
                foreach (Job j in m.AssignedJobs)
                {
                    Console.Write("{0}, ", j.ID);
                }
                Console.Write(" *");
                Console.Write(Environment.NewLine);
            }
            Console.WriteLine("*****************************");

        }

    }
}
