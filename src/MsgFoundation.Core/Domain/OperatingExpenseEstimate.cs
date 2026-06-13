namespace MsgFoundation.Core.Domain;

public sealed class OperatingExpenseEstimate
{
    public OperatingExpenseEstimate(
        decimal estimatedAnnualOperatingExpenses,
        DateOnly operatingExpenseLastUpdatedDate)
    {
        EstimatedAnnualOperatingExpenses = DomainValidation.NonNegative(
            estimatedAnnualOperatingExpenses,
            nameof(estimatedAnnualOperatingExpenses));
        OperatingExpenseLastUpdatedDate = operatingExpenseLastUpdatedDate;
    }

    public decimal EstimatedAnnualOperatingExpenses { get; }

    public DateOnly OperatingExpenseLastUpdatedDate { get; }

    public decimal GetWeeklyOperatingExpense() =>
        EstimatedAnnualOperatingExpenses / WeeklyFundsComputation.WeeksPerYear;
}
