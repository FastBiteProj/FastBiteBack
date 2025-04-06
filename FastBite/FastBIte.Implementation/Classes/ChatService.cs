// using AutoMapper;
// using FastBite.Core.Models;
// using FastBite.Implementation.Configs;
// using FastBite.Infrastructure.Contexts;
// using Microsoft.EntityFrameworkCore;
//
// namespace FastBite.Implementation.Classes;
//
// public class ChatService
// {
//     private readonly FastBiteContext _context;
//     public readonly IMapper _mapper;
//
//     public ChatService(FastBiteContext context)
//     {
//         _context = context;
//         _mapper = MappingConfiguration.InitializeConfig();
//     }
//
//     public <List<Product> GetProducts()
//     {
//         var products = _context.Products
//             .Include(p => p.Translations)
//             .Include(p => p.Category)
//             .Include(p => p.ProductTags)
//             .ToList();
//         return _mapper.Map<List<Product>>(products);
//     }
// }