using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulingProblem.Model
{
    public class Person
    {
        /// <summary>
        /// new instance of person
        /// </summary>
        public Person()
        {
            Id = -1;
            Name = "person";
            Workload = 100;
            Absences = new int[] { };
            Skills = new int[] { };
            Wage = 1;
            Capacity = 1;
        }

        public Person(int id, string name, int[] absences)
        {
            Id = id;
            Name = name;
            Absences = absences;
        }

        /// <summary>
        /// new instance of person
        /// </summary>
        /// <param name="id">The person's ID - should match their list index </param>
        /// <param name="name">The persons name</param>
        /// <param name="absences">List of timeslot indexes where the person cannot work</param>
        /// <param name="skills">List of skills this person has aqquired</param>
        /// <param name="wage">Cost per timeslot that this person is assigned</param>
        /// <param name="capacity">Number of Tasks this person can fulfill in one timeslot</param>
        public Person(int id, string name, int workload = 100, int[] absences = null, int[] skills = null, int wage = 1, int capacity = 1)
        {
            Id = id;
            Name = name;
            Workload = workload;
            Absences = absences ?? new int[] { };
            Skills = skills ?? new int[] { };
            Wage = wage;
            Capacity = capacity;
        }


        public int Id { get; set; }
        public string Name { get; set; }
        public int Workload { get; set; }
        public int[] Absences { get; set; }
        public int[] Skills { get; set; }
        public int Wage { get; set; }
        public int Capacity { get; set; }
    }
}
