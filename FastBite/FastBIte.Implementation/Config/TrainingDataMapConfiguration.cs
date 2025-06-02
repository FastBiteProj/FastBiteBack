using CsvHelper.Configuration;
using FastBite.Shared.DTOS;
using System.Globalization;
using static FastBite.ML.MLModelTrainer;


public static class CsvConfig
{
    public static CsvConfiguration GetConfig()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            Delimiter = ",",
            HeaderValidated = null,
            BadDataFound = null,
            PrepareHeaderForMatch = args => args.Header.ToLower()
        };
    }
}


public sealed class CategoryDataMap : ClassMap<TrainingData>
{
    public CategoryDataMap()
    {
        Map(m => m.UserInput).Name("UserInput");
        Map(m => m.Category).Name("Category");
        Map(m => m.ProductTags).Ignore();
    }
}

public sealed class TagDataMap : ClassMap<TagData>
{
    public TagDataMap()
    {
        Map(m => m.UserInput).Name("userinput");
        Map(m => m.Tags).Name("producttags")
            .Convert(row => 
            {
                var rawValue = row.Row[1]; 
                
                Console.WriteLine($"RAW: '{rawValue}'");

                if (string.IsNullOrWhiteSpace(rawValue)) 
                    return Array.Empty<string>();

                return rawValue.Replace("\"", "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToArray();
            });
    }
}