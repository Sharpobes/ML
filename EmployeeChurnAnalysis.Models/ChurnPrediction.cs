using Microsoft.ML.Data;

namespace EmployeeChurnAnalysis.Models
{
    public class ChurnPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedLabel { get; set; }

        public float Score { get; set; }

        public float Probability { get; set; }
    }
}
