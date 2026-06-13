using System.Globalization;
using System.Text;
using MsgFoundation.Core.Domain;

namespace MsgFoundation.Core.Reports;

public sealed class MortgageListReport : Report
{
    private readonly IReadOnlyList<Mortgage> _mortgages;

    public MortgageListReport(IEnumerable<Mortgage> mortgages)
        : base(ReportType.MortgageList)
    {
        ArgumentNullException.ThrowIfNull(mortgages);
        _mortgages = mortgages
            .OrderBy(mortgage => mortgage.AccountNumber, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyList<Mortgage> Mortgages => _mortgages;

    public override string Generate(CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        var output = new StringBuilder();
        output.AppendLine("MSG Foundation - Mortgage List Report");
        output.AppendLine(
            "Account | Mortgagee | Home price | Issued | Weekly P&I | Weekly income | Annual tax | Annual insurance");

        if (_mortgages.Count == 0)
        {
            output.AppendLine("(none)");
        }
        else
        {
            foreach (var mortgage in _mortgages)
            {
                output.AppendLine(
                    $"{mortgage.AccountNumber} | " +
                    $"{mortgage.MortgageeLastName} | " +
                    $"{FormatMoney(mortgage.OriginalPurchasePriceOfHome, culture)} | " +
                    $"{mortgage.DateMortgageIssued:yyyy-MM-dd} | " +
                    $"{FormatMoney(mortgage.WeeklyPrincipalAndInterestPayment, culture)} | " +
                    $"{FormatMoney(mortgage.CurrentCombinedGrossWeeklyIncome, culture)} | " +
                    $"{FormatMoney(mortgage.AnnualRealEstateTax, culture)} | " +
                    $"{FormatMoney(mortgage.AnnualHomeownerInsurancePremium, culture)}");
            }
        }

        return output.ToString().TrimEnd();
    }
}
