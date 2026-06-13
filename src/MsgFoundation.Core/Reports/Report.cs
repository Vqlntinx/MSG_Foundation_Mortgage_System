using System.Globalization;

namespace MsgFoundation.Core.Reports;

public abstract class Report
{
    protected Report(ReportType reportType)
    {
        ReportId = Guid.NewGuid();
        ReportType = reportType;
        GeneratedAt = DateTimeOffset.UtcNow;
    }

    public Guid ReportId { get; }

    public ReportType ReportType { get; }

    public DateTimeOffset GeneratedAt { get; }

    public abstract string Generate(CultureInfo? culture = null);

    protected static string FormatMoney(decimal value, CultureInfo culture) =>
        value.ToString("C2", culture);
}
