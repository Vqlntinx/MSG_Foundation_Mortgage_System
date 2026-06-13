namespace MsgFoundation.Core.Domain;

public sealed class Mortgage
{
    public Mortgage(
        string accountNumber,
        string mortgageeLastName,
        decimal originalPurchasePriceOfHome,
        DateOnly dateMortgageIssued,
        decimal weeklyPrincipalAndInterestPayment,
        decimal currentCombinedGrossWeeklyIncome,
        DateOnly combinedGrossWeeklyIncomeLastUpdatedDate,
        decimal annualRealEstateTax,
        DateOnly annualRealEstateTaxLastUpdatedDate,
        decimal annualHomeownerInsurancePremium,
        DateOnly homeownerInsurancePremiumLastUpdatedDate)
    {
        AccountNumber = DomainValidation.Required(accountNumber, nameof(accountNumber));
        MortgageeLastName = DomainValidation.Required(mortgageeLastName, nameof(mortgageeLastName));
        OriginalPurchasePriceOfHome = DomainValidation.Positive(
            originalPurchasePriceOfHome,
            nameof(originalPurchasePriceOfHome));
        DateMortgageIssued = dateMortgageIssued;
        WeeklyPrincipalAndInterestPayment = DomainValidation.NonNegative(
            weeklyPrincipalAndInterestPayment,
            nameof(weeklyPrincipalAndInterestPayment));
        CurrentCombinedGrossWeeklyIncome = DomainValidation.NonNegative(
            currentCombinedGrossWeeklyIncome,
            nameof(currentCombinedGrossWeeklyIncome));
        CombinedGrossWeeklyIncomeLastUpdatedDate = combinedGrossWeeklyIncomeLastUpdatedDate;
        AnnualRealEstateTax = DomainValidation.NonNegative(
            annualRealEstateTax,
            nameof(annualRealEstateTax));
        AnnualRealEstateTaxLastUpdatedDate = annualRealEstateTaxLastUpdatedDate;
        AnnualHomeownerInsurancePremium = DomainValidation.NonNegative(
            annualHomeownerInsurancePremium,
            nameof(annualHomeownerInsurancePremium));
        HomeownerInsurancePremiumLastUpdatedDate = homeownerInsurancePremiumLastUpdatedDate;
    }

    public const decimal BorrowerIncomeRatio = 0.28m;

    public string AccountNumber { get; }

    public string MortgageeLastName { get; }

    public decimal OriginalPurchasePriceOfHome { get; }

    public DateOnly DateMortgageIssued { get; }

    public decimal WeeklyPrincipalAndInterestPayment { get; }

    public decimal CurrentCombinedGrossWeeklyIncome { get; }

    public DateOnly CombinedGrossWeeklyIncomeLastUpdatedDate { get; }

    public decimal AnnualRealEstateTax { get; }

    public DateOnly AnnualRealEstateTaxLastUpdatedDate { get; }

    public decimal AnnualHomeownerInsurancePremium { get; }

    public DateOnly HomeownerInsurancePremiumLastUpdatedDate { get; }

    public decimal CalculateWeeklyEscrowPayment() =>
        (AnnualRealEstateTax + AnnualHomeownerInsurancePremium) /
        WeeklyFundsComputation.WeeksPerYear;

    public decimal CalculateWeeklyTotalMortgageCost() =>
        WeeklyPrincipalAndInterestPayment + CalculateWeeklyEscrowPayment();

    public decimal CalculateBorrowerMaximumWeeklyPayment() =>
        CurrentCombinedGrossWeeklyIncome * BorrowerIncomeRatio;

    public decimal CalculateBorrowerActualWeeklyPayment() =>
        Math.Min(CalculateWeeklyTotalMortgageCost(), CalculateBorrowerMaximumWeeklyPayment());

    public decimal CalculateWeeklyGrant() =>
        Math.Max(0m, CalculateWeeklyTotalMortgageCost() - CalculateBorrowerMaximumWeeklyPayment());
}
