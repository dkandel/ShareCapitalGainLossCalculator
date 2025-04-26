using ShareCapitalGainLossCalculator.Models;

namespace ShareCapitalGainLossCalculator.Services;

public interface ICapitalGainLossCalculatorService
{
    Task<List<CalculationResult>> CalculateCapitalGainLossAsync(List<IFormFile> files);
}
