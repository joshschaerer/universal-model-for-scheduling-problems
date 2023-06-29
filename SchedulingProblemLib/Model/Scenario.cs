using System.Collections.Generic;

namespace SchedulingProblem.Model
{
    public class Scenario
    {
        /// <summary>
        /// new empty instance of a scenario
        /// </summary>
        public Scenario()
        {
            People = new List<Person>();
            TimeSlots = new List<TimeSlot>();
            Tasks = new List<SchedulingTask>();
            Locations = new List<Location>();
            Skills = new List<Skill>();
        }
        /// <summary>
        /// new instance of a scenario
        /// </summary>
        /// <param name="ppl">list of people</param>
        /// <param name="ts">list of time slots</param>
        /// <param name="t">list of tasks</param>
        /// <param name="l">list of locations</param>
        public Scenario(List<Person> ppl, List<TimeSlot> ts, List<SchedulingTask> t, List<Location> l)
        {
            People = ppl;
            TimeSlots = ts;
            Tasks = t;
            Locations = l;
            Skills = new List<Skill>();
        }
        /// <summary>
        /// new instance of a scenario
        /// </summary>
        /// <param name="ppl">list of people</param>
        /// <param name="ts">list of time slots</param>
        /// <param name="t">list of tasks</param>
        /// <param name="l">list of locations</param>
        /// <param name="skills">list of skills</param>
        /// <param name="optFairness">if true, tries to divide workloads equally, in respect to assigned timeslots</param>
        /// <param name="mlc">if true, tries to minimize the amount of location changes for these people</param>
        /// <param name="mets">if true, tries to minimize the empty timeslots between these peoples first and last timeslots</param>
        public Scenario(List<Person> ppl, List<TimeSlot> ts, List<SchedulingTask> t, List<Location> l, List<Skill> skills = null, bool optFairness = false, List<Person> mlc = null, List<Person> mets = null, int[,] travelCost = null)
        {
            People = ppl;
            TimeSlots = ts;
            Tasks = t;
            Locations = l;
            Skills = skills;
            TravelCost = travelCost ?? new int[People.Count, Locations.Count];
        }

        /// <summary>
        /// The list of People in this Scenario
        /// </summary>
        public List<Person> People { get; set; }

        /// <summary>
        /// The list TimeSlots in this Scenario
        /// </summary>
        public List<TimeSlot> TimeSlots { get; set; }

        /// <summary>
        /// The list of Tasks in this Scenario
        /// </summary>
        public List<SchedulingTask> Tasks { get; set; }

        /// <summary>
        /// The list of Locations in this Scenario
        /// </summary>
        public List<Location> Locations { get; set; }

        /// <summary>
        /// The list of skills appearing in this Scenario
        /// </summary>
        public List<Skill> Skills { get; set; }

        // ==== HARD CONSTRAINTS ====

        public bool HasAbsences { get; set; }

        public bool HasRepetitions { get; set; }

        public bool HasReqNrOfPpl { get; set; }

        public bool HasReqPeople { get; set; }

        public bool HasReqTS { get; set; }

        public bool HasReqLoc { get; set; }

        public bool HasSkills { get; set; }

        public bool HasCapacity { get; set; }


        // ==== SOFT CONSTRAINTS ====


        public int WageWeight { get; set; }

        public int TravelCostWeight { get; set; }

        public int FairnessWeight { get; set; }

        public int PrefNrOfPeopleWeight { get; set; }

        public int minTSWeight { get; set; }

        public int minLocWeight { get; set; }


        public int[,] TravelCost { get; set; }
    };

}