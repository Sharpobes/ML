using CsvHelper;
using CsvHelper.Configuration;
using EmployeeChurnAnalysis.Models;
using Microsoft.ML;
using System.Data;
using System.Globalization;

namespace EmployeeChurnAnalysis.Data
{
    public class DataProcessor
    {
        /// <summary>
        /// Подготовка данных сотрудников для машинного обучения
        /// </summary>
        public List<EmployeeForML> PrepareDataForML(List<Employee> employees)
        {
            return employees.Select(e => new EmployeeForML
            {
                PersonId = e.PersonId,
                Age = e.Age,
                Sex = e.Sex,
                WorkExperience = (float)((DateTime.Now - e.ReceptionDate).TotalDays),
                WasTrainee = e.WasTrainee ? 1f : 0f,
                GradeUpChange = e.GradeUpChange,
                DepartmentChange = e.DepartmentChange,
                PositionChange = e.PositionChange,
                VacationDays = e.VacationDays,
                AbsenceDays = e.AbsenceDays,
                SickLeaveDays = e.SickLeaveDays,
                Performance2023 = e.Performance2023 ?? 0f,
                TotalSeniority = e.TotalSeniority,
                CourseCount = e.CoursesByMonth.Values.Sum(),
                HasLeft = e.HasLeft
            }).ToList();
        }

        /// <summary>
        /// Сохранение подготовленных данных в CSV файл
        /// </summary>
        public void SaveProcessedData(List<EmployeeForML> data, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            csv.WriteRecords(data);
        }

        /// <summary>
        /// Загрузка данных в формат IDataView для ML.NET
        /// </summary>
        public IDataView LoadDataView(MLContext mlContext, string filePath)
        {
            return mlContext.Data.LoadFromTextFile<EmployeeForML>(
                filePath,
                hasHeader: true,
                separatorChar: ',');
        }

        /// <summary>
        /// Разделение данных на обучающую и тестовую выборки
        /// </summary>
        public (IDataView TrainSet, IDataView TestSet) SplitData(MLContext mlContext, IDataView dataView)
        {
            var dataSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            return (dataSplit.TrainSet, dataSplit.TestSet);
        }

        /// <summary>
        /// Анализ данных об оттоке по месяцам
        /// </summary>
        public Dictionary<int, int> GetChurnByMonth(List<Employee> employees)
        {
            return employees
                .Where(e => e.HasLeft && e.LeaveDate.HasValue)
                .GroupBy(e => e.LeaveDate.Value.Month)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Анализ причин оттока
        /// </summary>
        public Dictionary<string, int> GetChurnReasons(List<Employee> employees)
        {
            return employees
                .Where(e => e.HasLeft)
                .GroupBy(e => e.LeaveReason)
                .OrderByDescending(g => g.Count())
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Анализ оттока по отделам
        /// </summary>
        public Dictionary<string, double> GetChurnRateByDepartment(List<Employee> employees)
        {
            return employees
                .GroupBy(e => e.Department)
                .Where(g => g.Count() >= 5) // Минимальное количество сотрудников для расчета
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round((double)g.Count(e => e.HasLeft) / g.Count() * 100, 2)
                );
        }
    }
}
