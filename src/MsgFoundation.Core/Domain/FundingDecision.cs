namespace MsgFoundation.Core.Domain;

public sealed class FundingDecision
{
    internal FundingDecision(
        string requestId,
        DateOnly decisionDate,
        bool approved,
        decimal amountAvailableBeforeDecision,
        decimal homeCost,
        decimal amountDeducted,
        decimal amountRemainingAfterDecision)
    {
        RequestId = requestId;
        DecisionDate = decisionDate;
        Approved = approved;
        AmountAvailableBeforeDecision = amountAvailableBeforeDecision;
        HomeCost = homeCost;
        AmountDeducted = amountDeducted;
        AmountRemainingAfterDecision = amountRemainingAfterDecision;
    }

    public string RequestId { get; }

    public DateOnly DecisionDate { get; }

    public bool Approved { get; }

    public decimal AmountAvailableBeforeDecision { get; }

    public decimal HomeCost { get; }

    public decimal AmountDeducted { get; }

    public decimal AmountRemainingAfterDecision { get; }
}
