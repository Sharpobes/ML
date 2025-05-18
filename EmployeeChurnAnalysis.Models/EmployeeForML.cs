using Microsoft.ML.Data;

namespace EmployeeChurnAnalysis.Models
{
    public class EmployeeForML
    {
        public string PersonId { get; set; }
        public float Age { get; set; }
        public float Sex { get; set; }
        public float WorkExperience { get; set; }
        public float WasTrainee { get; set; }
        public float GradeUpChange { get; set; }
        public float DepartmentChange { get; set; }
        public float PositionChange { get; set; }
        public float VacationDays { get; set; }
        public float AbsenceDays { get; set; }
        public float SickLeaveDays { get; set; }
        public float Performance2023 { get; set; }
        public float TotalSeniority { get; set; }
        public float CourseCount { get; set; }
        public bool HasLeft { get; set; }
        public string Department { get; set; }
        public string LeaveReason { get; set; }
        public string VoluntaryType { get; set; }
        public bool LeftAfterBonus { get; set; }
    }
}