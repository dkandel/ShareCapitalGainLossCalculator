namespace ShareCapitalGainLossCalculator.Models;

public class Holding
{
    public int RemainingQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal Brokerage { get; set; }
}