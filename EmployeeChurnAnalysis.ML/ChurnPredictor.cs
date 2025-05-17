using EmployeeChurnAnalysis.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Data;

namespace EmployeeChurnAnalysis.ML
{
    public class ChurnPredictor
    {
        private readonly MLContext _mlContext;

        public ChurnPredictor()
        {
            _mlContext = new MLContext(seed: 42);
        }

        /// <summary>
        /// Обучение модели предсказания оттока
        /// </summary>
        public ITransformer TrainModel(IDataView trainingDataView)
        {
            // Определение пайплайна обработки данных и обучения
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("SexEncoded", "Sex")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("WasTraineeEncoded", "WasTrainee"))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    "Age", "SexEncoded", "WorkExperience", "WasTraineeEncoded",
                    "GradeUpChange", "DepartmentChange", "PositionChange",
                    "VacationDays", "AbsenceDays", "SickLeaveDays",
                    "Performance2023", "TotalSeniority", "CourseCount"))
                .Append(_mlContext.BinaryClassification.Trainers.FastTree(
                    labelColumnName: "Label",
                    featureColumnName: "Features",
                    numberOfLeaves: 20,
                    numberOfTrees: 100));

            // Обучение модели
            Console.WriteLine("Обучение модели...");
            var model = pipeline.Fit(trainingDataView);
            Console.WriteLine("Модель обучена");
            return model;
        }

        /// <summary>
        /// Оценка качества модели
        /// </summary>
        public void EvaluateModel(ITransformer model, IDataView testDataView)
        {
            var predictions = model.Transform(testDataView);
            var metrics = _mlContext.BinaryClassification.Evaluate(predictions);

            Console.WriteLine("=== Метрики модели ===");
            Console.WriteLine($"Точность (Accuracy): {metrics.Accuracy:P2}");
            Console.WriteLine($"AUC: {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"F1 Score: {metrics.F1Score:P2}");
            Console.WriteLine($"Положительная точность: {metrics.PositivePrecision:P2}");
            Console.WriteLine($"Отрицательная точность: {metrics.NegativePrecision:P2}");
            Console.WriteLine($"Положительная полнота: {metrics.PositiveRecall:P2}");
            Console.WriteLine($"Отрицательная полнота: {metrics.NegativeRecall:P2}");
            Console.WriteLine("=== Матрица ошибок ===");
            Console.WriteLine($"Истинно положительные: {metrics.ConfusionMatrix.TruePositives}");
            Console.WriteLine($"Ложно положительные: {metrics.ConfusionMatrix.FalsePositives}");
            Console.WriteLine($"Истинно отрицательные: {metrics.ConfusionMatrix.TrueNegatives}");
            Console.WriteLine($"Ложно отрицательные: {metrics.ConfusionMatrix.FalseNegatives}");
        }

        /// <summary>
        /// Получение важности признаков модели
        /// </summary>
        public List<FeatureImportance> GetFeatureImportance(ITransformer model, IDataView dataView)
        {
            var permutationMetrics = _mlContext.BinaryClassification.PermutationFeatureImportance(
                model, dataView, labelColumnName: "Label", permutationCount: 10);

            var featureImportanceMetrics = permutationMetrics
                .Select((m, i) => new { Index = i, Metrics = m })
                .OrderByDescending(feature => Math.Abs(feature.Metrics.AreaUnderRocCurve.Mean))
                .ToList();

            var featureNames = new[]
            {
                "Age", "Sex", "WorkExperience", "WasTrainee",
                "GradeUpChange", "DepartmentChange", "PositionChange",
                "VacationDays", "AbsenceDays", "SickLeaveDays",
                "Performance2023", "TotalSeniority", "CourseCount"
            };

            var result = featureImportanceMetrics
                .Select(feature => new FeatureImportance
                {
                    FeatureName = featureNames[feature.Index],
                    Importance = feature.Metrics.AreaUnderRocCurve.Mean
                })
                .ToList();

            return result;
        }

        /// <summary>
        /// Сохранение обученной модели
        /// </summary>
        public void SaveModel(ITransformer model, string filePath)
        {
            _mlContext.Model.Save(model, null, filePath);
        }

        /// <summary>
        /// Загрузка обученной модели
        /// </summary>
        public ITransformer LoadModel(string filePath)
        {
            return _mlContext.Model.Load(filePath, out var _);
        }

        /// <summary>
        /// Предсказание оттока для сотрудника
        /// </summary>
        public ChurnPrediction PredictChurn(ITransformer model, EmployeeForML employee)
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<EmployeeForML, ChurnPrediction>(model);
            return predictionEngine.Predict(employee);
        }

        /// <summary>
        /// Предсказание оттока для группы сотрудников
        /// </summary>
        public List<(Employee Employee, float Risk)> PredictChurnForEmployees(
            ITransformer model,
            List<Employee> employees,
            DataProcessor dataProcessor)
        {
            // Подготовка данных
            var employeeData = dataProcessor.PrepareDataForML(employees);

            // Создание предсказателя
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<EmployeeForML, ChurnPrediction>(model);

            // Выполнение предсказаний
            var results = new List<(Employee Employee, float Risk)>();

            foreach (var employee in employees)
            {
                var data = employeeData.FirstOrDefault(e => e.PersonId == employee.PersonId);
                if (data != null)
                {
                    var prediction = predictionEngine.Predict(data);
                    results.Add((employee, prediction.Probability));
                }
            }

            return results.OrderByDescending(r => r.Risk).ToList();
        }
    }

    public class FeatureImportance
    {
        public string FeatureName { get; set; } = string.Empty;
        public double Importance { get; set; }
    }
}
