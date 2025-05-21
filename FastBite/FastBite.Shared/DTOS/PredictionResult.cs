namespace FastBite.Shared.DTOS;

public class PredictionResult
{
    public string PredictedCategory { get; set; }
    public List<string> PredictedTags { get; set; }
}