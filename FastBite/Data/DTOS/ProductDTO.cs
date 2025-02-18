namespace FastBite.Data.DTOS
{
    public record ProductDTO
    (
        string CategoryName,
        string ImageUrl,
        int Price,
        List<ProductTranslationDto> Translations
    );
}
