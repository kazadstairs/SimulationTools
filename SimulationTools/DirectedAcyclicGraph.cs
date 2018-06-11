using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class DirectedAcyclicGraph
    {
        public int N { get; private set; }
        public int M { get; private set; }

        public List<Job> Jobs { get; private set; }
        private List<Arc> MachineArcs;

        public DirectedAcyclicGraph()
        {
            N = 0;
            M = 0;
            Jobs = new List<Job>();
            Arcs = new List<Arc>();
        }

        public DirectedAcyclicGraph(int Nvertices, int Medges)
        {
            N = Nvertices;
            M = Medges;
            Jobs = new List<Job>(N);
            Arcs = new List<Arc>(M);
        }

        public void AddJob(Job j)
        {
            Jobs.Add(j);
            N++;
        }

        public void AddArc(Job u, Job v)
        {
            if (PathExists(v, u))
            {
                throw new Exception(string.Format("Adding arc from vertex {0} to vertex {1} would create a cycle in DAG", u.ID, v.ID));
            }
            M++;
            if (u.Successors.Count > 0 && u.Successors.Contains(v))
            { //Arc already present, no need to add it again 
                return;
            }
            Arcs.Add(new Arc(u, v));
            u.Successors.Add(v);
            v.Predecessors.Add(u);
        }

        public Job GetJobById(int id)
        {
            if (Jobs[id].ID != id) { throw new Exception("Id does not match index."); }
            else
            {
                return Jobs[id];
            }
        }

        public void AddArcById(int uid, int vid)
        {
            AddArc(GetJobById(uid), GetJobById(vid));
        }


        //In O(V + E) return all jobs in precedence order, IGNORING MACHINE Assignments!


        public bool PathExists(Job u, Job v)
        {
            // idea: set all visited vertices on a stack, when finished, reset them all to unvisited.
            // so ensure that this is always true after the algorithm:

            foreach (Job w in Jobs)
            {
                w.IsBFSVisited = false;
            }

            Stack<Job> VisitedVertices = new Stack<Job>();

            Queue<Job> BFSQueue = new Queue<Job>();
            Job CurrentVertex = null;
            BFSQueue.Enqueue(u);
            while (BFSQueue.Count > 0)
            {
                CurrentVertex = BFSQueue.Dequeue();
                if (CurrentVertex == v)
                {
                    while (VisitedVertices.Count > 0)
                    {
                        VisitedVertices.Pop().IsBFSVisited = false;
                    }
                    return true; // path exists from u to v
                }
                else
                {
                    foreach (Job w in CurrentVertex.Successors)
                    {
                        if (!CurrentVertex.IsBFSVisited)
                        {
                            // add to explore list
                            BFSQueue.Enqueue(w);
                            w.IsBFSVisited = true;
                            VisitedVertices.Push(w);
                        }
                        //otherwise it is already explored, do nothing
                    }

                }

            }
            while (VisitedVertices.Count > 0)
            {
                VisitedVertices.Pop().IsBFSVisited = false;
            }
            return false;

            

        }

        

        

    }

    class Arc
    {
        Job U; // from this vertex
        Job V; // to this vertex

        public Arc()
        {
            U = null;
            V = null;
        }
        public Arc(Job u, Job v)
        {
            U = u;
            V = v;
        }
    }

    class MachineArc : Arc
    {
        Job U;
        Job V;
        Machine M;

        public MachineArc(Job u, Job v, Machine m)
        {
            U = u;
            V = v;
            M = m;
        }
    }

}
