using CashRegister.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

            var products = new Dictionary<int, Product>();

            foreach (var r in receiptLineDto)
            {
                products[r.ProductID] = await _context.Products.FirstOrDefaultAsync(p => p.ID == r.ProductID);

                if (products[r.ProductID] == null)
                {
                    return BadRequest($"Unknown product ID {r.ProductID}");
                }
            }

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

            await _context.Receipts.AddAsync(newReceipt);
            await _context.SaveChangesAsync();

            return StatusCode((int)HttpStatusCode.Created, newReceipt);
        }
    }
}
