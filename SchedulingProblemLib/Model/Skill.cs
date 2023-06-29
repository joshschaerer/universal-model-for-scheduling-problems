using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulingProblem.Model
{
    public class Skill
    {
        public Skill()
        {
            Id = -1;
            Description = "";
        }

        /// <summary>
        /// new instance of skill
        /// </summary>
        /// <param name="id">skills id - NEEDS to match its list index</param>
        /// <param name="desc"></param>
        public Skill(int id, string desc)
        {
            Id = id;
            Description = desc;
        }
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
