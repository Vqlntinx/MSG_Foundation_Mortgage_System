namespace MsgFoundation.Core.Domain;

public sealed class FundingRequest
{
    public FundingRequest(string requestId, DateOnly requestDate, decimal homeCost)
    {
        RequestId = DomainValidation.Required(requestId, nameof(requestId));
        RequestDate = requestDate;
        HomeCost = DomainValidation.Positive(homeCost, nameof(homeCost));
        Status = FundingRequestStatus.Pending;
    }

    public string RequestId { get; }

    public DateOnly RequestDate { get; }

    public decimal HomeCost { get; }

    public FundingRequestStatus Status { get; private set; }

    internal void MarkApproved()
    {
        EnsurePending();
        Status = FundingRequestStatus.Approved;
    }

    internal void MarkRejected()
    {
        EnsurePending();
        Status = FundingRequestStatus.Rejected;
    }

    private void EnsurePending()
    {
        if (Status != FundingRequestStatus.Pending)
        {
            throw new InvalidOperationException("The funding request has already been evaluated.");
        }
    }
}
