using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;
using ShareCapitalGainLossCalculator.Services;

namespace ShareCapitalGainLossCalculator.UnitTests.ServicesTests;

public class CapitalGainLossCalculatorServiceTests
{
    private readonly CapitalGainLossCalculatorService _capitalGainLossCalculatorService = new();

    [Fact]
    public async Task CalculateCapitalGainLossAsync_WhenBuyAndSellWithinOneYear_ShouldNotApplyCGTDiscount()
    {
        // Arrange
        const string csvContent = """
                                  Code,Company,Date,Type,Quantity,Unit Price ($),Trade Value ($),Brokerage+GST ($),GST ($),Contract Note,Total Value ($)
                                  BHP,BHP Group,01/01/2024,Buy,100,10,1000,10,0,CN1,1010
                                  BHP,BHP Group,01/06/2024,Sell,-100,15,1500,15,0,CN2,1485
                                  """;
        var mockFile = CreateMockIFormFile(csvContent, "transactions.csv");

        var results = await _capitalGainLossCalculatorService.CalculateCapitalGainLossAsync([mockFile]);
        var result = results.Single();

        Assert.Equal(475, result.Gains);
    }

    [Fact]
    public async Task CalculateCapitalGainLossAsync_WhenBuyAndSellAfterOneYear_ShouldApplyCGTDiscount()
    {
        // Arrange
        const string csvContent = """
                                  Code,Company,Date,Type,Quantity,Unit Price ($),Trade Value ($),Brokerage+GST ($),GST ($),Contract Note,Total Value ($)
                                  BHP,BHP Group,01/01/2023,Buy,100,10,1000,10,0,CN1,1010
                                  BHP,BHP Group,01/02/2024,Sell,-100,15,1500,15,0,CN2,1485
                                  """;
        var mockFile = CreateMockIFormFile(csvContent, "transactions.csv");

        // Act
        var results = await _capitalGainLossCalculatorService.CalculateCapitalGainLossAsync([mockFile]);
        var result = results.Single();

        // Assert
        // Capital gain = 1500 - 1000 - (10 + 15) = 475
        // CGT discount = 50% of the gain (since held for more than 1 year)
        const decimal expectedGainAfterDiscount = 475 * 0.5m; // 50% discount applied
        Assert.Equal(expectedGainAfterDiscount, result.Gains);
    }

    [Fact]
    public async Task CalculateCapitalGainLossAsync_PartialSell_CorrectlyCalculateProportion()
    {
        // Arrange
        const string csvContent = """
                                  Code,Company,Date,Type,Quantity,Unit Price ($),Trade Value ($),Brokerage+GST ($),GST ($),Contract Note,Total Value ($)
                                  BHP,BHP Group,01/01/2023,Buy,100,10,1000,10,0,CN1,1010
                                  BHP,BHP Group,01/02/2023,Sell,-50,15,750,15,0,CN2,735
                                  """;
        var mockFile = CreateMockIFormFile(csvContent, "transactions.csv");

        // Act
        var results = await _capitalGainLossCalculatorService.CalculateCapitalGainLossAsync([mockFile]);
        var result = results.Single();

        // Assert
        // Capital gain for 50 shares = (Sell Trade Value - Proportional Buy Trade Value - Brokerage (Buy + Sell))
        // Proportional Buy Trade Value = (1000 / 100) * 50 = 500
        // Capital gain = 750 - 500 - (10 * 0.5 + 15) = 750 - 500 - 20 = 230
        Assert.Equal(230, result.Gains);
    }

    [Fact]
    public async Task CalculateCapitalGainLossAsync_SellAtLoss_CalculatesCapitalLoss()
    {
        // Arrange
        const string csvContent = """
                                  Code,Company,Date,Type,Quantity,Unit Price ($),Trade Value ($),Brokerage+GST ($),GST ($),Contract Note,Total Value ($)
                                  BHP,BHP Group,01/01/2023,Buy,100,10,1000,10,0,CN1,1010
                                  BHP,BHP Group,01/02/2024,Sell,-100,8,800,15,0,CN2,785
                                  """;
        var mockFile = CreateMockIFormFile(csvContent, "transactions.csv");

        // Act
        var results = await _capitalGainLossCalculatorService.CalculateCapitalGainLossAsync([mockFile]);
        var result = results.Single();

        // Assert
        // Capital loss = (Proportional Buy Trade Value + Brokerage (Buy + Sell)) - Sell Trade Value
        // Capital loss = (1000 + 10 + 15) - 800 = 1225 - 800 = 225
        Assert.Equal(0, result.Gains); // No gains in this scenario
        Assert.Equal(225, result.Losses); // Loss should be 225
    }
    
