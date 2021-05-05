using CashRegister.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashRegister.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly CashRegisterDataContext _context;
        public ReceiptsController(CashRegisterDataContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Receipt))]
        public async Task<IActionResult> PostReceipt([FromBody] List<ReceiptLineDto> receiptLineDto )
        {
            if (receiptLineDto == null || receiptLineDto.Count == 0) return BadRequest();

            // Read product data from DB for incoming product IDs
            var products = new Dictionary<int, Product>();

            // Here you have to add code that reads all products referenced by product IDs
            // in receiptDto.Lines and store them in the `products` dictionary.
            foreach (var r in receiptLineDto)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.ID == r.ProductID);
            }



            // Build receipt from DTO
            var newReceipt = new Receipt
            {
                ReceiptTimestamp = DateTime.UtcNow,
                ReceiptLines = receiptLineDto.Select(rl => new ReceiptLine
                {
                    ID = 0,
                    Product = products[rl.ProductID],
                    Amount = rl.Amount,
                    TotalPrice = rl.Amount * products[rl.ProductID].UnitPrice
                }).ToList()
            };
            newReceipt.TotalPrice = newReceipt.ReceiptLines.Sum(rl => rl.TotalPrice);
        }
    }
}
