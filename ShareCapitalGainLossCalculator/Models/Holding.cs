namespace ShareCapitalGainLossCalculator.Models;

public class Holding
{
    public int RemainingQuantity { get; set; }
    public decimal UnitPrice { get; init; }
    public DateTime PurchaseDate { get; init; }
    public decimal Brokerage { get; init; }
}
