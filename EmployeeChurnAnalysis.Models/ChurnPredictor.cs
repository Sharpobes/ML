using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.MulticlassClassification;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmployeeChurnAnalysis.Models
{
    public class ChurnPredictor
    {
        private readonly string[] inputFeatureNames = new[]
        {
            "SEX", "GRADE", "WAS_TRAINEE", "Age", "FULL_SENIORITY", "ILL_DAYS",
            "DAYS_WITHOUT_VALID_REASONS", "VACATION_COUNT_2024", "YEAR_2022", "YEAR_2023",
            "COURSES_07", "COURSES_08", "COURSES_09", "COURSES_10", "COURSES_11", "COURSES_12"
        };

        public ITransformer TrainModel(IDataView trainData, MLContext mlContext)
        {
            var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ChurnInput.VOLUNTARY_TYPE))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("SEX"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("GRADE"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("WAS_TRAINEE"))
                .Append(mlContext.Transforms.Concatenate("Features", inputFeatureNames))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(mlContext.MulticlassClassification.Trainers.OneVersusAll(
                    binaryEstimator: mlContext.BinaryClassification.Trainers.FastTree(),
                    labelColumnName: "Label"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            return pipeline.Fit(trainData);
        }

        public void EvaluateModel(ITransformer model, IDataView testData, MLContext mlContext)
        {
            var predictions = model.Transform(testData);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

            Console.WriteLine("\nОЦЕНКА МОДЕЛИ:");
            Console.WriteLine($"Macro Accuracy: {metrics.MacroAccuracy:P2}");
            Console.WriteLine($"Micro Accuracy: {metrics.MicroAccuracy:P2}");
            Console.WriteLine($"Log Loss: {metrics.LogLoss:F4}");
        }

        public void ShowFeatureImportance(ITransformer model, IDataView trainData, MLContext mlContext)
        {
            Console.WriteLine("\nВАЖНОСТЬ ПРИЗНАКОВ:");

            var permutationMetrics = mlContext.MulticlassClassification
                .PermutationFeatureImportance(
                    model,
                    trainData,
                    labelColumnName: "Label",
                    permutationCount: 3);

            var metricList = permutationMetrics.ToList();

            if (inputFeatureNames.Length != metricList.Count)
            {
                throw new InvalidOperationException("Размерность признаков не совпадает с размерностью важностей.");
            }

            var importances = metricList
                .Select((MulticlassClassificationMetrics v, int index) => new
                {
                    Feature = inputFeatureNames[index],
                    Importance = Math.Abs((double)v.MicroAccuracy.Mean)
                })
                .OrderByDescending(x => x.Importance)
                .ToList();

            double totalImportance = importances.Sum(x => x.Importance);

            foreach (var item in importances)
            {
                double percent = totalImportance == 0 ? 0 : item.Importance / totalImportance * 100.0;
                Console.WriteLine($"🔹 {item.Feature}: {percent:F2}%");
            }
        }

        public void TestHypotheses(IDataView data, MLContext mlContext)
        {
            Console.WriteLine("\n🧪 ПРОВЕРКА ГИПОТЕЗ:");

            var rows = mlContext.Data.CreateEnumerable<ChurnInput>(data, reuseRowObject: false).ToList();

            var trainees = rows.Where(r => r.WAS_TRAINEE == "YES").ToList();
            var nonTrainees = rows.Where(r => r.WAS_TRAINEE == "NO").ToList();

            double voluntaryRateTrainee = trainees.Count(x => x.VOLUNTARY_TYPE == "Voluntary") / (double)Math.Max(1, trainees.Count);
            double voluntaryRateNonTrainee = nonTrainees.Count(x => x.VOLUNTARY_TYPE == "Voluntary") / (double)Math.Max(1, nonTrainees.Count);

            Console.WriteLine($"Стажёры - добровольный уход:     {voluntaryRateTrainee:P2}");
            Console.WriteLine($"Не стажёры - добровольный уход: {voluntaryRateNonTrainee:P2}");

            var low = rows.Where(x => x.YEAR_2023 <= 2).ToList();
            var high = rows.Where(x => x.YEAR_2023 >= 4).ToList();

            double lowRate = low.Count(x => x.VOLUNTARY_TYPE == "Voluntary") / (double)Math.Max(1, low.Count);
            double highRate = high.Count(x => x.VOLUNTARY_TYPE == "Voluntary") / (double)Math.Max(1, high.Count);

            Console.WriteLine($"\nСлабая оценка (≤2) - добровольный: {lowRate:P2}");
            Console.WriteLine($"Высокая оценка (≥4) - добровольный: {highRate:P2}");
        }

        public void SaveModel(ITransformer model, MLContext mlContext, string path, DataViewSchema schema)
        {
            mlContext.Model.Save(model, schema, path);
        }
    }
}
