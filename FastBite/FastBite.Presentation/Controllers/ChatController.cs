using FastBite.Core.Models;
using FastBite.Implementation.Classes;
using FastBite.Infrastructure.Contexts;
using FastBite.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace FastBite.Presentation.Controllers;

[Route("api/chat")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly MLModelTrainer _mlModelTrainer;
    private readonly FastBiteContext _context;

    public ChatController(FastBiteContext context)
    {
        _mlModelTrainer = new MLModelTrainer(new MLContext());
        _context = context;
    }

[HttpPost("ask")]
public IActionResult Ask([FromBody] TrainingData request)
{
    var products = _context.Products
        .Include(p => p.Translations)
        .Include(p => p.Category)
        .Include(p => p.ProductTags)
        .ToList();

    var predictedTag = _mlModelTrainer.Predict(request.UserInput, products);

    if (string.IsNullOrEmpty(predictedTag))
    {
        return Ok(new
        {
            Message = "Я не нашел подходящей еды. Попробуйте переформулировать запрос."
        });
    }

    var recommendedProducts = products
        .Where(p => p.ProductTags.Any(t => t.Name.Equals(predictedTag, StringComparison.OrdinalIgnoreCase)))
        .Select(p => new
        {
            Id = p.Id,
            Name = p.Translations.FirstOrDefault(t => t.LanguageCode == "ru")?.Name ?? "Без названия",
            Category = p.Category?.Name,
            Price = p.Price
        })
        .ToList();
    if (recommendedProducts.Count == 0)
    {
        return Ok(new
        {
            Message = "Я не нашел подходящей еды. Попробуйте переформулировать запрос."
        });
    }

    return Ok(new
    {
        Message = $"Рекомендую попробовать: {string.Join(", ", recommendedProducts.Select(p => p.Name))}",
        Products = recommendedProducts
    });
}

}