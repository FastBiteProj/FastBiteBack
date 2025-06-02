using Microsoft.ML.Data;

namespace FastBite.Shared.DTOS;
public class BinaryTagPrediction
{
    [ColumnName("PredictedLabel")]
    public bool Predicted { get; set; }

    public float Probability { get; set; }

    public float Score { get; set; }
}