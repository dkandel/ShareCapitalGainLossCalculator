using Microsoft.AspNetCore.Mvc;
using ShareCapitalGainLossCalculator.Models;
using ShareCapitalGainLossCalculator.Services;
using ShareCapitalGainLossCalculator.Validators;

namespace ShareCapitalGainLossCalculator.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CalculatorsController: ControllerBase
{
    private readonly ICapitalGainLossCalculatorService _capitalGainLossCalculatorService;

    public CalculatorsController(ICapitalGainLossCalculatorService capitalGainLossCalculatorService)
    {
        _capitalGainLossCalculatorService = capitalGainLossCalculatorService;
    }

    [HttpPost("shares/capital-gain-loss")]
    public async Task<IActionResult> CalculateGainOrLoss(IFormFile file)
    {
        var validationResult = file.Validate();
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ErrorMessage);
        }

        return Ok(await _capitalGainLossCalculatorService.CalculateCapitalGainLossAsync(file));
    }
}
