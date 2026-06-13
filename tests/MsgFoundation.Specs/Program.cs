using MsgFoundation.Core.Domain;

var tests = new (string Name, Action Run)[]
{
    ("annual values are converted to weekly values", AnnualValuesAreConvertedToWeeklyValues),
    ("mortgage projection applies escrow and 28 percent rule", MortgageProjectionAppliesRules),
    ("weekly funds computation follows the report formula", WeeklyFundsComputationFollowsFormula),
    ("approved funding is deducted and oversized funding is rejected", FundingDecisionsUpdateRemainingAmount),
    ("reports include expected domain data", ReportsContainExpectedData),
    ("invalid financial values are rejected", InvalidValuesAreRejected)
};

var failures = new List<string>();

foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS: {test.Name}");
    }
    catch (Exception exception)
    {
        failures.Add($"{test.Name}: {exception.Message}");
        Console.WriteLine($"FAIL: {test.Name}");
    }
}

Console.WriteLine();
Console.WriteLine($"{tests.Length - failures.Count}/{tests.Length} specs passed.");

if (failures.Count > 0)
{
    Console.WriteLine(string.Join(Environment.NewLine, failures));
    Environment.ExitCode = 1;
}

return;

static void AnnualValuesAreConvertedToWeeklyValues()
{
    var investment = new Investment(
        "INV-1",
        "Test Investment",
        52_000m,
        new DateOnly(2026, 6, 1));
    var expense = new OperatingExpenseEstimate(
        10_400m,
        new DateOnly(2026, 6, 1));

    Spec.Equal(1_000m, investment.GetWeeklyEstimatedReturn());
    Spec.Equal(200m, expense.GetWeeklyOperatingExpense());
}

static void MortgageProjectionAppliesRules()
{
    var mortgage = CreateMortgage();
    var projection = MortgageWeeklyProjection.CalculateFrom(
        new DateOnly(2026, 6, 8),
        mortgage);

    Spec.Equal(70m, projection.WeeklyEscrowPayment);
    Spec.Equal(570m, projection.WeeklyTotalMortgageCost);
    Spec.Equal(420m, projection.BorrowerMaximumWeeklyPayment);
    Spec.Equal(420m, projection.BorrowerActualWeeklyPayment);
    Spec.Equal(150m, projection.WeeklyGrant);
}

static void WeeklyFundsComputationFollowsFormula()
{
    var computation = WeeklyFundsComputation.Create(
        new DateOnly(2026, 6, 8),
        [new Investment("INV-1", "Test Investment", 52_000m, new DateOnly(2026, 6, 1))],
        new OperatingExpenseEstimate(10_400m, new DateOnly(2026, 6, 1)),
        [CreateMortgage()]);

    Spec.Equal(1_000m, computation.WeeklyInvestmentIncome);
    Spec.Equal(200m, computation.WeeklyOperatingExpense);
    Spec.Equal(420m, computation.ExpectedMortgagePayments);
    Spec.Equal(150m, computation.ExpectedGrants);
    Spec.Equal(1_070m, computation.AmountAvailableAtStartOfWeek);
    Spec.Equal(1_070m, computation.RemainingAmountAvailable);
}

static void FundingDecisionsUpdateRemainingAmount()
{
    var foundation = CreateFoundation();
    var weekStartDate = new DateOnly(2026, 6, 8);
    foundation.ComputeWeeklyFunds(weekStartDate);

    var approvedRequest = new FundingRequest(
        "REQ-1",
        new DateOnly(2026, 6, 9),
        1_000m);
    var approvedDecision =
        foundation.EvaluateFundingRequest(weekStartDate, approvedRequest);

    Spec.True(approvedDecision.Approved);
    Spec.Equal(FundingRequestStatus.Approved, approvedRequest.Status);
    Spec.Equal(1_000m, approvedDecision.AmountDeducted);
    Spec.Equal(70m, approvedDecision.AmountRemainingAfterDecision);

    var rejectedRequest = new FundingRequest(
        "REQ-2",
        new DateOnly(2026, 6, 10),
        100m);
    var rejectedDecision =
        foundation.EvaluateFundingRequest(weekStartDate, rejectedRequest);

    Spec.False(rejectedDecision.Approved);
    Spec.Equal(FundingRequestStatus.Rejected, rejectedRequest.Status);
    Spec.Equal(0m, rejectedDecision.AmountDeducted);
    Spec.Equal(70m, rejectedDecision.AmountRemainingAfterDecision);
}

static void ReportsContainExpectedData()
{
    var foundation = CreateFoundation();
    var weekStartDate = new DateOnly(2026, 6, 8);
    foundation.ComputeWeeklyFunds(weekStartDate);

    var weeklyReport = foundation.GenerateWeeklyFundsReport(weekStartDate).Generate();
    var investmentReport = foundation.GenerateInvestmentListReport().Generate();
    var mortgageReport = foundation.GenerateMortgageListReport().Generate();

    Spec.Contains("2026-06-08", weeklyReport);
    Spec.Contains("INV-1", investmentReport);
    Spec.Contains("MTG-1", mortgageReport);
    Spec.Contains("Test", mortgageReport);
}

static void InvalidValuesAreRejected()
{
    Spec.Throws<ArgumentOutOfRangeException>(() =>
        new Investment(
            "INV-1",
            "Invalid",
            -1m,
            new DateOnly(2026, 6, 1)));

    Spec.Throws<ArgumentOutOfRangeException>(() =>
        new FundingRequest(
            "REQ-1",
            new DateOnly(2026, 6, 9),
            0m));
}

static Foundation CreateFoundation()
{
    var foundation = new Foundation(
        "MSG Foundation",
        new OperatingExpenseEstimate(10_400m, new DateOnly(2026, 6, 1)));
    foundation.SaveInvestment(new Investment(
        "INV-1",
        "Test Investment",
        52_000m,
        new DateOnly(2026, 6, 1)));
    foundation.SaveMortgage(CreateMortgage());
    return foundation;
}

static Mortgage CreateMortgage() =>
    new(
        accountNumber: "MTG-1",
        mortgageeLastName: "Test",
        originalPurchasePriceOfHome: 200_000m,
        dateMortgageIssued: new DateOnly(2025, 1, 1),
        weeklyPrincipalAndInterestPayment: 500m,
        currentCombinedGrossWeeklyIncome: 1_500m,
        combinedGrossWeeklyIncomeLastUpdatedDate: new DateOnly(2026, 6, 1),
        annualRealEstateTax: 2_600m,
        annualRealEstateTaxLastUpdatedDate: new DateOnly(2026, 1, 1),
        annualHomeownerInsurancePremium: 1_040m,
        homeownerInsurancePremiumLastUpdatedDate: new DateOnly(2026, 1, 1));

internal static class Spec
{
    public static void Equal<T>(T expected, T actual)
        where T : notnull
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected '{expected}', but found '{actual}'.");
        }
    }

    public static void True(bool value)
    {
        if (!value)
        {
            throw new InvalidOperationException("Expected true, but found false.");
        }
    }

    public static void False(bool value)
    {
        if (value)
        {
            throw new InvalidOperationException("Expected false, but found true.");
        }
    }

    public static void Contains(string expected, string actual)
    {
        if (!actual.Contains(expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected text to contain '{expected}'.");
        }
    }

    public static void Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Expected exception '{typeof(TException).Name}' was not thrown.");
    }
}
