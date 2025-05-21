namespace FastBite.Shared.DTOS;

public record ProductTagDTO(
    Guid Id,
    List<ProductTagTranslationDTO> Translations
);