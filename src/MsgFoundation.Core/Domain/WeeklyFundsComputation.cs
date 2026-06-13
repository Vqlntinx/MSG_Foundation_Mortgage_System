namespace MsgFoundation.Core.Domain;

public sealed class WeeklyFundsComputation
{
    private readonly List<FundingDecision> _fundingDecisions = [];

    private WeeklyFundsComputation(
        DateOnly computationWeekStartDate,
        decimal weeklyInvestmentIncome,
        decimal weeklyOperatingExpense,
        IReadOnlyList<MortgageWeeklyProjection> mortgageProjections)
    {
        ComputationWeekStartDate = computationWeekStartDate;
        WeeklyInvestmentIncome = weeklyInvestmentIncome;
        WeeklyOperatingExpense = weeklyOperatingExpense;
        MortgageProjections = mortgageProjections;
        ExpectedMortgagePayments = mortgageProjections.Sum(
            projection => projection.BorrowerActualWeeklyPayment);
        ExpectedGrants = mortgageProjections.Sum(projection => projection.WeeklyGrant);
        AmountAvailableAtStartOfWeek =
            WeeklyInvestmentIncome +
            ExpectedMortgagePayments -
            WeeklyOperatingExpense -
            ExpectedGrants;
        RemainingAmountAvailable = AmountAvailableAtStartOfWeek;
    }

    public const decimal WeeksPerYear = 52m;

    public DateOnly ComputationWeekStartDate { get; }

    public decimal WeeklyInvestmentIncome { get; }

    public decimal WeeklyOperatingExpense { get; }

    public decimal ExpectedMortgagePayments { get; }

    public decimal ExpectedGrants { get; }

    public decimal AmountAvailableAtStartOfWeek { get; }

    public decimal RemainingAmountAvailable { get; private set; }

    public IReadOnlyList<MortgageWeeklyProjection> MortgageProjections { get; }

    public IReadOnlyList<FundingDecision> FundingDecisions => _fundingDecisions;

    public static WeeklyFundsComputation Create(
        DateOnly computationWeekStartDate,
        IEnumerable<Investment> investments,
        OperatingExpenseEstimate operatingExpense,
        IEnumerable<Mortgage> mortgages)
    {
        ArgumentNullException.ThrowIfNull(investments);
        ArgumentNullException.ThrowIfNull(operatingExpense);
        ArgumentNullException.ThrowIfNull(mortgages);

        var investmentList = investments.ToList();
        var mortgageList = mortgages.ToList();

        if (investmentList.Any(investment => investment is null))
        {
            throw new ArgumentException("The investment collection cannot contain null values.", nameof(investments));
        }

        if (mortgageList.Any(mortgage => mortgage is null))
        {
            throw new ArgumentException("The mortgage collection cannot contain null values.", nameof(mortgages));
        }

        var weeklyInvestmentIncome =
            investmentList.Sum(investment => investment.GetWeeklyEstimatedReturn());
        var weeklyOperatingExpense = operatingExpense.GetWeeklyOperatingExpense();
        var mortgageProjections = mortgageList
            .Select(mortgage => MortgageWeeklyProjection.CalculateFrom(
                computationWeekStartDate,
                mortgage))
            .ToArray();

        return new WeeklyFundsComputation(
            computationWeekStartDate,
            weeklyInvestmentIncome,
            weeklyOperatingExpense,
            mortgageProjections);
    }

    public bool CanFund(decimal homeCost)
    {
        DomainValidation.Positive(homeCost, nameof(homeCost));
        return homeCost <= RemainingAmountAvailable;
    }

    internal void DeductApprovedHomeCost(decimal homeCost)
    {
        if (!CanFund(homeCost))
        {
            throw new InvalidOperationException("The requested home cost exceeds the remaining amount available.");
        }

        RemainingAmountAvailable -= homeCost;
    }

    internal void RecordDecision(FundingDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);
        _fundingDecisions.Add(decision);
    }
}
