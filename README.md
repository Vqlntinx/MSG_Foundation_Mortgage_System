# MSG Foundation Weekly Funds Computation System

MSG Foundation 사례 연구와 분석/설계 보고서를 바탕으로 작성한 C# 구현입니다.

## 프로젝트 구성

- `src/MsgFoundation.Core`: 도메인 모델, 주간 자금 계산, 지원 승인, 보고서
- `src/MsgFoundation.Console`: 전체 업무 흐름 실행 예제
- `tests/MsgFoundation.Specs`: 외부 테스트 패키지 없이 실행되는 핵심 규칙 검증

## 핵심 계산 규칙

```text
weeklyInvestmentIncome = sum(estimatedAnnualReturn / 52)
weeklyOperatingExpense = estimatedAnnualOperatingExpenses / 52
weeklyEscrowPayment = (annualRealEstateTax + annualInsurancePremium) / 52
weeklyTotalMortgageCost = weeklyP&I + weeklyEscrowPayment
borrowerMaximumWeeklyPayment = combinedGrossWeeklyIncome * 0.28
borrowerActualWeeklyPayment = min(totalMortgageCost, maximumWeeklyPayment)
weeklyGrant = max(0, totalMortgageCost - maximumWeeklyPayment)

amountAvailable =
    weeklyInvestmentIncome
    + expectedMortgagePayments
    - weeklyOperatingExpense
    - expectedGrants
```

승인된 주택 구매 요청의 `homeCost`는 해당 주의 `remainingAmountAvailable`에서
차감됩니다. 잔액보다 큰 요청은 거절되며 금액은 차감되지 않습니다.

## 실행

```powershell
dotnet build MsgFoundation.sln
dotnet run --project src/MsgFoundation.Console
dotnet run --project tests/MsgFoundation.Specs
```

현재 설치된 SDK에 맞춰 `net10.0`을 대상으로 합니다.
