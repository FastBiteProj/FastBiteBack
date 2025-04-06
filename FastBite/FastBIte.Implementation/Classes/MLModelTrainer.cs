using FastBite.Core.Models;
using Microsoft.ML;

namespace FastBite.Implementation.Classes;

public class MLModelTrainer
{
    private static readonly string ModelPath = "MLModel.zip";
    private MLContext _mlContext;
    private ITransformer _model;

    public MLModelTrainer(MLContext mlContext)
    {
        _mlContext = mlContext;
    }

    public string Predict(string userInput, List<Product> products)
    {
        var tagList = products.SelectMany(p => p.ProductTags).Distinct().ToList();
         var bestMatch = tagList
            .FirstOrDefault(tagName =>
                userInput.IndexOf(tagName.Name, StringComparison.OrdinalIgnoreCase) >= 0);

        
        return bestMatch?.Name ?? "Не нашел подходящих продуктов.";
    }

    private void LoadModel()
    {
        if (File.Exists(ModelPath))
        {
            _model = _mlContext.Model.Load(ModelPath, out _);
        }
    }
}