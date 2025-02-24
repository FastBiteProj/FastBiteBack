namespace FastBite.Shared.DTOS
{
    public record ProductDTO
    (
        Guid Id,
        string CategoryName,
        string ImageUrl,
        int Price,
        List<ProductTranslationDto> Translations
    );
}
