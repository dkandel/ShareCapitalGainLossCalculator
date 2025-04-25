namespace ShareCapitalGainLossCalculator.Models;

/// <summary>
/// Represents the result of a capital gain or loss calculation, including details such as 
/// total gains, total losses, net gain, net loss, and whether the result is a net gain.
/// </summary>
public class CalculationResult
{
    // The total amount of gains from transactions
    public double Gains { get; set; }

    // The total amount of losses from transactions
    public double Losses { get; set; }

    // The net gain after subtracting losses from gains (if positive)
    public double NetGain { get; set; }

    // The net loss after subtracting gains from losses (if positive)
    public double NetLoss { get; set; }

    // Indicates whether the result is a net gain (true if NetGain > 0)
    public bool IsGain { get; set; }
}
