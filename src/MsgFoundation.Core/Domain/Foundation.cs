using MsgFoundation.Core.Reports;

namespace MsgFoundation.Core.Domain;

public sealed class Foundation
{
    private readonly Dictionary<string, Investment> _investments =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Mortgage> _mortgages =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<DateOnly, WeeklyFundsComputation> _computations = [];
    private readonly Dictionary<string, FundingRequest> _fundingRequests =
        new(StringComparer.OrdinalIgnoreCase);

    public Foundation(string name, OperatingExpenseEstimate operatingExpenseEstimate)
    {
        Name = DomainValidation.Required(name, nameof(name));
        OperatingExpenseEstimate =
            operatingExpenseEstimate ?? throw new ArgumentNullException(nameof(operatingExpenseEstimate));
    }

    public string Name { get; }

    public OperatingExpenseEstimate OperatingExpenseEstimate { get; private set; }

    public IReadOnlyCollection<Investment> Investments => _investments.Values;

    public IReadOnlyCollection<Mortgage> Mortgages => _mortgages.Values;

    public IReadOnlyCollection<WeeklyFundsComputation> Computations => _computations.Values;

    public void SaveInvestment(Investment investment)
    {
        ArgumentNullException.ThrowIfNull(investment);
        _investments[investment.ItemNumber] = investment;
    }

    public bool RemoveInvestment(string itemNumber) =>
        _investments.Remove(DomainValidation.Required(itemNumber, nameof(itemNumber)));

    public void SetOperatingExpenseEstimate(OperatingExpenseEstimate operatingExpenseEstimate)
    {
        OperatingExpenseEstimate =
            operatingExpenseEstimate ?? throw new ArgumentNullException(nameof(operatingExpenseEstimate));
    }

    public void SaveMortgage(Mortgage mortgage)
    {
        ArgumentNullException.ThrowIfNull(mortgage);
        _mortgages[mortgage.AccountNumber] = mortgage;
    }

    public bool RemoveMortgage(string accountNumber) =>
        _mortgages.Remove(DomainValidation.Required(accountNumber, nameof(accountNumber)));

    public WeeklyFundsComputation ComputeWeeklyFunds(DateOnly weekStartDate)
    {
        if (_computations.TryGetValue(weekStartDate, out var existing) &&
            existing.FundingDecisions.Count > 0)
        {
            throw new InvalidOperationException(
                "A computation with funding decisions cannot be replaced.");
        }

        var computation = WeeklyFundsComputation.Create(
            weekStartDate,
            _investments.Values,
            OperatingExpenseEstimate,
            _mortgages.Values);

        _computations[weekStartDate] = computation;
        return computation;
    }

    public FundingDecision EvaluateFundingRequest(
        DateOnly weekStartDate,
        FundingRequest request,
        DateOnly? decisionDate = null)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_computations.TryGetValue(weekStartDate, out var computation))
        {
            throw new InvalidOperationException(
                $"No weekly funds computation exists for {weekStartDate:yyyy-MM-dd}.");
        }

        if (request.RequestDate < weekStartDate || request.RequestDate > weekStartDate.AddDays(6))
        {
            throw new ArgumentException(
                "The request date must fall within the computation week.",
                nameof(request));
        }

        if (_fundingRequests.ContainsKey(request.RequestId))
        {
            throw new InvalidOperationException(
                $"Funding request '{request.RequestId}' has already been evaluated.");
        }

        var effectiveDecisionDate = decisionDate ?? request.RequestDate;
        if (effectiveDecisionDate < request.RequestDate)
        {
            throw new ArgumentException(
                "The decision date cannot be earlier than the request date.",
                nameof(decisionDate));
        }

        var amountBeforeDecision = computation.RemainingAmountAvailable;
        FundingDecision decision;

        if (computation.CanFund(request.HomeCost))
        {
            computation.DeductApprovedHomeCost(request.HomeCost);
            request.MarkApproved();
            decision = new FundingDecision(
                request.RequestId,
                effectiveDecisionDate,
                approved: true,
                amountBeforeDecision,
                request.HomeCost,
                amountDeducted: request.HomeCost,
                computation.RemainingAmountAvailable);
        }
        else
        {
            request.MarkRejected();
            decision = new FundingDecision(
                request.RequestId,
                effectiveDecisionDate,
                approved: false,
                amountBeforeDecision,
                request.HomeCost,
                amountDeducted: 0m,
                computation.RemainingAmountAvailable);
        }

        _fundingRequests.Add(request.RequestId, request);
        computation.RecordDecision(decision);
        return decision;
    }

    public WeeklyFundsReport GenerateWeeklyFundsReport(DateOnly weekStartDate)
    {
        if (!_computations.TryGetValue(weekStartDate, out var computation))
        {
            throw new InvalidOperationException(
                $"No weekly funds computation exists for {weekStartDate:yyyy-MM-dd}.");
        }

        return new WeeklyFundsReport(computation);
    }

    public InvestmentListReport GenerateInvestmentListReport() =>
        new(_investments.Values);

    public MortgageListReport GenerateMortgageListReport() =>
        new(_mortgages.Values);
}
