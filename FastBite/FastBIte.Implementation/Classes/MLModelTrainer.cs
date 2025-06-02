using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Globalization;

namespace FastBite.ML;

public class MLModelTrainer
{
    private readonly string _categoryModelPath = "MLModels/CategoryModel.zip";
    private readonly string _tagModelPath = "MLModels/ProductTagModel.zip";

    private readonly MLContext _mlContext;

    public MLModelTrainer()
    {
        _mlContext = new MLContext(seed: 42);
    }

    public void TrainModels()
    {
        TrainCategoryModel();
        TrainTagModel();
    }

    #region Category Model

    private void TrainCategoryModel()
    {
        try
        {
            var data = LoadCategoryData("MLModels/category-training-data.csv");
            var dataView = _mlContext.Data.LoadFromEnumerable(data);

            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(CategoryData.UserInput))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(CategoryData.Category)))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(dataView);
            SaveModel(model, dataView.Schema, _categoryModelPath);

            Console.WriteLine($"✅ Category model saved to {_categoryModelPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error training category model: {ex.Message}");
            throw;
        }
    }

    private List<CategoryData> LoadCategoryData(string path)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            PrepareHeaderForMatch = args => args.Header.ToLowerInvariant()
        };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<CategoryDataMap>();

        return csv.GetRecords<CategoryData>()
            .Where(d => !string.IsNullOrWhiteSpace(d.UserInput) && !string.IsNullOrWhiteSpace(d.Category))
            .ToList();
    }

    #endregion

    #region Tag Models

    private void TrainTagModel()
{
    try
    {
        var data = LoadTagData("MLModels/tag-training-data.csv");
        var tagNames = GetUniqueTags(data);

        Console.WriteLine($"🏷 Обнаружено тегов: {string.Join(", ", tagNames)}");

        foreach (var tag in tagNames)
        {
            var binaryData = data.Select(d => new BinaryTagData
            {
                UserInput = d.UserInput,
                Label = d.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)
            }).ToList();

            var dataView = _mlContext.Data.LoadFromEnumerable(binaryData);

            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(BinaryTagData.UserInput))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(dataView);

            var modelPath = $"MLModels/Tags/{tag}-model.zip";
            Directory.CreateDirectory(Path.GetDirectoryName(modelPath));
            _mlContext.Model.Save(model, dataView.Schema, modelPath);

            Console.WriteLine($"✅ Модель для тега '{tag}' сохранена в {modelPath}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка при обучении моделей тегов: {ex}");
        throw;
    }
}

    private List<string> GetUniqueTags(List<TagData> data)
    {
        return data.SelectMany(d => d.Tags)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    private List<MultiLabelData> TransformToMultiLabelFormat(List<TagData> data, List<string> tagNames)
    {
        return data.Select(d => new MultiLabelData
        {
            UserInput = d.UserInput,
            Label = tagNames
                .Select(tag => d.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase) ? 1f : 0f)
                .ToArray()
        }).ToList();
    }

    private List<TagData> LoadTagData(string filePath)
    {
        var lines = File.ReadAllLines(filePath).Skip(1);
        var data = new List<TagData>();

        foreach (var line in lines)
        {
            var parts = line.Split(',', 2);
            if (parts.Length < 2) continue;

            var userInput = parts[0].Trim();
            var tags = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(tag => tag.Trim().Replace("\"", ""))
                   .ToArray();

            data.Add(new TagData
            {
                UserInput = userInput,
                Tags = tags
            });
        }

        return data;
    }

    #endregion

    #region Helpers

    private void SaveModel(ITransformer model, DataViewSchema schema, string modelPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(modelPath));
        _mlContext.Model.Save(model, schema, modelPath);
    }

    #endregion

    #region Data Classes

    public class CategoryData
    {
        public string UserInput { get; set; }
        public string Category { get; set; }
    }
    public class MultiLabelData
    {
        public string UserInput { get; set; }

        [VectorType(20)] 
        public float[] Label { get; set; }
    }

    public class BinaryTagData
    {
        public string UserInput { get; set; }
        public bool Label { get; set; }  
    }

    public class TagData
    {
        public string UserInput { get; set; }
        public string[] Tags { get; set; }
    }

    private sealed class CategoryDataMap : ClassMap<CategoryData>
    {
        public CategoryDataMap()
        {
            Map(m => m.UserInput).Name("userinput");
            Map(m => m.Category).Name("category");
        }
    }

    private sealed class TagDataMap : ClassMap<TagData>
    {
        public TagDataMap()
        {
            Map(m => m.UserInput).Name("userinput");
            Map(m => m.Tags).Name("producttags")
                .Convert(row =>
                {
                    var raw = row.Row.GetField("producttags");
                    return raw?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(t => t.Trim().ToLowerInvariant())
                               .ToArray() ?? Array.Empty<string>();
                });
        }
    }

    #endregion
}