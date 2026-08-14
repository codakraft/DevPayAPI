/*
    Reset a loan from Disbursed back to Approved so disbursement can be re-run.
    Intended for testing the Providus disbursement path.

    Each statement below is self-contained: no variables, no transaction. You can
    run them one at a time or all together and the behaviour is identical. The loan
    is located by the borrower's account number every time.

    !! READ BEFORE RUNNING STEP 2 !!

    Step 2 clears DisbursementTransactionRef, the idempotency key. Providus
    deduplicates on that reference (response 7709), so clearing it means the next
    attempt goes out under a reference Providus has never seen, and will be
    processed as a fresh transfer.

    That is only safe if NO REAL TRANSFER was ever made for this loan. It is safe
    for a loan disbursed while Providus:UseMockMode was true, because no funds moved.

    If in doubt, query GetNIPTransactionStatus with the loan's current
    DisbursementTransactionRef first: responseCode 01 ("transaction not found")
    means nothing moved and the reset is safe; 00 means funds WERE sent and you
    must not clear the reference.

    The audit row is left in place — it records something that happened. The
    disbursement email has already been sent and cannot be recalled.

    Account 0084689637 is the one name enquiry resolved to VALENTINA IFEDAYO
    ATEWOGBADE. Change it in each statement if you mean a different borrower.
*/

-- ── STEP 1: inspect ──────────────────────────────────────────────────────────
-- Status 2 = Approved, 4 = Disbursed, 9 = OfferLetterSigned.
-- If this returns no rows, the account number is wrong or the borrower record is
-- not linked to a loan — stop and check before going further.

SELECT      l.[Id], l.[Status], l.[DisbursementDate], l.[DisbursementReference],
            l.[DisbursementTransactionRef], l.[DisbursementOutcomeUnknown],
            l.[DueDate], ba.[AccountNo], ba.[BankCode]
FROM        [Loans] l
INNER JOIN  [BorrowerApplications] ba ON ba.[LoanId] = l.[Id]
WHERE       ba.[AccountNo] = N'0084689637';


-- ── STEP 2: reset the loan ───────────────────────────────────────────────────
-- Sets Approved and clears every disbursement field. Offer-letter fields are
-- untouched, so the loan keeps its signing state.

UPDATE  l
SET     l.[Status]                      = 2,
        l.[DisbursementDate]            = NULL,
        l.[DisbursementReference]       = NULL,
        l.[DueDate]                     = NULL,
        l.[DisbursementTransactionRef]  = NULL,
        l.[DisbursementOutcomeUnknown]  = 0
FROM        [Loans] l
INNER JOIN  [BorrowerApplications] ba ON ba.[LoanId] = l.[Id]
WHERE       ba.[AccountNo] = N'0084689637';

-- Expect 1. If this prints 0, STEP 1 returned nothing and nothing was changed.
PRINT 'Loans rows updated: ' + CONVERT(nvarchar(10), @@ROWCOUNT);


-- ── STEP 3: drop the repayment schedule ──────────────────────────────────────
-- DisburseLoanAsync recreates this on the next run; leaving it would duplicate.

DELETE      r
FROM        [Repayments] r
INNER JOIN  [BorrowerApplications] ba ON ba.[LoanId] = r.[LoanId]
WHERE       ba.[AccountNo] = N'0084689637';

PRINT 'Repayment rows deleted: ' + CONVERT(nvarchar(10), @@ROWCOUNT);


-- ── STEP 4: verify ───────────────────────────────────────────────────────────
-- Expect Status 2, the four disbursement columns NULL, and RepaymentRows 0.

SELECT      l.[Id], l.[Status], l.[DisbursementDate], l.[DisbursementReference],
            l.[DisbursementTransactionRef], l.[DisbursementOutcomeUnknown],
            l.[DueDate],
            (SELECT COUNT(*) FROM [Repayments] r WHERE r.[LoanId] = l.[Id]) AS [RepaymentRows]
FROM        [Loans] l
INNER JOIN  [BorrowerApplications] ba ON ba.[LoanId] = l.[Id]
WHERE       ba.[AccountNo] = N'0084689637';
