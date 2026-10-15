namespace BudgetCalculator.Models;

public class BudgetRequest
{
    public decimal DailyCost { get; set; }
    public int Days { get; set; }
    public bool IncludeInsurance { get; set; } 
}
