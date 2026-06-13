namespace MsgFoundation.Core.Domain;

public sealed class Investment
{
    public Investment(
        string itemNumber,
        string itemName,
        decimal estimatedAnnualReturn,
        DateOnly estimatedAnnualReturnLastUpdatedDate)
    {
        ItemNumber = DomainValidation.Required(itemNumber, nameof(itemNumber));
        ItemName = DomainValidation.Required(itemName, nameof(itemName));
        EstimatedAnnualReturn = DomainValidation.NonNegative(
            estimatedAnnualReturn,
            nameof(estimatedAnnualReturn));
        EstimatedAnnualReturnLastUpdatedDate = estimatedAnnualReturnLastUpdatedDate;
    }

    public string ItemNumber { get; }

    public string ItemName { get; }

    public decimal EstimatedAnnualReturn { get; }

    public DateOnly EstimatedAnnualReturnLastUpdatedDate { get; }

    public decimal GetWeeklyEstimatedReturn() =>
        EstimatedAnnualReturn / WeeklyFundsComputation.WeeksPerYear;
}
