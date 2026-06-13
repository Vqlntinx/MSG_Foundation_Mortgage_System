using System.Globalization;
using System.Text;
using MsgFoundation.Core.Domain;

namespace MsgFoundation.Core.Reports;

public sealed class WeeklyFundsReport : Report
{
    public WeeklyFundsReport(WeeklyFundsComputation computation)
        : base(ReportType.WeeklyFunds)
    {
        Computation = computation ?? throw new ArgumentNullException(nameof(computation));
    }

    public WeeklyFundsComputation Computation { get; }

    public override string Generate(CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        var output = new StringBuilder();
        output.AppendLine("MSG Foundation - Weekly Funds Report");
        output.AppendLine($"Week start: {Computation.ComputationWeekStartDate:yyyy-MM-dd}");
        output.AppendLine($"Weekly investment income: {FormatMoney(Computation.WeeklyInvestmentIncome, culture)}");
        output.AppendLine($"Weekly operating expense: {FormatMoney(Computation.WeeklyOperatingExpense, culture)}");
        output.AppendLine($"Expected mortgage payments: {FormatMoney(Computation.ExpectedMortgagePayments, culture)}");
        output.AppendLine($"Expected grants: {FormatMoney(Computation.ExpectedGrants, culture)}");
        output.AppendLine($"Amount available at start: {FormatMoney(Computation.AmountAvailableAtStartOfWeek, culture)}");
        output.AppendLine($"Remaining amount available: {FormatMoney(Computation.RemainingAmountAvailable, culture)}");
        output.AppendLine();
        output.AppendLine("Mortgage projections");

        if (Computation.MortgageProjections.Count == 0)
        {
            output.AppendLine("(none)");
        }
        else
        {
            foreach (var projection in Computation.MortgageProjections)
            {
                output.AppendLine(
                    $"{projection.Mortgage.AccountNumber} | " +
                    $"{projection.Mortgage.MortgageeLastName} | " +
                    $"payment {FormatMoney(projection.BorrowerActualWeeklyPayment, culture)} | " +
                    $"grant {FormatMoney(projection.WeeklyGrant, culture)}");
            }
        }

        output.AppendLine();
        output.AppendLine("Funding decisions");

        if (Computation.FundingDecisions.Count == 0)
        {
            output.AppendLine("(none)");
        }
        else
        {
            foreach (var decision in Computation.FundingDecisions)
            {
                output.AppendLine(
                    $"{decision.RequestId} | " +
                    $"{(decision.Approved ? "APPROVED" : "REJECTED")} | " +
                    $"home cost {FormatMoney(decision.HomeCost, culture)} | " +
                    $"remaining {FormatMoney(decision.AmountRemainingAfterDecision, culture)}");
            }
        }

        return output.ToString().TrimEnd();
    }
}
