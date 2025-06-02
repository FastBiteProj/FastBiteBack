using FastBite.Shared.DTOS;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static FastBite.ML.MLModelTrainer;

namespace FastBite.ML;

public class MLModelPredictor
{
    private readonly string _categoryModelPath = "MLModels/CategoryModel.zip";
    private readonly string _tagModelsDirectory = "MLModels/Tags";
    private readonly MLContext _mlContext;
    private PredictionEngine<TrainingData, CategoryPrediction> _categoryEngine;

    private Dictionary<string, PredictionEngine<BinaryTagData, BinaryTagPrediction>> _tagEngines;

    public MLModelPredictor()
    {
        _mlContext = new MLContext();
        LoadModels();
    }

    private void LoadModels()
    {
        try
        {
            if (!File.Exists(_categoryModelPath))
                throw new FileNotFoundException($"Category model not found at {_categoryModelPath}");

            var categoryModel = _mlContext.Model.Load(_categoryModelPath, out _);
            _categoryEngine = _mlContext.Model.CreatePredictionEngine<TrainingData, CategoryPrediction>(categoryModel);

            if (!Directory.Exists(_tagModelsDirectory))
                throw new DirectoryNotFoundException($"Tag models directory not found at {_tagModelsDirectory}");

            _tagEngines = new Dictionary<string, PredictionEngine<BinaryTagData, BinaryTagPrediction>>();

            foreach (var modelFile in Directory.GetFiles(_tagModelsDirectory, "*-model.zip"))
            {
                var tagName = Path.GetFileNameWithoutExtension(modelFile)
                  .Replace("-model", "")
                  .Replace("\"", "")
                  .Trim();
                var model = _mlContext.Model.Load(modelFile, out _);
                var engine = _mlContext.Model.CreatePredictionEngine<BinaryTagData, BinaryTagPrediction>(model);
                _tagEngines[tagName] = engine;
            }

            if (_tagEngines.Count == 0)
                throw new Exception("No tag models loaded.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load ML models", ex);
        }
    }

    public (string Category, string[] Tags) Predict(string userInput)
    {
        var categoryInput = new TrainingData { UserInput = userInput };
        var categoryPrediction = _categoryEngine.Predict(categoryInput);

        var tags = new List<string>();

        foreach (var (tagName, engine) in _tagEngines)
        {
            var tagInput = new BinaryTagData { UserInput = userInput };
            var prediction = engine.Predict(tagInput);

            if (prediction.Predicted && prediction.Probability > 0.5)
            {
                tags.Add(tagName);
            }
        }

        return (categoryPrediction.PredictedCategory, tags.ToArray());
    }
}