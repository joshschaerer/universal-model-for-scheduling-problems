using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulingProblem.Model
{
    public class SchedulingTask
    {
        public SchedulingTask()
        {
            Id = -1;
            Description = "";
            Reps = 1;
            PrefPpl = 1;
            ReqPpl = 1;
            ReqSpecPpl = new int[] { };
            ReqSpecTS = new int[] { };
            ReqSpecLoc = new int[] { };
            Skills = new int[] { };
        }

        public SchedulingTask(int id, string desc, int reps = 1, int reqPpl = 1, int[] reqSpecPpl = null, int[] reqSpecTS = null, int[] reqSpecLoc = null, int[] skills = null, int prefPpl = 1)
        {
            Id = id;
            Description = desc;
            Reps = reps;
            PrefPpl = prefPpl;
            ReqPpl = reqPpl;
            ReqSpecPpl = reqSpecPpl ?? new int[] { };
            ReqSpecTS = reqSpecTS ?? new int[] { };
            ReqSpecLoc = reqSpecLoc ?? new int[] { };
            Skills = skills ?? new int[] { };
        }
        public int Id { get; set; }
        public string Description { get; set; }
        public int Reps { get; set; }
        public int PrefPpl { get; set; }
        public int ReqPpl { get; set; }
        public int[] ReqSpecPpl { get; set; }
        public int[] ReqSpecTS { get; set; }
        public int[] ReqSpecLoc { get; set; }
        public int[] Skills { get; set; }
    };
}
