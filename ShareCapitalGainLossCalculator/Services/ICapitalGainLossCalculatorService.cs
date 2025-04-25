using ShareCapitalGainLossCalculator.Models;

namespace ShareCapitalGainLossCalculator.Services;

public interface ICapitalGainLossCalculatorService
{
    Task<CalculationResult> CalculateCapitalGainLossAsync(IFormFile file);
}
