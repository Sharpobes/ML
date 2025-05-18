using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;

namespace EmployeeChurnAnalysis.ML
{
    public class ChurnInput
    {
        [LoadColumn(0)] public string PERSON_ID;
        [LoadColumn(1)] public string SEX;
        [LoadColumn(2)] public float Age;
        [LoadColumn(3)] public string GRADE;
        [LoadColumn(4)] public string WAS_TRAINEE;
        [LoadColumn(5)] public float FULL_SENIORITY;
        [LoadColumn(6)] public float ILL_DAYS;
        [LoadColumn(7)] public float DAYS_WITHOUT_VALID_REASONS;
        [LoadColumn(8)] public float VACATION_COUNT_2024;
        [LoadColumn(9)] public float YEAR_2022;
        [LoadColumn(10)] public float YEAR_2023;
        [LoadColumn(11)] public float COURSES_07;
        [LoadColumn(12)] public float COURSES_08;
        [LoadColumn(13)] public float COURSES_09;
        [LoadColumn(14)] public float COURSES_10;
        [LoadColumn(15)] public float COURSES_11;
        [LoadColumn(16)] public float COURSES_12;

        [LoadColumn(17)] public string VOLUNTARY_TYPE;
    }

    public class ChurnPredictor
    {
        public ITransformer TrainModel(IDataView trainData, MLContext mlContext)
        {
            var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ChurnInput.VOLUNTARY_TYPE))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("SEX"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("GRADE"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("WAS_TRAINEE"))
                .Append(mlContext.Transforms.Concatenate("Features",
                    "SEX", "GRADE", "WAS_TRAINEE", "Age",
                    "FULL_SENIORITY", "ILL_DAYS", "DAYS_WITHOUT_VALID_REASONS",
                    "VACATION_COUNT_2024", "YEAR_2022", "YEAR_2023",
                    "COURSES_07", "COURSES_08", "COURSES_09",
                    "COURSES_10", "COURSES_11", "COURSES_12"))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(mlContext.MulticlassClassification.Trainers.OneVersusAll(
                    mlContext.BinaryClassification.Trainers.SdcaLogisticRegression()))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            return pipeline.Fit(trainData);
        }

        public void EvaluateModel(ITransformer model, IDataView testData, MLContext mlContext)
        {
            var predictions = model.Transform(testData);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

            Console.WriteLine($"Macro accuracy: {metrics.MacroAccuracy:P2}");
            Console.WriteLine($"Micro accuracy: {metrics.MicroAccuracy:P2}");
            Console.WriteLine($"Log loss: {metrics.LogLoss:F4}");
        }

        public void SaveModel(ITransformer model, MLContext mlContext, string path, DataViewSchema schema)
        {
            mlContext.Model.Save(model, schema, path);
        }
    }
}
