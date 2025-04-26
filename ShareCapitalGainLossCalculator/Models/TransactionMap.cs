using CsvHelper.Configuration;

namespace ShareCapitalGainLossCalculator.Models;

public sealed class TransactionMap : ClassMap<Transaction>
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