using Microsoft.ML.Data;

namespace FastBite.Shared.DTOS
{
    public class TagPrediction
    {
        [ColumnName("PredictedTags")]
        public string[] Tags { get; set; }

        [ColumnName("Score")]
        public float[] Scores { get; set; }
    }
}