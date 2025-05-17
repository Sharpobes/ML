using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using EmployeeChurnAnalysis.Models;
using System.Globalization;
using System.Linq;
using System;

namespace EmployeeChurnAnalysis.Data
{
    public class DataProcessor
    {
        public void SaveProcessedData(List<EmployeeForML> data, string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(data);
        }

        public void TestHypotheses(List<EmployeeForML> data)
        {
            var trainee = data.Where(e => e.WasTrainee == 1f).ToList();
            var notTrainee = data.Where(e => e.WasTrainee == 0f).ToList();
            var churnTrainee = trainee.Count(e => e.HasLeft) / (float)trainee.Count();
            var churnNotTrainee = notTrainee.Count(e => e.HasLeft) / (float)notTrainee.Count();
            Console.WriteLine($"Отток среди стажеров: {churnTrainee:P2}, среди не-стажеров: {churnNotTrainee:P2}");

            var reasons = data.Where(e => e.HasLeft)
                .GroupBy(e => e.LeaveReason)
                .Select(g => new { Reason = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();
            Console.WriteLine("Топ причин оттока:");
            foreach (var r in reasons.Take(5))
                Console.WriteLine($"{r.Reason ?? "Не указано"}: {r.Count}");

            var byDept = data.GroupBy(e => e.Department)
                .Select(g => new
                {
                    Department = g.Key,
                    ChurnRate = g.Count(e => e.HasLeft) / (float)g.Count(),
                    Count = g.Count()
                })
                .OrderByDescending(g => g.ChurnRate)
                .ToList();
            Console.WriteLine("Отток по отделам:");
            foreach (var d in byDept)
                Console.WriteLine($"{d.Department ?? "Не указано"}: {d.ChurnRate:P2} (n={d.Count})");

            var avgCoursesLeft = data.Where(e => e.HasLeft).Average(e => e.CourseCount);
            var avgCoursesStayed = data.Where(e => !e.HasLeft).Average(e => e.CourseCount);
            Console.WriteLine($"Среднее число курсов (ушедшие): {avgCoursesLeft:F2}, (оставшиеся): {avgCoursesStayed:F2}");

            var avgPerfLeft = data.Where(e => e.HasLeft).Average(e => e.Performance2023);
            var avgPerfStayed = data.Where(e => !e.HasLeft).Average(e => e.Performance2023);
            Console.WriteLine($"Средняя оценка 2023 (ушедшие): {avgPerfLeft:F2}, (оставшиеся): {avgPerfStayed:F2}");
        }

    }
}
