using FastBite.Shared.DTOS;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace FastBite.ML;

public class MLModelPredictor
{
    private readonly string _categoryModelPath = "MLModels/CategoryModel.zip";
    private readonly string _tagModelPath = "MLModels/ProductTagModel.zip";

    private readonly MLContext _mlContext;
    private PredictionEngine<TrainingData, CategoryPrediction> _categoryEngine;
    private PredictionEngine<TrainingData, TagPrediction> _tagEngine;

    public MLModelPredictor()
    {
        _mlContext = new MLContext();

        LoadModels();
    }

    private void LoadModels()
    {
        var categoryModel = _mlContext.Model.Load(_categoryModelPath, out _);
        _categoryEngine = _mlContext.Model.CreatePredictionEngine<TrainingData, CategoryPrediction>(categoryModel);

        var tagModel = _mlContext.Model.Load(_tagModelPath, out _);
        _tagEngine = _mlContext.Model.CreatePredictionEngine<TrainingData, TagPrediction>(tagModel);
    }

    public (string PredictedCategory, string PredictedTag) Predict(string userInput)
    {
        var input = new TrainingData
        {
            UserInput = userInput
        };

        var categoryPrediction = _categoryEngine.Predict(input);
        var tagPrediction = _tagEngine.Predict(input);

        return (categoryPrediction.PredictedCategory, tagPrediction.PredictedTag);
    }
}