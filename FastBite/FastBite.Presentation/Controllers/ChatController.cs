using FastBite.Infrastructure.Contexts;
using FastBite.ML;
using FastBite.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastBite.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ChatController : ControllerBase
{
    private readonly MLModelPredictor _predictor;

    private readonly FastBiteContext _dbContext;

    public ChatController(FastBiteContext dbContext)
    {
        _predictor = new MLModelPredictor();
        _dbContext = dbContext;
    }

   [HttpPost("predict")]
    public async Task<ActionResult> Predict([FromBody] PredictionInput input)
    {
        var prediction = _predictor.Predict(input.UserInput);

        var matchedProducts = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Translations)
            .Where(p => p.Category.Name == prediction.Category)
            .Where(p => prediction.Tags.All(tag =>
                p.ProductTags.Any(pt =>
                    pt.Translations.Any(t =>
                        EF.Functions.Like(t.Name, tag, "\\_")))))
            .ToListAsync();

        return Ok(new
        {
            message = "Вот что я нашёл для вас",
            category = prediction.Category,
            tags = prediction.Tags,
            products = matchedProducts
        });
    }
}