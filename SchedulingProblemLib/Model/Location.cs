using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulingProblem.Model
{

    public class Location
    {
        public Location()
        {
            Id = -1;
            Name = "";
            Skills = new int[] { };
        }

        /// <summary>
        /// new instance of location
        /// </summary>
        /// <param name="id">the locations id - should match its list index</param>
        /// <param name="name">the locations name</param>
        /// <param name="skills">the skills for tasks that can only be completed in this room</param>
        public Location(int id, string name, int[] skills = null)
        {
            Id = id;
            Name = name;
            Skills = skills ?? new int[] { };
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int[] Skills { get; set; }
    };
}
