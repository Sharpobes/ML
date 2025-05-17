using Microsoft.ML;
using EmployeeChurnAnalysis.Models;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Transforms;
using System;
using System.Linq;

namespace EmployeeChurnAnalysis.ML
{
    public class ChurnPredictor
    {
        public void TrainAndEvaluate(string mlReadyCsvPath)
        {
            var mlContext = new MLContext(seed: 42);
            var dataView = mlContext.Data.LoadFromTextFile<EmployeeForML>(
                mlReadyCsvPath,
                hasHeader: true,
                separatorChar: ';'
            );

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = mlContext.Transforms.Categorical.OneHotEncoding("SexEncoded", nameof(EmployeeForML.Sex))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("WasTraineeEncoded", nameof(EmployeeForML.WasTrainee)))
                .Append(mlContext.Transforms.Concatenate("Features",
                    nameof(EmployeeForML.Age), "SexEncoded", nameof(EmployeeForML.WorkExperience), "WasTraineeEncoded",
                    nameof(EmployeeForML.GradeUpChange), nameof(EmployeeForML.DepartmentChange), nameof(EmployeeForML.PositionChange),
                    nameof(EmployeeForML.VacationDays), nameof(EmployeeForML.AbsenceDays), nameof(EmployeeForML.SickLeaveDays),
                    nameof(EmployeeForML.Performance2023), nameof(EmployeeForML.TotalSeniority), nameof(EmployeeForML.CourseCount)))
                .Append(mlContext.BinaryClassification.Trainers.FastTree(
                    labelColumnName: nameof(EmployeeForML.HasLeft),
                    featureColumnName: "Features"
                ));

            var model = pipeline.Fit(split.TrainSet);

            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: nameof(EmployeeForML.HasLeft));
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}, AUC: {metrics.AreaUnderRocCurve:P2}, F1: {metrics.F1Score:P2}");

            var permutation = mlContext.BinaryClassification
                .PermutationFeatureImportance<CalibratedModelParametersBase<FastTreeBinaryModelParameters>>(
                    ((BinaryPredictionTransformer<CalibratedModelParametersBase<FastTreeBinaryModelParameters>>)model.LastTransformer),
                    split.TestSet,
                    labelColumnName: nameof(EmployeeForML.HasLeft));

            var featureNames = new[]
            {
                "Age", "Sex", "WorkExperience", "WasTrainee",
                "GradeUpChange", "DepartmentChange", "PositionChange",
                "VacationDays", "AbsenceDays", "SickLeaveDays",
                "Performance2023", "TotalSeniority", "CourseCount"
            };
            Console.WriteLine("Feature importances:");
            for (int i = 0; i < permutation.Length && i < featureNames.Length; i++)
            {
                Console.WriteLine($"{featureNames[i]}: {permutation[i].AreaUnderRocCurve.Mean:F4}");
            }
        }
    }
}
