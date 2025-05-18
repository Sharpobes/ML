using System;
using Microsoft.ML;
using Microsoft.ML.Data;

class Program
{
    static void Main()
    {
        var mlContext = new MLContext();

        var modelPath = "E:\\umneishiy\\EmployeeChurnAnalysis.Console\\bin\\Debug\\net8.0\\Models\\ChurnModel.zip"; // файл должен лежать рядом
        DataViewSchema schema;

        var model = mlContext.Model.Load(modelPath, out schema);
        Console.WriteLine("Модель успешно загружена.");

        var predictionEngine = mlContext.Model.CreatePredictionEngine<ChurnInput, ChurnPrediction>(model);

        var newEmployee = new ChurnInput
        {
            PERSON_ID = "E99999",
            SEX = "1",
            Age = 27,
            GRADE = "M1",
            WAS_TRAINEE = "YES",
            FULL_SENIORITY = 1200,
            ILL_DAYS = 1,
            DAYS_WITHOUT_VALID_REASONS = 0,
            VACATION_COUNT_2024 = 2,
            YEAR_2022 = 4,
            YEAR_2023 = 3,
            COURSES_07 = 1,
            COURSES_08 = 0,
            COURSES_09 = 0,
            COURSES_10 = 0,
            COURSES_11 = 0,
            COURSES_12 = 0,
            VOLUNTARY_TYPE = ""
        };

        var prediction = predictionEngine.Predict(newEmployee);
        Console.WriteLine($"🧠 Прогноз оттока для сотрудника: {prediction.Prediction}");
    }
}
public class ChurnInput
{
    public string PERSON_ID { get; set; }
    public string SEX { get; set; }
    public float Age { get; set; }
    public string GRADE { get; set; }
    public string WAS_TRAINEE { get; set; }
    public float FULL_SENIORITY { get; set; }
    public float ILL_DAYS { get; set; }
    public float DAYS_WITHOUT_VALID_REASONS { get; set; }
    public float VACATION_COUNT_2024 { get; set; }
    public float YEAR_2022 { get; set; }
    public float YEAR_2023 { get; set; }
    public float COURSES_07 { get; set; }
    public float COURSES_08 { get; set; }
    public float COURSES_09 { get; set; }
    public float COURSES_10 { get; set; }
    public float COURSES_11 { get; set; }
    public float COURSES_12 { get; set; }
    public string VOLUNTARY_TYPE { get; set; }
}

public class ChurnPrediction
{
    [ColumnName("PredictedLabel")] public string Prediction { get; set; }
    public float[] Score { get; set; }
}
