using System.Globalization;
using MsgFoundation.Core.Domain;

var foundation = new Foundation(
    "Martha Stockton Greengage Foundation",
    new OperatingExpenseEstimate(
        estimatedAnnualOperatingExpenses: 520_000m,
        operatingExpenseLastUpdatedDate: new DateOnly(2026, 6, 1)));

foundation.SaveInvestment(new Investment(
    itemNumber: "INV-001",
    itemName: "Treasury Bond Portfolio",
    estimatedAnnualReturn: 1_040_000m,
    estimatedAnnualReturnLastUpdatedDate: new DateOnly(2026, 6, 1)));

foundation.SaveInvestment(new Investment(
    itemNumber: "INV-002",
    itemName: "Municipal Bond Portfolio",
    estimatedAnnualReturn: 520_000m,
    estimatedAnnualReturnLastUpdatedDate: new DateOnly(2026, 6, 1)));

foundation.SaveMortgage(new Mortgage(
    accountNumber: "MTG-1001",
    mortgageeLastName: "Kim",
    originalPurchasePriceOfHome: 250_000m,
    dateMortgageIssued: new DateOnly(2024, 3, 15),
    weeklyPrincipalAndInterestPayment: 650m,
    currentCombinedGrossWeeklyIncome: 2_000m,
    combinedGrossWeeklyIncomeLastUpdatedDate: new DateOnly(2026, 5, 30),
    annualRealEstateTax: 3_120m,
    annualRealEstateTaxLastUpdatedDate: new DateOnly(2026, 1, 2),
    annualHomeownerInsurancePremium: 1_040m,
    homeownerInsurancePremiumLastUpdatedDate: new DateOnly(2026, 1, 2)));

foundation.SaveMortgage(new Mortgage(
    accountNumber: "MTG-1002",
    mortgageeLastName: "Lee",
    originalPurchasePriceOfHome: 180_000m,
    dateMortgageIssued: new DateOnly(2025, 8, 20),
    weeklyPrincipalAndInterestPayment: 420m,
    currentCombinedGrossWeeklyIncome: 2_200m,
    combinedGrossWeeklyIncomeLastUpdatedDate: new DateOnly(2026, 6, 2),
    annualRealEstateTax: 2_080m,
    annualRealEstateTaxLastUpdatedDate: new DateOnly(2026, 1, 2),
    annualHomeownerInsurancePremium: 780m,
    homeownerInsurancePremiumLastUpdatedDate: new DateOnly(2026, 1, 2)));

var weekStartDate = new DateOnly(2026, 6, 8);
foundation.ComputeWeeklyFunds(weekStartDate);

var request = new FundingRequest(
    requestId: "REQ-2026-001",
    requestDate: new DateOnly(2026, 6, 10),
    homeCost: 15_000m);

var decision = foundation.EvaluateFundingRequest(weekStartDate, request);

Console.WriteLine(
    $"Funding request {decision.RequestId}: " +
    $"{(decision.Approved ? "APPROVED" : "REJECTED")}");
Console.WriteLine();
Console.WriteLine(
    foundation.GenerateWeeklyFundsReport(weekStartDate)
        .Generate(CultureInfo.GetCultureInfo("en-US")));
Console.WriteLine();
Console.WriteLine(
    foundation.GenerateInvestmentListReport()
        .Generate(CultureInfo.GetCultureInfo("en-US")));
Console.WriteLine();
Console.WriteLine(
    foundation.GenerateMortgageListReport()
        .Generate(CultureInfo.GetCultureInfo("en-US")));
