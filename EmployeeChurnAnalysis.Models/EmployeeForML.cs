using Microsoft.ML.Data;

namespace EmployeeChurnAnalysis.Models
{
    public class EmployeeForML
    {
        [LoadColumn(0)]
        public string PersonId { get; set; } = string.Empty;

        [LoadColumn(1)]
        public float Age { get; set; }

        [LoadColumn(2)]
        public float Sex { get; set; }

        [LoadColumn(3)]
        public float WorkExperience { get; set; } // Стаж в днях

        [LoadColumn(4)]
        public float WasTrainee { get; set; } // 1 - да, 0 - нет

        [LoadColumn(5)]
        public float GradeUpChange { get; set; }

        [LoadColumn(6)]
        public float DepartmentChange { get; set; }

        [LoadColumn(7)]
        public float PositionChange { get; set; }

        [LoadColumn(8)]
        public float VacationDays { get; set; }

        [LoadColumn(9)]
        public float AbsenceDays { get; set; }

        [LoadColumn(10)]
        public float SickLeaveDays { get; set; }

        [LoadColumn(11)]
        public float Performance2023 { get; set; }

        [LoadColumn(12)]
        public float TotalSeniority { get; set; }

        [LoadColumn(13)]
        public float CourseCount { get; set; } // Суммарное количество пройденных курсов

        [LoadColumn(14), ColumnName("Label")]
        public bool HasLeft { get; set; }
    }
}
