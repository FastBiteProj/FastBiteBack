using CsvHelper.Configuration.Attributes;
using Microsoft.ML.Data;
namespace FastBite.Shared.DTOS;

public class TrainingData
{
    [LoadColumn(0)]
    public string UserInput { get; set; }

    [LoadColumn(1)]
    public string Category { get; set; }

    [LoadColumn(2)]
    public string[] ProductTags { get; set; } 
}
