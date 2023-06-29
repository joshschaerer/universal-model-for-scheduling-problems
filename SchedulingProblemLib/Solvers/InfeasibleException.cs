namespace SchedulingProblem.Solvers
{
    [System.Serializable]
    public class InfeasibleException : System.Exception
    {
        public InfeasibleException() { }
        public InfeasibleException(string message) : base(message) { }
        public InfeasibleException(string message, System.Exception inner) : base(message, inner) { }
        protected InfeasibleException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}