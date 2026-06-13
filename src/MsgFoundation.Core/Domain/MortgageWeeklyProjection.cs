namespace MsgFoundation.Core.Domain;

public sealed class MortgageWeeklyProjection
{
    private MortgageWeeklyProjection(
        DateOnly weekStartDate,
        Mortgage mortgage,
        decimal weeklyEscrowPayment,
        decimal weeklyTotalMortgageCost,
        decimal borrowerMaximumWeeklyPayment,
        decimal borrowerActualWeeklyPayment,
        decimal weeklyGrant)
    {
        WeekStartDate = weekStartDate;
        Mortgage = mortgage;
        WeeklyEscrowPayment = weeklyEscrowPayment;
        WeeklyTotalMortgageCost = weeklyTotalMortgageCost;
        BorrowerMaximumWeeklyPayment = borrowerMaximumWeeklyPayment;
        BorrowerActualWeeklyPayment = borrowerActualWeeklyPayment;
        WeeklyGrant = weeklyGrant;
    }

    public DateOnly WeekStartDate { get; }

    public Mortgage Mortgage { get; }

    public decimal WeeklyEscrowPayment { get; }

    public decimal WeeklyTotalMortgageCost { get; }

    public decimal BorrowerMaximumWeeklyPayment { get; }

    public decimal BorrowerActualWeeklyPayment { get; }

    public decimal WeeklyGrant { get; }

    public static MortgageWeeklyProjection CalculateFrom(DateOnly weekStartDate, Mortgage mortgage)
    {
        ArgumentNullException.ThrowIfNull(mortgage);

        var weeklyEscrowPayment = mortgage.CalculateWeeklyEscrowPayment();
        var weeklyTotalMortgageCost =
            mortgage.WeeklyPrincipalAndInterestPayment + weeklyEscrowPayment;
        var borrowerMaximumWeeklyPayment = mortgage.CalculateBorrowerMaximumWeeklyPayment();
        var borrowerActualWeeklyPayment =
            Math.Min(weeklyTotalMortgageCost, borrowerMaximumWeeklyPayment);
        var weeklyGrant =
            Math.Max(0m, weeklyTotalMortgageCost - borrowerMaximumWeeklyPayment);

        return new MortgageWeeklyProjection(
            weekStartDate,
            mortgage,
            weeklyEscrowPayment,
            weeklyTotalMortgageCost,
            borrowerMaximumWeeklyPayment,
            borrowerActualWeeklyPayment,
            weeklyGrant);
    }
}
