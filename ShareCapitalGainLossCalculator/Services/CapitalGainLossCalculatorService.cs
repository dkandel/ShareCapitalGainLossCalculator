using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ShareCapitalGainLossCalculator.Models;

namespace ShareCapitalGainLossCalculator.Services;

public class CapitalGainLossCalculatorService : ICapitalGainLossCalculatorService
{
    private readonly Dictionary<string, Queue<Holding>> _holdings = new();

    public async Task<List<CalculationResult>> CalculateCapitalGainLossAsync(List<IFormFile> files)
    {
        var groupedTransactions = await ParseCsvFilesAsync(files);
        var results = CalculateResults(groupedTransactions);
        return results;
    }
    
    private static async Task<List<IGrouping<string, Transaction>>> ParseCsvFilesAsync(List<IFormFile> files)
    {
        try
        {
            var transactions = new List<Transaction>();
            foreach (var file in files)
            {
                await using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                });

                // Register the custom mapping
                csv.Context.RegisterClassMap<TransactionMap>();

                var records = csv.GetRecords<Transaction>();
                transactions.AddRange(records);
            }

            return transactions.GroupBy(x => x.Code).ToList();
        }
        catch (Exception ex)
        {
            throw new ArgumentException("The file content is invalid or cannot be parsed into transactions.", ex);
        }
    }

    private List<CalculationResult> CalculateResults(IEnumerable<IGrouping<string, Transaction>> groupedTransactions)
    {
        var results = new List<CalculationResult>();
        // if there are no sells, we can ignore those transactions as there is no loss or gain without a sell transaction.
        var transactionsWithSell = groupedTransactions.Where(x => x.Any(y => y.Type == TransactionType.Sell));
        foreach (var groupedTransaction in transactionsWithSell)
        {
            if (!_holdings.TryGetValue(groupedTransaction.Key, out var value))
            {
                value = new Queue<Holding>();
                _holdings[groupedTransaction.Key] = value;
            }

            var transactions = groupedTransaction.OrderBy(x => x.Date).ToList();
            var calculationResult = new CalculationResult
            {
                Code = groupedTransaction.Key
            };
            foreach (var transaction in transactions)
            {
                switch (transaction.Type)
                {
                    case TransactionType.Buy:
                        value.Enqueue(new Holding
                        {
                            PurchaseDate = transaction.Date,
                            Brokerage = transaction.BrokerageWithGst,
                            // initially remaining quantity will be the actual quantity of the buy transaction
                            RemainingQuantity = transaction.Quantity,
                            UnitPrice = transaction.UnitPrice
                        });
                        break;
                    case TransactionType.Sell:
                    {
                        if (_holdings[transaction.Code].Count != 0)
                        {
                            var (gain, loss) = ProcessSell(transaction);
                            calculationResult.Gains += gain;
                            calculationResult.Losses -= loss;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(transaction.Type.ToString());
                }
            }

            if (calculationResult != null)
            {
                results.Add(calculationResult);
            }

            ;
        }

        return results;
    }

    private (decimal Gain, decimal Loss) ProcessSell(Transaction sellTransaction)
    {
        var holdingsQueue = _holdings[sellTransaction.Code];
        var quantityToSell = -sellTransaction.Quantity;
        var saleProceedsPerShare = sellTransaction.UnitPrice;
        var saleBrokerage = sellTransaction.BrokerageWithGst;
        decimal gain = 0, loss = 0;
        while (quantityToSell > 0 && holdingsQueue.Count != 0)
        {
            var holding = holdingsQueue.Peek();
            var sellQuantity = Math.Min(quantityToSell, holding.RemainingQuantity);

            var purchaseCost = sellQuantity * holding.UnitPrice;
            var purchaseBrokerageProportion = holding.Brokerage * (sellQuantity / (decimal) holding.RemainingQuantity);

            var saleProceeds = sellQuantity * saleProceedsPerShare;
            var saleBrokerageProportion = saleBrokerage * (sellQuantity / (decimal) sellTransaction.Quantity);

            var costBase = purchaseCost + purchaseBrokerageProportion;
            var capitalProceeds = saleProceeds - saleBrokerageProportion;
            var gainOrLoss = capitalProceeds - costBase;

            var eligibleForCgtDiscount = (sellTransaction.Date - holding.PurchaseDate).TotalDays > 365;

            if (gainOrLoss > 0)
            {
                if (eligibleForCgtDiscount)
                {
                    gainOrLoss *= 0.5m; // Apply 50% CGT discount
                }

                gain = gainOrLoss;
            }
            else
            {
                loss = Math.Abs(gainOrLoss);
            }

            holding.RemainingQuantity -= sellQuantity;
            quantityToSell -= sellQuantity;

            if (holding.RemainingQuantity == 0)
            {
                holdingsQueue.Dequeue(); // Remove holding if fully sold
            }
        }

        return (gain, loss);
    }
}
