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
    public async Task<IActionResult> CalculateGainOrLoss(List<IFormFile> files)
    {
        var validationResult = files.Select(x=>x.Validate()).ToList();
        if (validationResult.Any(x=>!x.IsValid))
        {
            var errors = validationResult.Where(x=>!x.IsValid).Select(x=>x.ErrorMessage).ToList();
            return BadRequest(string.Join(",", errors));
        }

        return Ok(await _capitalGainLossCalculatorService.CalculateCapitalGainLossAsync(files));
    }
}
