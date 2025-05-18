using System;
using System.IO;
using Microsoft.ML;
using EmployeeChurnAnalysis.Data;
using EmployeeChurnAnalysis.ML;

namespace EmployeeChurnAnalysis.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("===== Анализ оттока сотрудников =====");

            var dataPath = @"E:\hakaton_tele2\IT HUB Data\csv\final_employees_filtered.csv";
            var modelPath = Path.Combine(Environment.CurrentDirectory, "Models", "ChurnModel.zip");

            Directory.CreateDirectory(Path.GetDirectoryName(modelPath));

            try
            {
                var mlContext = new MLContext(seed: 42);

                var dataView = mlContext.Data.LoadFromTextFile<ChurnInput>(
                    path: dataPath,
                    hasHeader: true,
                    separatorChar: ';');

                var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

                var predictor = new ChurnPredictor();
                var model = predictor.TrainModel(split.TrainSet, mlContext);
                predictor.EvaluateModel(model, split.TestSet, mlContext);
                predictor.ShowFeatureImportance(model, split.TrainSet, mlContext);
                predictor.TestHypotheses(split.TrainSet, mlContext);
                predictor.SaveModel(model, mlContext, modelPath, split.TrainSet.Schema);
                System.Console.WriteLine($"\nМодель сохранена в {modelPath}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Ошибка: {ex.Message}");
                System.Console.WriteLine(ex.StackTrace);
            }

            System.Console.WriteLine("\nНажмите любую клавишу для завершения...");
            System.Console.ReadKey();
        }
    }
}