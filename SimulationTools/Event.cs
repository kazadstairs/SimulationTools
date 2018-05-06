using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    abstract class Event : IComparable<Event>
    {
        public double Time { get; private set; }

        public Event(double time)
        {
            Time = time;
        }

        virtual public void Handle()
        {
            throw new System.NotImplementedException();
        }

        public int CompareTo(Event other)
        {
            return this.Time.CompareTo(other.Time);
        }
    }

    class EGeneric : Event
    {
        public EGeneric(double time) : base(time) { }

        override public void Handle()
        {
            //Console.WriteLine("An EGeneric event happenned at time: {0}", Time);
        }
    }
}
