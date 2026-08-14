# DevPayAPI — Application Documentation

Low-level reference for the DevPay lending platform: a salary-backed lending
system where employees of onboarded companies borrow against verified income,
repay by direct-debit mandate, and are disbursed through ProvidusBank.

[§1](#1-end-to-end-process-summary) is a summary of the whole journey. Everything
after it is detail.

---

## 1. End-to-end process summary

```
BORROWER                          ADMIN                    EXTERNAL
────────                          ─────                    ────────
Step 1  email + personal info
Step 1B email OTP                                          → Email/SMS
Step 2  identity + bank account                            → Mono BVN/NIN
Step 2B identity OTP
Step 3  documents + address                                → Mono credit analysis
        └─ eligibility computed                               or Remita salary history
Step 4  pick amount & tenor
        accept offer letter
        └─ Loan created (Pending)
Step 5  mandate generated                                  → Mono / Remita
Step 6  mandate OTP requested
Step 6B mandate activated
        └─ application IsCompleted
                                  reviews loan
                                  approve → Approved
                                  send offer letter → OfferLetterSent
        signs & uploads                                    
                                  → OfferLetterSigned
                                  disburse  ──────────────→ Providus NIP transfer
                                  └─ Disbursed + Repayment schedule

        repayments collected over tenor                    ← Remita/Mono webhooks
```

**In prose.** A borrower is invited or self-registers against a company and a
loan product. They verify their email by OTP, then their identity (BVN or NIN)
and bank account, again by OTP. They upload documents, at which point the system
calls the active data provider to establish how much they can borrow — Mono
credit analysis or Remita salary history, depending on configuration. The
borrower then chooses an amount and tenor within that eligibility band and
accepts the offer letter, which creates the `Loan` in `Pending`.

Before the loan can move, a repayment mandate must exist: the system generates a
direct-debit mandate with Mono or Remita, the borrower authorises it with an OTP,
and only then is the onboarding application marked complete.

An admin reviews the pending loan and approves or rejects it. On approval the
offer letter is sent; the signed copy is uploaded and the loan reaches
`OfferLetterSigned`. An admin then disburses, which routes through the Providus
integration — bank-code translation, name enquiry, then an NIP transfer over the
site-to-site VPN. On success the loan becomes `Disbursed` and a repayment record
is created. Repayments are collected against the mandate over the tenor, with
inbound webhooks reconciling what was actually collected.

Two things worth noting up front: **the mandate is created during onboarding, not
at disbursement** — disbursement only asserts that one exists. And **eligibility
is computed once, at Step 3**, and stored on the application; Step 4 validates
the requested amount against those stored bounds.

---

## 2. Solution structure

| Project | Contains |
|---|---|
| `LendingSolution.API` | Controllers, DI wiring (`Extensions/ServiceExtensions.cs`), `Program.cs`, appsettings |
| `LendingSolution.Application` | Services (business logic), repositories, interfaces |
| `LendingSolution.Core` | Entities (`Models/`), DTOs, enums, settings POCOs |
| `LendingSolution.Infrastructure` | `ApplicationDbContext`, EF migrations, `DatabaseLoggerProvider` |

.NET 9. EF Core against Azure SQL. ASP.NET Identity for authentication, JWT
bearer tokens, API versioning via `api/v{version}/…`.

`Program.cs` applies migrations on startup (`db.Database.Migrate()`) and seeds
roles. See [§13](#13-known-issues-and-gotchas) for why that startup call matters.

---

## 3. Roles

Seeded in `Program.cs:98`:

`SuperAdmin`, `Admin`, `LoanOfficer`, `CollectionsOfficer`, `Underwriter`,
`SupportAgent`, `Auditor`, `Viewer`

Loan actions — approve, reject, process, send offer letter, disburse — are
restricted to `Admin,SuperAdmin` (`LoanController`). Cross-company visibility is
`SuperAdmin` only; `Admin` is scoped to its own company. The borrower onboarding
endpoints are **unauthenticated** — a borrower is not an Identity user, they are
a `BorrowerApplication` row progressed by OTP.

---

## 4. Domain model

| Entity | Role |
|---|---|
| `BorrowerApplication` | The onboarding journey. Holds identity, bank details, eligibility bounds, mandate ids, and `CurrentStep`. |
| `Loan` | The financial contract. Created at Step 4, then driven by `LoanStatus`. |
| `LoanProduct` | Interest rate, computation basis, fee percentages, amount and tenor bounds. |
| `Company` | Employer. Owns a wallet that pays per-transaction platform fees. |
| `Repayment` | Repayment schedule created at disbursement. |
| `Disbursement` | Legacy/manual disbursement record — **not written by the Providus path** ([§13](#13-known-issues-and-gotchas)). |
| `MonoMandateReference`, `RemitaSalaryHistory`, `MonoCreditAnalysisRecord` | Provider-specific artefacts. |
| `AuditLog` | Business-event audit trail. |
| `AppLog` | Application log sink, written by `DatabaseLoggerProvider`. |

All entities derive from `Base` (`Guid Id`, `CreatedAt`, `UpdatedAt`).

### `BorrowerOnboardingStep`

```
Step1_EmailSent              = 1
Step1B_EmailValidated        = 2
Step2_BvnSent                = 3
Step2B_BvnValidated          = 4
Step3_DocumentsUploaded      = 5
Step4_LoanSubmitted          = 6
Step5_MandateGenerated       = 7
Step6_MandateActivationPending = 8
Step6B_MandateActivated      = 9
```

### `LoanStatus`

```
Pending = 0   NotBooked = 1   Approved = 2   Rejected = 3   Disbursed = 4
Repaid  = 5   Overdue   = 6   Cancelled = 7  OfferLetterSent = 8
OfferLetterSigned = 9
```

---

## 5. Provider selection

A single configuration key switches the income/eligibility and mandate provider:

```jsonc
"ActiveDataProvider": "Mono"   // or "Remita"
```

Read at `BorrowerOnboardingService.cs:503` and again for mandate generation.

| Concern | Mono | Remita |
|---|---|---|
| Identity | BVN lookup + OTP, or NIN direct lookup | — |
| Eligibility | Credit analysis (`AnalyzeCreditHistoryAsync`) | Salary history |
| Mandate | e-mandate, variable debit | Direct debit (`SO`) |
| Bank codes | Supplies both CBN `bank_code` and NIBSS `nip_code` | — |

Providus is **not** switchable — it is the disbursement rail regardless.

---

## 6. Onboarding, step by step

All endpoints under `api/v{version}/borrower`. All unauthenticated.

### Step 1 — `POST /step1` → `Step1_SaveBorrowerInfoAsync`

Creates the `BorrowerApplication` against a company and loan product. Sends an
email OTP. → `Step1_EmailSent`.

### Step 1B — `POST /step1b` → `Step1B_ValidateEmailOtpAsync`

Validates the email OTP. → `Step1B_EmailValidated`.

### Step 2 — `POST /step2` → `Step2_SaveBvnAsync`

Takes `IdentityType` (`nin` | `bvn`), the identity number, `AccountNo` and
`BankCode`. Both bank fields are mandatory (`:206`).

**The raw BVN is never persisted** — only `GenerateBvnHash()` of it. This matters
downstream: Step 3 needs a real BVN for credit analysis, so the client must
re-send it.

- **NIN path**: `NinLookupAsync` resolves directly, no OTP. Jumps straight to
  `Step2B_BvnValidated`.
- **BVN path**: `BvnLookupAsync` opens a Mono session, `BvnVerifyAsync` sends an
  OTP to the borrower's alternate phone. → `Step2_BvnSent`.

`application.BankCode` is written here (`:229`, `:253`, `:308`) from the client
request. **It is stored in CBN format** (Sterling = `232`) — see
[§9](#9-bank-codes-two-namespaces).

Going back to Step 2 after progressing clears all subsequent step data.

### Step 2B — `POST /step2b` → `Step2B_ValidateBvnOtpAsync`

Validates the identity OTP. → `Step2B_BvnValidated`.

### Step 3 — `POST /step3` → `Step3_SaveBankAddressDocumentsAsync`

Address and documents, then **eligibility computation** — the substantive part.

**Mono path** (`:506-585`). Resolves which BVN to use for credit analysis:

| Situation | BVN used |
|---|---|
| `Mono:monoCreditHistoryBVN` configured | that test BVN (sandbox override) |
| NIN-verified (`MonoBvnSessionId` null) | the client-supplied BVN if valid, else none |
| BVN-verified | client-supplied BVN, **must hash-match** the Step 2 identity |

Then `AnalyzeCreditHistoryAsync(bvn, provider)` where provider defaults to `xds`.

```
if creditAnalysis is null or RecommendedAction == "Decline":
    minLoanEligible = product.MinAmount > 0 ? product.MinAmount : 1_000
    maxLoanEligible = 50_000                    ← default test eligibility
else:
    monoMax        = product.MaxAmount > 0 ? min(analysis.MaxLoanAmount, product.MaxAmount)
                                           : analysis.MaxLoanAmount
    minLoanEligible = product.MinAmount > 0 ? product.MinAmount : 1_000
    maxLoanEligible = max(monoMax, minLoanEligible)
```

> A declined credit analysis does **not** stop the application — it falls through
> to a ₦50,000 default band. That is test behaviour and should be reviewed before
> production lending.

**Remita path** (`:588+`). `GetSalaryHistoryAsync(accountNo, bankCode, bvn, appId)`.
A null response or `status != "success"` throws — unlike the Mono path, this one
does block.

→ `Step3_DocumentsUploaded`, with `MinLoanEligible`, `MaxLoanEligible`,
`MinTenor`, `MaxTenor` persisted on the application.

### Step 4 — `POST /step4` → `Step4_SubmitLoanApplicationAsync`

Creates the `Loan`. Guards, in order (`:682-713`):

1. `AcceptOfferLetter` must be true
2. `CurrentStep >= Step3_DocumentsUploaded`
3. not already `IsCompleted`
4. `LoanAmount` within `[MinLoanEligible, MaxLoanEligible]`
5. `Tenor > 0` and within `[MinTenor, MaxTenor]`
6. **company wallet must cover the email fee** — else `503` with a generic
   message, deliberately not disclosing the billing reason to the borrower

Then the money math ([§7](#7-loan-arithmetic)), and a `Loan` is created in
`Pending` with the computed amounts. → `Step4_LoanSubmitted`,
`IsOfferLetterAccepted = true`. **`IsCompleted` stays false** until the mandate is
activated.

### Step 5 — mandate generation

Not a separate endpoint; triggered after Step 4.

**Mono** (`:1010-1058`):

```csharp
MandateType = "emandate", DebitType = "variable",
Amount      = (int)((loan.TotalRepayment ?? loan.Amount) * 100),   // kobo
StartDate   = DateTime.UtcNow.AddHours(1),                          // WAT
EndDate     = start.AddMonths(loan.DurationInMonths)
```

Two subtleties preserved in comments there:

- **`Amount` is the total ceiling** debitable across the whole mandate window, not
  the per-debit amount. It is set to the full repayment so every instalment can
  be collected.
- **`StartDate` is `UtcNow + 1h`** because Mono validates against Nigerian local
  time (WAT, UTC+1). Near midnight UTC a plain `UtcNow` date reads as yesterday
  in WAT and Mono rejects it as past-dated. Nigeria has no DST, so a fixed offset
  is safe.

Sets `loan.MandateRef`, `loan.IsMandateCreated`, → `Step5_MandateGenerated`, and
returns Mono's authorisation URL for the borrower to complete.

**Remita** (`:1063+`) — see [§13](#13-known-issues-and-gotchas); this path is
currently pinned to sandbox test values.

### Step 6 / 6B — `POST /step4b` → `Step4B_ActivateMandateAsync`

Validates the mandate OTP via `ValidateMandateAuthorizationAsync`; requires
`StatusCode == "00"`. → `Step6B_MandateActivated`, `IsCompleted = true`. Audit:
`MandateActivated`.

### Supporting endpoints

`resend-step1-email-otp`, `resend-step2-bvn-otp`, `generate-email-otp`,
`validate-email-otp`, `generate-bvn-otp`, `current-step`, `GET /{emailOrId}`,
`PUT /update-documents`, `POST /{id}/upload-signed-offer-letter`.

---

## 7. Loan arithmetic

Computed once at Step 4 (`:735-792`) and persisted on the `Loan`. Nothing
recalculates later — disbursement and repayment read the stored values.

```
LP  Loan Principal      = requested amount
AI  Applied Interest    = per computation basis below
RA  Total Repayment     = LP + AI
AF  Applicable Fees     = processing + maintenance + legal
AtD Amount to Disburse  = LP − AF          ← what actually leaves the bank
```

**Monthly rate.** If `InterestCostComputation == PerMonth` the product rate is
already monthly; otherwise it is annual and divided by 12. Then `/100` to a
decimal.

**Flat basis:**
```
AI               = LP × monthlyRate × tenor
monthlyRepayment = (LP + AI) / tenor
```

**Reducing balance (amortised):**
```
monthlyRepayment = LP × [r(1+r)ⁿ] / [(1+r)ⁿ − 1]
AI               = (monthlyRepayment × n) − LP
```
with `r` = monthly rate, `n` = tenor. Zero-rate short-circuits to `LP / tenor`.

**Fees:**
```
processing  = LP × (ProcessingFeePercent/100) + ProcessingFeeFlat
maintenance = LP × (MaintenanceFeePercent/100)
legal       = LP × (LegalFeePercent/100)      + LegalFeeFlat
```

All rounded to 2 dp. Note the borrower repays on **LP**, but receives **LP − AF**
— fees are deducted at source, not added to the balance.

---

## 8. Loan lifecycle

```
Pending ──approve──→ Approved ──send offer──→ OfferLetterSent
   │                                                │
   └──reject──→ Rejected                       upload signed
                                                    ▼
                                            OfferLetterSigned
                                                    │
                                                 disburse
                                                    ▼
                                                Disbursed → Repaid / Overdue
```

### Approval — `POST /loan/{id}/approve` (`Admin,SuperAdmin`)

`ApproveLoan` (`LoanService.cs:151`). Rejects if already approved / sent /
signed, if rejected, or if disbursed. Sets `Approved`, `ApprovedAt`,
`ApprovedBy`, `Reason`, and `DueDate = UtcNow + tenor`. Audit: `LoanApproved`.

> `DueDate` is set here **and again at disbursement**. The approval value is a
> placeholder; disbursement overwrites it with the real one.

The offer letter is *not* sent by approval — the comment at `:200` notes it moved
into onboarding Step 4.

### Rejection — `POST /loan/{id}/reject`

Cannot reject an approved or disbursed loan. Audit: `LoanRejected`.

### Offer letter — `POST /loan/{id}/send-offer-letter`

`SendOfferLetterAsync` (`:608`). Requires `Approved` or `OfferLetterSent`
(resend). Emails the letter, → `OfferLetterSent`. Response advertises a 7-day
expiry, though **nothing enforces it**.

Signed copy: `POST /borrower/{id}/upload-signed-offer-letter` →
`OfferLetterSigned`.

### Disbursement — `POST /loan/{id}/disburse`

`DisburseLoanAsync` (`:700`). Accepts `Approved` **or** `OfferLetterSigned` — the
signed letter is not strictly required. Detailed in [§10](#10-disbursement).

---

## 9. Bank codes: two namespaces

`BorrowerApplication.BankCode` holds a **CBN code** (Sterling `232`, Providus
`101`), captured from the client at Step 2.

Providus `NIPFundTransfer` requires a **6-digit NIBSS code** (Sterling `000001`,
Providus `000023`). The two are unrelated numbering systems — there is no
arithmetic between them.

**The column is deliberately not migrated.** Mono's mandate creation
(`MonoService.cs:236`) reads the same field and expects the CBN value; rewriting
it would fix disbursement and silently break every direct-debit mandate.
Translation happens at disbursement only, via `IBankCodeResolver`.

`BankCodeResolver` is backed by Mono's `/v3/banks/list`, which returns both codes
on every entry, so the mapping is authoritative rather than name-matched — name
matching would be unsafe given entries like `STERLING BANK` vs `STERLING MOBILE`.
Cached 24h, singleton, and serves a stale list rather than blocking a
disbursement if Mono is unreachable.

Bank list endpoints: `misc/static-banks` (140 hardcoded CBN entries),
`mono/banks` (live, both codes), and `misc/banks` — which **throws
`NotImplementedException`** (`RemitaService.cs:554`) and should not be built on.

---

## 10. Disbursement

Detail in [`providus-site-to-site-vpn.md`](./providus-site-to-site-vpn.md) for
the network layer. Application flow:

1. Load loan; require `Approved` or `OfferLetterSigned`
2. Read beneficiary from `BorrowerApplication`; require `AccountNo` + `BankCode`
3. Require `TotalRepayment` / `MonthlyRepayment`
4. Require `IsMandateCreated` + `MandateRef`
5. **Reconciliation gate** — abort `409` if `DisbursementOutcomeUnknown`
6. Amount = `DisbursementAmount ?? Amount`
7. **Persist `DisbursementTransactionRef` before any network call**; reuse on retry
8. Resolve CBN → NIP code; unresolvable ⇒ fail, never send an unmapped code
9. Route: NIP code == `ProvidusNipBankCode` (`000023`) ⇒ intra-bank
   `ProvidusFundTransfer`; otherwise `NIPFundTransfer`
10. **NIP path only**: name enquiry via `GetNIPAccount`; use the bank's returned
    `accountName` as `beneficiaryAccountName`
11. Transfer
12. `7709` (duplicate reference) ⇒ requery `GetNIPTransactionStatus`, do not resend
13. Timeout/connection failure ⇒ `IsIndeterminate` ⇒ latch
    `DisbursementOutcomeUnknown`, throw `502`
14. Success ⇒ `Disbursed`, set date/reference/due date, create `Repayment`, audit,
    email

### Idempotency

Providus deduplicates on `transactionReference` and returns `7709` for a repeat.
That only protects you if the reference is **stable across retries**, which is
why it is persisted before the call. A fresh reference per attempt would defeat
the guard entirely and risk paying twice.

`DisbursementOutcomeUnknown` exists because a timeout is not a failure — the
debit may have landed. The loan is latched until someone reconciles the reference
against `GetNIPTransactionStatus`.

### Providus configuration

```jsonc
"Providus": {
  "BaseUrl": "http://10.0.2.4:8888",          // VPN proxy, VNet-only
  "FundTransferEndpoint":         "/postingrest/ProvidusFundTransfer",
  "NipFundTransferEndpoint":      "/postingrest/NIPFundTransfer",
  "NipAccountEnquiryEndpoint":    "/postingrest/GetNIPAccount",
  "NipTransactionStatusEndpoint": "/postingrest/GetNIPTransactionStatus",
  "NipBanksEndpoint":             "/postingrest/GetNIPBanks",
  "ProvidusNipBankCode": "000023",
  "UseMockMode": true,
  "TimeoutSeconds": 300
}
```

`UseMockMode` returns canned responses; amounts ending `.99` simulate failure and
`.98` simulate timeout.

---

## 11. Repayment and collections

`Repayment` is created at disbursement with `TotalDue`, `TotalRepaid = 0`,
`AmountUnpaid`, status `Active`.

Inbound webhooks (`WebhookController`, unauthenticated by design, Mono verified
by `mono-webhook-secret` header):

| Endpoint | Purpose |
|---|---|
| `POST /webhook/remita/collection` | Remita collection notification → `ProcessLoanCollectionNotificationAsync` |
| `POST /webhook/mono` | Mono mandate/debit events |

Admin operations: `POST /loan/{id}/stop-collection` (stops the mandate) and
`POST /loan/{id}/reconcile`.

---

## 12. Fees, wallets, audit and logging

**Platform fees** are charged to the **company's** wallet, not the borrower's —
`Settings` holds `OtpFee` (default ₦20), `EmailFee`, `DocumentationFee`, each
`FIXED` or percentage. Step 4 validates the balance before proceeding
(`ValidateCompanyBalanceForFeeAsync`).

Loan fees (processing/maintenance/legal) are a different thing entirely — they
come from `LoanProduct` and are deducted from the borrower's principal
([§7](#7-loan-arithmetic)).

**Audit** (`AuditLogs`): `LoanApproved`, `LoanRejected`, `OfferLetterAccepted`,
`MandateActivated`, `LoanDisbursed`. Each carries actor, entity, company, a
free-text `Details`, and an optional `Amount`.

**Application logs** (`AppLogs`): `DatabaseLoggerProvider` registered at
`Program.cs:14` with `LogLevel.Information`, batched and flushed every 5s or at
50 entries. Every insert is wrapped in a swallowing `try/catch` — so if the table
is absent, logging fails **silently**.

---

## 13. Known issues and gotchas

Ordered by how much damage they can do.

1. **The Remita mandate path is pinned to sandbox test data**
   (`BorrowerOnboardingService.cs:1070-1078`): `PayerName = "John Doe"`,
   `PayerEmail = "john.doe@mailinator.com"`, `PayerBankCode = "057"`,
   `PayerAccountNumber = "0100034932"`, `Amount = 10000`. The borrower's real
   details are commented out. **A Remita mandate created in production would be
   against DevPay's own account, not the borrower's.** Must be fixed before
   Remita goes live. The Mono path uses real borrower data and is unaffected.

2. **Production runs as `ASPNETCORE_ENVIRONMENT=Development`.**
   `appsettings.Development.json` holds the production connection string, and
   `Program.cs:82` swallows startup migration failures in Development but
   rethrows otherwise. Correcting the environment variable without first
   reconciling migrations would stop the app booting.

3. **`prod-column-reconcile.sql` creates schema drift by design.** It adds
   columns but writes **no** `__EFMigrationsHistory` rows, so the schema runs
   ahead of the history and EF replays applied work, failing with SQL error 2705.
   It also cannot create a *table* — every statement is guarded on the table
   already existing — so a table introduced by a migration that never truly ran
   is silently absent. That is how `AppLogs` went missing while its migration was
   recorded as applied.

4. **The `Disbursements` table is not written by the Providus path.** Only
   `FinanceService.cs:341` inserts into it. Reporting built on that table will
   show nothing for real disbursements, and `FinanceService`'s "already disbursed"
   totals ignore them.

5. **The NIBSS `sessionId` is logged but never persisted.** It is the identifier
   the bank uses to trace a transfer. Answering "did this go out, and to whom?"
   currently means a `LIKE` search over `AppLogs.Message`, since `AppLogs` has no
   entity id to join on.

6. **A declined Mono credit analysis does not block the application** — it falls
   back to a ₦50,000 default band (`:568-569`).

7. **Offer letter expiry is advertised but not enforced** — the 7 days in the
   response is cosmetic.

8. **Disbursement accepts `Approved`**, so a loan can be disbursed without a
   signed offer letter.

9. **`misc/banks` throws `NotImplementedException`** — a live endpoint that
   always 500s.

10. **Beneficiary account numbers are written to `AppLogs` in plain text** now
    that the Providus request/response logging sits at `Information`. Credentials
    are masked; account numbers and names are not.

---

## 14. Configuration reference

| Key | Purpose |
|---|---|
| `ActiveDataProvider` | `Mono` or `Remita` — eligibility and mandate provider |
| `ConnectionStrings:DefaultConnection` | Azure SQL |
| `Providus:*` | Disbursement — see [§10](#10-disbursement) |
| `Mono:SecretKey`, `Mono:BaseUrl` | Mono API |
| `Mono:CreditHistoryProvider` | Credit bureau, default `xds` |
| `Mono:monoCreditHistoryBVN` | Sandbox BVN override — **must be unset in production** |
| `Remita:*` | Remita API |
| `Embedly:*` | Wallet provider |
| `Logging:LogLevel:Default` | Gates `AppLogs` writes |

---

## 15. Related documents

| Document | Covers |
|---|---|
| [`providus-site-to-site-vpn.md`](./providus-site-to-site-vpn.md) | VPN, proxy, IPsec, bank negotiation |
| `prod-disbursement-idempotency.sql` | Migration chain reconciliation |
| `prod-check-missing-tables.sql` | Schema drift detection |
| `prod-create-applogs.sql` | Creates the missing `AppLogs` table |
| `reset-loan-disbursement.sql` | Resets a loan for disbursement re-testing |

Providus API docs: https://developer.providusbank.com/transfer-services
