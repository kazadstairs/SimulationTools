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
        public List<Machine> Machines;
        public string Description;

        public ProblemInstance()
        {
            DAG = new DirectedAcyclicGraph();
            Machines = new List<Machine>();
        }
        public void InstanciatePinedo()
        {
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

            // machine arcs all match prec arcs in this instance
            Machines.Add(new Machine(0));
            Machines.Add(new Machine(1));

            Description = "Pinedo";

            
        }
    }
    
}
