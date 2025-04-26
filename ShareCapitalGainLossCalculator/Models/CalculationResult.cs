namespace ShareCapitalGainLossCalculator.Models;

/// <summary>
/// Represents the result of a capital gain or loss calculation, including details such as 
/// total gains, total losses, net gain, net loss, and whether the result is a net gain.
/// </summary>
public class CalculationResult
{
    public string Code { get; set; }
    // The total amount of gains from transactions
    public decimal Gains { get; set; }

    // The total amount of losses from transactions
    public decimal Losses { get; set; }

    // Indicates whether the result is a net gain (true if NetGain > 0)
    public decimal IsGain { get; set; }
}
