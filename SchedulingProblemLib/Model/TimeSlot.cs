using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulingProblem.Model
{
    public class TimeSlot
    {
        public TimeSlot()
        {
            Id = -1;
            Description = "";
        }
        /// <summary>
        /// new instance of timeslot
        /// </summary>
        /// <param name="id">timeslots id - NEEDS to match its list index</param>
        /// <param name="desc">description of timeslot</param>
        public TimeSlot(int id, string desc)
        { Id = id; Description = desc; }
        public int Id { get; set; }
        public string Description { get; set; }
    };
}
