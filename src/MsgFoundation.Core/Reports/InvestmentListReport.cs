using System.Globalization;
using System.Text;
using MsgFoundation.Core.Domain;

namespace MsgFoundation.Core.Reports;

public sealed class InvestmentListReport : Report
{
    private readonly IReadOnlyList<Investment> _investments;

    public InvestmentListReport(IEnumerable<Investment> investments)
        : base(ReportType.InvestmentList)
    {
        ArgumentNullException.ThrowIfNull(investments);
        _investments = investments
            .OrderBy(investment => investment.ItemNumber, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyList<Investment> Investments => _investments;

    public override string Generate(CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        var output = new StringBuilder();
        output.AppendLine("MSG Foundation - Investment List Report");
        output.AppendLine("Item | Name | Estimated annual return | Last updated");

        if (_investments.Count == 0)
        {
            output.AppendLine("(none)");
        }
        else
        {
            foreach (var investment in _investments)
            {
                output.AppendLine(
                    $"{investment.ItemNumber} | " +
                    $"{investment.ItemName} | " +
                    $"{FormatMoney(investment.EstimatedAnnualReturn, culture)} | " +
                    $"{investment.EstimatedAnnualReturnLastUpdatedDate:yyyy-MM-dd}");
            }
        }

        return output.ToString().TrimEnd();
    }
}
