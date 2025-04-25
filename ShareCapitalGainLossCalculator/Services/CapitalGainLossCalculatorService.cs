using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ShareCapitalGainLossCalculator.Models;

namespace ShareCapitalGainLossCalculator.Services;

public class CapitalGainLossCalculatorService : ICapitalGainLossCalculatorService
{
    public async Task<CalculationResult> CalculateCapitalGainLossAsync(IFormFile file)
    {
        var transactions = await ParseCsvFileAsync(file);
        foreach (var transaction in transactions)
        {
            Console.WriteLine($"{transaction.Code} {transaction.Type} {transaction.UnitPrice} {transaction.TradeValue} {transaction.TotalValue}");
        }
        return new CalculationResult();
    }

    private async Task<List<Transaction>> ParseCsvFileAsync(IFormFile file)
    {
        try
        {
            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null, // Ignore header validation errors
                MissingFieldFound = null // Ignore missing fields
            });

            // Register the custom mapping
            csv.Context.RegisterClassMap<TransactionMap>();
            var transactions = new List<Transaction>();
            await foreach (var record in csv.GetRecordsAsync<Transaction>())
            {
                transactions.Add(record);
            }

            return transactions;
        }
        catch (Exception ex)
        {
            throw new ArgumentException("The file content is invalid or cannot be parsed into transactions.", ex);
        }
    }
}
