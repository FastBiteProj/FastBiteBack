using FastBite.Shared.DTOS;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.IO;

namespace FastBite.ML;

public class MLModelTrainer
{
    private readonly string _categoryModelPath = "MLModels/CategoryModel.zip";
    private readonly string _tagModelPath = "MLModels/ProductTagModel.zip";

    private readonly MLContext _mlContext;

    public MLModelTrainer()
    {
        _mlContext = new MLContext();
    }

    public void TrainAndSaveModels(IEnumerable<TrainingData> trainingData)
    {
        TrainCategoryModel(trainingData);
        TrainTagModel(trainingData);
    }

    private void TrainCategoryModel(IEnumerable<TrainingData> data)
    {
        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms.Text.FeaturizeText("UserInputFeaturized", nameof(TrainingData.UserInput))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(TrainingData.Category)))
            .Append(_mlContext.Transforms.Concatenate("Features", "UserInputFeaturized"))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        var model = pipeline.Fit(dataView);

        Directory.CreateDirectory("MLModels");
        _mlContext.Model.Save(model, dataView.Schema, _categoryModelPath);
    }

    private void TrainTagModel(IEnumerable<TrainingData> data)
    {
        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms.Text.FeaturizeText("UserInputFeaturized", nameof(TrainingData.UserInput))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(TrainingData.ProductTag)))
            .Append(_mlContext.Transforms.Concatenate("Features", "UserInputFeaturized"))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        var model = pipeline.Fit(dataView);

        Directory.CreateDirectory("MLModels");
        _mlContext.Model.Save(model, dataView.Schema, _tagModelPath);
    }
}