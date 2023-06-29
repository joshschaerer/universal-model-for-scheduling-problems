namespace SchedulingProblem.Solvers
{
    public interface ISolver
    {
        /// <summary>
        /// Executes the optimization on the given Scenario 
        /// </summary>
        /// <returns>A 3-tuple with ( optimal result, variable array => [Person][TimeSlot][Task][Location], success/error message )</returns>
        public (int result, bool[][][][] variables, string message) Optimize(bool noreturn = false);
    }
}