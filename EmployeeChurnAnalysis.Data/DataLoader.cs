using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using EmployeeChurnAnalysis.Models;

namespace EmployeeChurnAnalysis.Data
{
    public class DataLoader
    {
        public List<EmployeeForML> LoadEmployees(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                MissingFieldFound = null,
                HeaderValidated = null,
                IgnoreBlankLines = true
            };

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, config);

            var records = new List<EmployeeForML>();
            csv.Read();
            csv.ReadHeader();
            var header = csv.HeaderRecord;

            while (csv.Read())
            {
                var e = new EmployeeForML
                {
                    PersonId = csv.GetField("PERSON_ID"),
                    Age = ParseFloat(csv, "Возраст"),
                    Sex = ParseSex(csv, "Пол", "SEX"),
                    WorkExperience = ParseWorkExperience(csv, "Дата приема"),
                    WasTrainee = ParseTrainee(csv, "WAS_TRAINEE"),
                    GradeUpChange = ParseFloat(csv, "GR_UP_CHANGE"),
                    DepartmentChange = ParseFloat(csv, "C_C_CHANGE"),
                    PositionChange = ParseFloat(csv, "POS_D_CHANGE"),
                    VacationDays = ParseFloat(csv, "VACATION_COUNT_2024"),
                    AbsenceDays = ParseFloat(csv, "DAYS_WITHOUT_VALID_REASONS"),
                    SickLeaveDays = ParseFloat(csv, "ILL_DAYS"),
                    Performance2023 = ParseFloat(csv, "2023"),
                    TotalSeniority = ParseFloat(csv, "FULL_SENIORITY"),
                    CourseCount = SumCourses(csv, header),
                    HasLeft = !string.IsNullOrWhiteSpace(csv.GetField("Дата увольнения")),
                    Department = csv.GetField("Отдел"),
                    LeaveReason = csv.GetField("Причины увольнения"),
                    VoluntaryType = csv.GetField("VOLUNTARY_TYPE"),
                    LeftAfterBonus = CalcLeftAfterBonus(csv.GetField("Дата увольнения"))
                };
                records.Add(e);
            }
            return records;
        }

        private float ParseFloat(CsvReader csv, string field)
        {
            var val = csv.GetField(field);
            if (float.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) return f;
            return 0f;
        }

        private float ParseSex(CsvReader csv, string field1, string field2)
        {
            var val = csv.GetField(field1);
            if (val == "М" || val == "0") return 0f;
            if (val == "Ж" || val == "1") return 1f;
            val = csv.GetField(field2);
            if (val == "0") return 0f;
            if (val == "1") return 1f;
            return 0f;
        }

        private float ParseWorkExperience(CsvReader csv, string field)
        {
            var val = csv.GetField(field);
            if (DateTime.TryParse(val, out DateTime dt))
                return (float)(new DateTime(2024, 12, 31) - dt).TotalDays;
            return 0f;
        }

        private float ParseTrainee(CsvReader csv, string field)
        {
            var val = csv.GetField(field);
            if (val == "YES" || val == "1") return 1f;
            return 0f;
        }

        private float SumCourses(CsvReader csv, string[] header)
        {
            float sum = 0f;
            foreach (var h in header)
            {
                if (h.StartsWith("COURSES_"))
                {
                    var val = csv.GetField(h);
                    if (string.IsNullOrWhiteSpace(val))
                    {
                        sum += 0f;
                    }
                    else if (float.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
                    {
                        sum += f;
                    }
                    else
                    {
                        sum += 0f;
                    }
                }
            }
            return sum;
        }


        private bool CalcLeftAfterBonus(string leaveDateStr)
        {
            if (DateTime.TryParse(leaveDateStr, out DateTime leaveDate))
            {
                var bonusDate = new DateTime(leaveDate.Year, 4, 1);
                return leaveDate >= bonusDate;
            }
            return false;
        }
    }
}
