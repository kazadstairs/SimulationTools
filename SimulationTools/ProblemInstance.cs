﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class ProblemInstance
    {
        public DirectedAcyclicGraph DAG;
        public int NMachines;
        //public List<Machine> Machines;
        public string Description;

        public ProblemInstance()
        {
            DAG = new DirectedAcyclicGraph();
        }
        public void InstanciatePinedo()
        {
            DAG.AddJob(new Job(0, 0, 0));

            DAG.AddJob(new Job(1, 4, 0));
            DAG.AddJob(new Job(2, 9, 0));
            DAG.AddJob(new Job(3, 3, 0));
            DAG.AddJob(new Job(4, 3, 0));
            DAG.AddJob(new Job(5, 6, 0));
            DAG.AddJob(new Job(6, 8, 0));
            DAG.AddJob(new Job(7, 12, 0));
            DAG.AddJob(new Job(8, 8, 0));
            DAG.AddJob(new Job(9, 6, 0));

            


            // precedence arcs
            DAG.AddArcById(1, 2);
            DAG.AddArcById(2, 6);
            DAG.AddArcById(3, 4);
            DAG.AddArcById(4, 5);
            DAG.AddArcById(5, 6);
            DAG.AddArcById(5, 7);
            DAG.AddArcById(6, 8);
            DAG.AddArcById(7, 8);
            DAG.AddArcById(7, 9);

            DAG.AddArcById(0, 1);
            DAG.AddArcById(0, 3);

            // machine arcs all match prec arcs in this instance
            NMachines = 2;

            DAG.FillSuccessorDictionaries();

            Description = "Pinedo";            
        }

        public void InstanciateLSTest()
        {
            DAG.AddJob(new Job(0, 0, 0));
            DAG.AddJob(new Job(1, 1, 0));
            DAG.AddJob(new Job(2, 1, 0));
            DAG.AddJob(new Job(3, 1, 0));

            DAG.AddArcById(0, 1);
            DAG.AddArcById(0, 2);
            DAG.AddArcById(0, 3);
            DAG.AddArcById(2, 3);

            NMachines = 2;

            Description = "LSTest";

            DAG.FillSuccessorDictionaries();
        }

        public void ReadFromFile(string FileName, string descr)
        {
            Description = descr;
            DAG.AddJob(new Job(0, 0, 0)); // dummy job
            

            string[] lines = System.IO.File.ReadAllLines(FileName);
            this.NMachines = int.Parse(lines[0]);
            int Njobs = int.Parse(lines[1]);
            // create jobs
            string[] jobs = lines[2].Split();
            string[] jobtemp;
            for (int jobID = 1; jobID < Njobs + 1; jobID++)
            { 
                jobtemp = jobs[jobID].Split(',');                
                DAG.AddJob(new Job(jobID, int.Parse(jobtemp[0]), int.Parse(jobtemp[1]))); // job id, pj, rj
            }

            // create prec relations
            string [] splitline;
            for (int rowid = 1; rowid < Njobs+1; rowid++)
            {
                splitline = lines[2 + rowid].Split();
                for (int colid = 1; colid < Njobs+1; colid++)
                {
                    if (splitline[colid] != "-")
                    {
                        DAG.AddArcById(rowid, colid);
                    }
                }
            }

            DAG.FillSuccessorDictionaries();
            
        }


    }
    
}
