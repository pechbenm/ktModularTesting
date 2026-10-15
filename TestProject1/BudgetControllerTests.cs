using BudgetCalculator.Controllers;
using BudgetCalculator.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BudgetCalculator.Tests;

public class BudgetControllerTests
{
    private readonly BudgetController _sut = new();


    private static decimal GetTotal(IActionResult result)
    {
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);

        var property = ok.Value!.GetType().GetProperty("TotalAmount");
        Assert.NotNull(property);

        return Assert.IsType<decimal>(property!.GetValue(ok.Value));
    }

    // позитивные сценарии

    [Fact]
    public void Calculate_ValidRequestWithoutInsurance_ReturnsOkWith500()
    {
        var request = new BudgetRequest { DailyCost = 100, Days = 5, IncludeInsurance = false };

        var result = _sut.Calculate(request);

        Assert.Equal(500m, GetTotal(result));
    }


    [Fact]
    public void Calculate_WithInsurance_AddsFivePercent()
    {
        var request = new BudgetRequest { DailyCost = 100, Days = 5, IncludeInsurance = true };

        var result = _sut.Calculate(request);

        Assert.Equal(525m, GetTotal(result));
    }

    // негативные сценарии

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]           
    [InlineData(int.MinValue)]
    public void Calculate_NonPositiveDays_ReturnsBadRequest(int days)
    {
        var request = new BudgetRequest { DailyCost = 100, Days = days, IncludeInsurance = false };

        var result = _sut.Calculate(request);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);
        Assert.Equal("Количество дней должно быть больше нуля.", bad.Value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Calculate_ZeroDailyCost_ReturnsZero(bool includeInsurance)
    {
        var request = new BudgetRequest { DailyCost = 0, Days = 5, IncludeInsurance = includeInsurance };

        var result = _sut.Calculate(request);

        Assert.Equal(0m, GetTotal(result));
    }

    // округление / копейки

    public static TheoryData<decimal, int, bool, decimal> TotalCases => new()
    {
        { 100.50m, 3, false, 301.50m },   // пример из задания
        { 33.33m,  3, false, 99.99m  },
        { 0.10m,   3, false, 0.30m   },   // на double было бы 0.30000000000000004
        { 100m,    1, false, 100m    },   // минимальное допустимое Days
        { 200m,    1, true,  210m    },   // граница + страховка
    };

    [Theory]
    [MemberData(nameof(TotalCases))]
    public void Calculate_DecimalCosts_AreExactToTheCent(
        decimal dailyCost, int days, bool insurance, decimal expected)
    {
        var request = new BudgetRequest { DailyCost = dailyCost, Days = days, IncludeInsurance = insurance };

        var result = _sut.Calculate(request);

        Assert.Equal(expected, GetTotal(result));
    }
}
