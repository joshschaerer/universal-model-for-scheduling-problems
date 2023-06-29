using System;
using System.Collections.Generic;
using System.Text;
using DecisionDiagrams;
using SchedulingProblem.Model;
using SchedulingProblem.Solvers;


namespace SchedulingProblem.Solvers
{
    /// <summary>
    /// BDD course model
    /// </summary>
    public class BDDSolver : ISolver
    {
        private readonly Scenario scene;
        private Dictionary<(int, int, int, int), VarBool<BDDNode>> x;
        private DD f;

        public BDDSolver(Scenario scene)
        {
            this.scene = scene;
        }

        public (int, bool[][][][], string) Optimize( bool noreturn = false)
        {

            throw new NotImplementedException();
            // var model = new DDManager<BDDNode>(new BDDNodeFactory());

            // CreateVariables(model);
            // CreateConstraints(model);
            // CreateObjective(model);
            // var assignment = model.Sat(f);

            // int[][][][] vars = new int[scene.TimeSlots.Count][][][];
            // StringBuilder sb = new StringBuilder();
            // if (assignment != null)
            // {

            //     foreach (var timeSlot in scene.TimeSlots)
            //     {
            //         //int[][][] locations = new int[scene.Locations.Count][][];
            //         sb.AppendLine(timeSlot.Description);
            //         foreach (var location in scene.Locations)
            //         {
            //             //int[][] tasks = new int[scene.Locations.Count][];
            //             sb.AppendLine($"  {location.Name}");
            //             foreach (var task in scene.Tasks)
            //             {
            //                 //int[] people = new int[scene.People.Count];
            //                 sb.AppendLine($"    {task.Description}");
            //                 foreach (var person in scene.People)
            //                 {
            //                     sb.AppendLine($"      {person.Name}\t{assignment.Get(x[(location.Id, person.Id, timeSlot.Id, task.Id)])}");
            //                     vars[location.Id][person.Id][timeSlot.Id][task.Id] =
            //                         assignment.Get(x[(location.Id, person.Id, timeSlot.Id, task.Id)]) ? 1 : 0;
            //                     ;
            //                 }
            //             }

            //         }
            //     }
            //     return (-1, vars, sb.ToString());
            // }
            // else
            // {
            //     return (-1, vars, "Model cannot be solved");
            // }
        }

        private void CreateVariables(DDManager<BDDNode> model)
        {
            x = new Dictionary<(int, int, int, int), VarBool<BDDNode>>(scene.Locations.Count * scene.People.Count * scene.TimeSlots.Count * scene.Tasks.Count);
            foreach (var location in scene.Locations)
            {
                foreach (var person in scene.People)
                {
                    foreach (var timeSlot in scene.TimeSlots)
                    {
                        foreach (var task in scene.Tasks)
                        {
                            x.Add((location.Id, person.Id, timeSlot.Id, task.Id), model.CreateBool());
                        }
                    }
                }
            }
        }

        private void CreateConstraints(DDManager<BDDNode> model)
        {
            f = model.True();

            // Each Task is assigned to the required numbers of person in the schedule period.
            List<VarBool<BDDNode>> literals = new List<VarBool<BDDNode>>();
            foreach (var location in scene.Locations)
            {
                foreach (var timeSlot in scene.TimeSlots)
                {
                    foreach (var task in scene.Tasks)
                    {
                        foreach (var person in scene.People)
                        {
                            literals.Add(x[(location.Id, person.Id, timeSlot.Id, task.Id)]);
                        }
                        //f = model.And(f, model.CreateInt(literals.Count).Eq(model.CreateInt(task.RequiredNrOfPeople)));
                        literals.Clear();
                    }
                }
            }

            //// Check if person is available per timeslot
            //foreach (var absence in scene.Absences)
            //{
            //	foreach (var location in scene.Locations)
            //	{
            //		foreach (var task in scene.Tasks)
            //		{
            //			f = model.And(f, model.Not(x[(location.Id, absence.PersonID, absence.TimeSlotID, task.Id)].Id()));
            //		}
            //	}
            //}
        }

        private void CreateObjective(DDManager<BDDNode> model)
        {

        }
    }
}

