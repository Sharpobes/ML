namespace EmployeeChurnAnalysis.Models
{
    public class Employee
    {
        // Идентификатор сотрудника
        public string PersonId { get; set; } = string.Empty;

        // Основная информация
        public DateTime BirthDate { get; set; }
        public int Sex { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public int Age { get; set; }

        // Информация о стажировке
        public bool WasTrainee { get; set; }

        // Личная информация
        public string MaritalStatus { get; set; } = string.Empty;
        public string MilitaryService { get; set; } = string.Empty;
        public string Education { get; set; } = string.Empty;

        // Карьерные изменения
        public int GradeUpChange { get; set; }
        public int DepartmentChange { get; set; }
        public int PositionChange { get; set; }

        // Стаж
        public int TotalSeniority { get; set; }
        public int CurrentGradeSeniority { get; set; }
        public int AverageGradeSeniority { get; set; }

        // Отсутствия
        public int VacationDays { get; set; }
        public int VacationCount { get; set; }
        public int AbsenceDays { get; set; }
        public int AbsenceCount { get; set; }
        public int SickLeaveDays { get; set; }
        public int SickLeaveCount { get; set; }
        public DateTime? LastVacationDate { get; set; }
        public DateTime? LastAbsenceDate { get; set; }
        public DateTime? LastSickLeaveDate { get; set; }

        // Производительность
        public double? Performance2020 { get; set; }
        public double? Performance2021 { get; set; }
        public double? Performance2022 { get; set; }
        public double? Performance2023 { get; set; }

        // Образовательные курсы
        public Dictionary<string, int> CoursesByMonth { get; set; } = new Dictionary<string, int>();

        // Информация об отделе
        public string Department { get; set; } = string.Empty;

        // Информация об увольнении
        public bool HasLeft { get; set; }
        public DateTime? LeaveDate { get; set; }
        public string LeaveReason { get; set; } = string.Empty;
        public bool IsVoluntary { get; set; }
    }
}
