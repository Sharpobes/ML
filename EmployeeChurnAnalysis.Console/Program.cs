using EmployeeChurnAnalysis.Data;
using EmployeeChurnAnalysis.Models;
using EmployeeChurnAnalysis.ML;
using System;
using System.IO;
using System.Linq;

namespace EmployeeChurnAnalysis.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string csvPath = @"E:\hakaton_tele2\IT HUB Data\csv\final_employees_filtered.csv";
            string mlReadyCsvPath = @"E:\hakaton_tele2\IT HUB Data\csv\ml_ready.csv";

            var dataLoader = new DataLoader();
            var dataProcessor = new DataProcessor();
            var churnPredictor = new ChurnPredictor();

            // 1. Загрузка данных
            var employees = dataLoader.LoadEmployees(csvPath);

            // 2. Сохранение подготовленных данных (если нужно)
            dataProcessor.SaveProcessedData(employees, mlReadyCsvPath);

            // 3. Анализ гипотез
            dataProcessor.TestHypotheses(employees);

            // 4. Обучение и анализ модели
            churnPredictor.TrainAndEvaluate(mlReadyCsvPath);

            Console.WriteLine("Анализ завершён.");
        }
    }
}