using BudgetCalculator.Models;
using Microsoft.AspNetCore.Mvc;

namespace BudgetCalculator.Controllers;

[ApiController]
[Route("api/budget")]
public class BudgetController : ControllerBase
{
    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] BudgetRequest request)
    {
        if (request.Days <= 0) return BadRequest("Количество дней должно быть больше нуля.");

        var total = request.DailyCost * request.Days;

        // БЫЛО (баг): total *= 5;  -> +400% вместо +5%
        if (request.IncludeInsurance)
        {
            total *= 1.05m; // страховка = +5%
        }

        return Ok(new { TotalAmount = total });
    }
}
