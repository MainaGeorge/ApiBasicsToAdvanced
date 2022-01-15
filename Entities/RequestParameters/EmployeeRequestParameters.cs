namespace Entities.RequestParameters
{
    public class EmployeeRequestParameters : RequestParameters
    {
        public uint MinAge { get; set; }
        public uint MaxAge { get; set; } = int.MaxValue;

        public bool IsValidAgeRage => MaxAge > MinAge;

    }
}