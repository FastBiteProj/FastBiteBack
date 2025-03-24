using AutoMapper;
using FastBite.Shared.DTOS;
using FastBite.Core.Models;

namespace FastBite.Implementation.Configs
{
    public class MappingConfiguration
    {
        public static Mapper InitializeConfig()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, RegisterDTO>()
                    .ConstructUsing(u => new RegisterDTO(u.Name, u.Surname, u.Email, u.phoneNumber, "", "", ""))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.phoneNumber))
                    .ForMember(dest => dest.Password, opt => opt.Ignore())
                    .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
                    .ForMember(dest => dest.CaptchaToken, opt => opt.Ignore())
                    .ReverseMap()
                    .ForMember(dest => dest.Password, opt => opt.Ignore());

                cfg.CreateMap<Table, TableDTO>()
                    .ConstructUsing(src => new TableDTO(
                        src.Number, 
                        src.Capacity, 
                        new List<ReservationDTO>())) 
                    .ForMember(dest => dest.TableNumber, opt => opt.MapFrom(src => src.Number))
                    .ForMember(dest => dest.TableCapacity, opt => opt.MapFrom(src => src.Capacity))
                    .ForMember(dest => dest.ReservationsOnDate, opt => opt.Ignore())  
                    .ReverseMap()
                    .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.TableNumber))
                    .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.TableCapacity))
                    .ForMember(dest => dest.Reservations, opt => opt.Ignore()); 
                
                cfg.CreateMap<Reservation, ReservationDTO>()
                    .ConstructUsing(src => new ReservationDTO(
                        src.Id, 
                        src.ReservationStart.ToString("HH:mm"),
                        src.ReservationEnd.ToString("HH:mm"),
                        src.ReservationDate.ToString("yyyy-MM-dd"),
                        src.Table.Capacity,
                        src.Table.Number,
                        src.UserId,
                        src.Order != null ? new CreateOrderDTO(
                            src.Order.Id, 
                            src.Order.OrderItems.Select(oi => new OrderProductDTO(
                                oi.Product.Translations
                                    .Where(t => t.LanguageCode == "en")
                                    .Select(t => t.Name)
                                    .FirstOrDefault() ?? "Unknown Product",
                                oi.Quantity)).ToList(),
                            src.Order.UserId,
                            src.Order.TotalPrice,
                            src.Order.TableNumber,
                            src.Order.ConfirmationDate) : null
                    ))
                    .ForMember(dest => dest.ReservationStartTime, opt => opt.MapFrom(src => src.ReservationStart))
                    .ForMember(dest => dest.ReservationEndTime, opt => opt.MapFrom(src => src.ReservationEnd))
                    .ForMember(dest => dest.ReservationDate, opt => opt.MapFrom(src => src.ReservationDate))
                    .ForMember(dest => dest.GuestCount, opt => opt.MapFrom(src => src.GuestCount))
                    .ForMember(dest => dest.TableNumber, opt => opt.MapFrom(src => src.Table.Number))
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                    .ReverseMap()
                    .ForMember(dest => dest.ReservationStart, opt => opt.MapFrom(src => TimeOnly.Parse(src.ReservationStartTime)))
                    .ForMember(dest => dest.ReservationEnd, opt => opt.MapFrom(src => TimeOnly.Parse(src.ReservationEndTime)))
                    .ForMember(dest => dest.ReservationDate, opt => opt.MapFrom(src => DateOnly.Parse(src.ReservationDate)))
                    .ForMember(dest => dest.Table, opt => opt.Ignore())
                    .ForMember(dest => dest.User, opt => opt.Ignore())
                    .ForMember(dest => dest.TableId, opt => opt.Ignore())
                    .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                    .ForMember(dest => dest.ConfirmationDate, opt => opt.Ignore());
                
                cfg.CreateMap<Order, CreateOrderDTO>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.ProductNames, opt => opt.MapFrom(src =>
                        src.OrderItems != null ? src.OrderItems.Select(oi =>
                            new OrderProductDTO(
                                oi.Product.Translations.FirstOrDefault(t => t.LanguageCode == "en") != null 
                                    ? oi.Product.Translations.FirstOrDefault(t => t.LanguageCode == "en").Name 
                                    : "Unknown Product",
                                oi.Quantity)).ToList() : new List<OrderProductDTO>()))
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                    .ForMember(dest => dest.TableNumber, opt => opt.MapFrom(src => src.TableNumber))
                    .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                    .ForMember(dest => dest.ConfirmationDate, opt => opt.MapFrom(src => src.ConfirmationDate))
                    .ConstructUsing(o => new CreateOrderDTO(
                        o.Id, 
                        o.OrderItems != null ? o.OrderItems.Select(oi =>
                            new OrderProductDTO(
                                oi.Product.Translations.FirstOrDefault(t => t.LanguageCode == "en") != null 
                                    ? oi.Product.Translations.FirstOrDefault(t => t.LanguageCode == "en").Name 
                                    : "Unknown Product",
                                oi.Quantity)).ToList() : new List<OrderProductDTO>(),
                        o.UserId,
                        o.TotalPrice,
                        o.TableNumber,
                        o.ConfirmationDate))
                    .ReverseMap();

                cfg.CreateMap<OrderItem, OrderProductDTO>()
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src =>
                        src.Product.Translations.FirstOrDefault(t => t.LanguageCode == "en") != null 
                            ? src.Product.Translations.FirstOrDefault(t => t.LanguageCode == "en").Name 
                            : "Unknown Product"))
                    .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                    .ConstructUsing(src =>
                        new OrderProductDTO(
                            src.Product.Translations.FirstOrDefault(t => t.LanguageCode == "en") != null 
                                ? src.Product.Translations.FirstOrDefault(t => t.LanguageCode == "en").Name 
                                : "Unknown Product",
                            src.Quantity))
                    .ReverseMap()
                    .ForMember(dest => dest.Product, opt => opt.Ignore())
                    .ForMember(dest => dest.Order, opt => opt.Ignore());


                cfg.CreateMap<AppRole, RoleDTO>()
                    .ConstructUsing(role => new RoleDTO(role.Name))
                    .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
                    .ReverseMap()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name));

                cfg.CreateMap<Product, ProductDTO>()
                    .ConstructUsing(src => new ProductDTO(
                        src.Id,
                        src.Category != null ? src.Category.Name : "Unknown Category",
                        src.ImageUrl,
                        src.Price,
                        src.Translations != null
                            ? src.Translations.Select(t => new ProductTranslationDto(
                                t.LanguageCode,
                                t.Name,
                                t.Description)).ToList()
                            : new List<ProductTranslationDto>(),
                        src.ProductTags != null
                            ? src.ProductTags.Select(t => new ProductTagDTO(t.Name)).ToList()
                            : new List<ProductTagDTO>()))
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                    .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                    .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.Translations))
                    .ForMember(dest => dest.ProductTags, opt => opt.MapFrom(src => src.ProductTags))
                    .ReverseMap()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.Category, opt => opt.Ignore())
                    .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                    .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductTags, opt => opt.Ignore())
                    .ForMember(dest => dest.Translations, opt => opt.MapFrom(dest => dest.Translations));

                cfg.CreateMap<ProductTranslation, ProductTranslationDto>()
                    .ConstructUsing(src => new ProductTranslationDto(
                        src.LanguageCode,
                        src.Name ?? "Unknown Name",
                        src.Description ?? "No Description"))
                    .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.LanguageCode))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                    .ReverseMap()
                    .ForMember(dest => dest.Product, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductId, opt => opt.Ignore());

                cfg.CreateMap<Category, CategoryDTO>()
                    .ConstructUsing(src => new CategoryDTO(
                        src.Id,
                        src.Name
                    ))
                    .ReverseMap()
                    .ForMember(dest => dest.Products, opt => opt.Ignore());

                cfg.CreateMap<ProductTag, ProductTagDTO>()
                    .ConstructUsing(src => new ProductTagDTO(src.Name))
                    .ReverseMap()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.Products, opt => opt.Ignore());
            });

            mapperConfig.AssertConfigurationIsValid();

            var mapper = new Mapper(mapperConfig);
            return mapper;
        }
    }
}