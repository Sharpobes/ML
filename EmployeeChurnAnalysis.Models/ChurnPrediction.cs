using Microsoft.ML.Data;

namespace EmployeeChurnAnalysis.ML
{
    public class ChurnPrediction
    {
        [ColumnName("PredictedLabel")] public string Prediction;
        public float[] Score;
    }
}