    [Fact]
    public async Task CalculateCapitalGainLossAsync_WhenSellAtLoss_ReturnsIsGainAsFalse()
    {
        // Arrange
        const string csvContent = """
                                  Code,Company,Date,Type,Quantity,Unit Price ($),Trade Value ($),Brokerage+GST ($),GST ($),Contract Note,Total Value ($)
                                  BHP,BHP Group,01/01/2023,Buy,100,10,1000,10,0,CN1,1010
                                  BHP,BHP Group,01/02/2024,Sell,-100,8,800,15,0,CN2,785
                                  """;
        var mockFile = CreateMockIFormFile(csvContent, "transactions.csv");

        // Act
        var results = await _capitalGainLossCalculatorService.CalculateCapitalGainLossAsync([mockFile]);
        var result = results.Single();

        Assert.False(result.IsGain);
    }
    
    [Fact]
    public async Task CalculateCapitalGainLossAsync_WhenSellAtGain_ReturnsIsGainAsTrue()
    {
        // Arrange
        const string csvContent = """
                                  Code,Company,Date,Type,Quantity,Unit Price ($),Trade Value ($),Brokerage+GST ($),GST ($),Contract Note,Total Value ($)
                                  BHP,BHP Group,01/01/2024,Buy,100,10,1000,10,0,CN1,1010
                                  BHP,BHP Group,01/06/2024,Sell,-100,15,1500,15,0,CN2,1485
                                  """;
        var mockFile = CreateMockIFormFile(csvContent, "transactions.csv");

        var results = await _capitalGainLossCalculatorService.CalculateCapitalGainLossAsync([mockFile]);
        var result = results.Single();

        Assert.True(result.IsGain);
    }

    [Fact]
    public async Task CalculateCapitalGainLossAsync_MultipleBuysBeforeSell_ShouldApplyFIFO()
    {
        // Arrange
        const string csvContent = """
                                  Code,Company,Date,Type,Quantity,Unit Price ($),Trade Value ($),Brokerage+GST ($),GST ($),Contract Note,Total Value ($)
                                  BHP,BHP Group,01/01/2023,Buy,50,10,500,5,0,CN1,505
                                  BHP,BHP Group,01/02/2024,Buy,50,12,600,5,0,CN2,605
                                  BHP,BHP Group,01/03/2024,Sell,-75,15,1125,10,0,CN3,1115
                                  """;
        var mockFile = CreateMockIFormFile(csvContent, "transactions.csv");

        // Act
        var results = await _capitalGainLossCalculatorService.CalculateCapitalGainLossAsync([mockFile]);
        var result = results.Single();

        // Assert
        // FIFO: First 50 shares at $10, next 25 shares at $12
        // First 50: Sell Price 743.33, Purchase Price 505, Gain 50% of 238.33 (CGT Discount)
        // Next 25: Sell Price 371.67, Purchase Price 302.5, Gain 69.17 (No CGT Discount)
        Assert.Equal(188.335m, result.Gains);
        Assert.Equal(0, result.Losses);
    }
    
    [Fact]
    public async Task CalculateCapitalGainLossAsync_SellMoreThanOwned_ShouldThrowInvalidOperationException()
    {
        // Arrange
        const string csvContent = """
                                  Code,Company,Date,Type,Quantity,Unit Price ($),Trade Value ($),Brokerage+GST ($),GST ($),Contract Note,Total Value ($)
                                  BHP,BHP Group,01/01/2023,Buy,50,10,500,5,0,CN1,505
                                  BHP,BHP Group,01/02/2024,Sell,-100,15,1500,10,0,CN2,1490
                                  """;
        var mockFile = CreateMockIFormFile(csvContent, "transactions.csv");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => _capitalGainLossCalculatorService.CalculateCapitalGainLossAsync([mockFile]));
        Assert.Equal("There are no holdings to sell.", exception.Message);
    }

    private static IFormFile CreateMockIFormFile(string content, string fileName)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(stream.Length);
        return mockFile.Object;
    }
}
