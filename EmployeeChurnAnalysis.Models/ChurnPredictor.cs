using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.FastTree;
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
            try
            {
                var preview = trainData.Preview();
                foreach (var col in preview.Schema)
                {
                    Console.WriteLine($"{col.Name} — {col.Type}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при предпросмотре данных: {ex.Message}");
            }
            var pipeline = mlContext.Transforms.Categorical.OneHotEncoding("SEX")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("GRADE"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("WAS_TRAINEE"))
                .Append(mlContext.Transforms.Concatenate("Features", inputFeatureNames))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(mlContext.BinaryClassification.Trainers.FastTree());

            return pipeline.Fit(trainData);
        }


        public void EvaluateModel(ITransformer model, IDataView testData, MLContext mlContext)
        {
            var predictions = model.Transform(testData);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions);

            Console.WriteLine("\nОЦЕНКА МОДЕЛИ:");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"AUC: {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"F1 Score: {metrics.F1Score:P2}");
        }

        public void ShowFeatureImportance(ITransformer model)
        {
            var predictor = model as BinaryPredictionTransformer<FastTreeBinaryModelParameters>;

            if (predictor?.Model is FastTreeBinaryModelParameters treeModel)
            {
                VBuffer<float> weights = default;
                treeModel.GetFeatureWeights(ref weights);

                var featureImportance = weights.GetValues().ToArray()
                    .Select((w, i) => new { Feature = inputFeatureNames[i], Weight = Math.Abs(w) })
                    .OrderByDescending(x => x.Weight)
                    .ToList();

                Console.WriteLine("\nВАЖНОСТЬ ПРИЗНАКОВ:");
                foreach (var item in featureImportance)
                {
                    Console.WriteLine($"🔹 {item.Feature}: {item.Weight:F4}");
                }
            }
            else
            {
                Console.WriteLine("Модель не поддерживает анализ важности признаков");
            }
        }



        public void TestHypotheses(IDataView data, MLContext mlContext)
        {
            Console.WriteLine("\n🧪 ПРОВЕРКА ГИПОТЕЗ:");

            var rows = mlContext.Data.CreateEnumerable<ChurnInput>(data, reuseRowObject: false).ToList();

            var trainees = rows.Where(r => r.WAS_TRAINEE == "YES").ToList();
            var nonTrainees = rows.Where(r => r.WAS_TRAINEE == "NO").ToList();

            double voluntaryRateTrainee = trainees.Count(x => x.IsVoluntary) / (double)Math.Max(1, trainees.Count);
            double voluntaryRateNonTrainee = nonTrainees.Count(x => x.IsVoluntary) / (double)Math.Max(1, nonTrainees.Count);

            Console.WriteLine($"Стажёры - добровольный уход:     {voluntaryRateTrainee:P2}");
            Console.WriteLine($"Не стажёры - добровольный уход: {voluntaryRateNonTrainee:P2}");

            var low = rows.Where(x => x.YEAR_2023 <= 2).ToList();
            var high = rows.Where(x => x.YEAR_2023 >= 4).ToList();

            double lowRate = low.Count(x => x.IsVoluntary) / (double)Math.Max(1, low.Count);
            double highRate = high.Count(x => x.IsVoluntary) / (double)Math.Max(1, high.Count);

            Console.WriteLine($"\nСлабая оценка (≤2) - добровольный: {lowRate:P2}");
            Console.WriteLine($"Высокая оценка (≥4) - добровольный: {highRate:P2}");
        }

        public void SaveModel(ITransformer model, MLContext mlContext, string path, DataViewSchema schema)
        {
            mlContext.Model.Save(model, schema, path);
        }
    }
}
