namespace FastBite.Shared.DTOS
{
    public record ProductDTO
    (
        string CategoryName,
        string ImageUrl,
        int Price,
        List<ProductTranslationDto> Translations
    );
}
