using CsvHelper.Configuration;

namespace ShareCapitalGainLossCalculator.Models;

/// <summary>
/// Represents a stock transaction, including details such as stock code, company name, 
/// transaction date, type (BUY or SELL), quantity, unit price, trade value, brokerage fees, 
/// GST, contract note, and total transaction value.
/// </summary>
public class Transaction
{
    // Stock code (e.g., BHP, CBA, etc.)
    public string Code { get; set; }

    // Name of the company
    public string Company { get; set; }

    // Date of the transaction
    public DateTime Date { get; set; }

    // Type of transaction (BUY or SELL)
    public string Type { get; set; }

    // Number of shares bought or sold
    public int Quantity { get; set; }

    // Price per share at the time of the transaction
    public decimal UnitPrice { get; set; }

    // Total value of the trade (Quantity * UnitPrice)
    public decimal TradeValue { get; set; }

    // Brokerage fees including GST
    public decimal BrokerageWithGst { get; set; }

    // GST portion of the brokerage fee
    public decimal Gst { get; set; }

    // Contract note number or reference
    public string ContractNote { get; set; }

    // Total value of the transaction, including brokerage
    public decimal TotalValue { get; set; }
}

public class TransactionMap : ClassMap<Transaction>
{
    public TransactionMap()
    {
        Map(m => m.Code).Name("Code");
        Map(m => m.Company).Name("Company");
        Map(m => m.Date).Name("Date").TypeConverterOption.Format("dd/MM/yyyy"); // Parse date in dd/MM/yyyy format
        Map(m => m.Type).Name("Type");
        Map(m => m.Quantity).Name("Quantity");
        Map(m => m.UnitPrice).Name("Unit Price ($)");
        Map(m => m.TradeValue).Name("Trade Value ($)");
        Map(m => m.BrokerageWithGst).Name("Brokerage+GST ($)");
        Map(m => m.Gst).Name("GST ($)");
        Map(m => m.ContractNote).Name("Contract Note");
        Map(m => m.TotalValue).Name("Total Value ($)");
    }
}
