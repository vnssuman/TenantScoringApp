# Tenant Scoring App

## Rental agreement drafts

Owners can generate an agreement when sending a rental request from Map Tenant. The
owner enters the advance, unit number, permanent address and proposed signing date;
the registered owner and tenant names, phone numbers and emails, property details,
rent and lease dates are included in the PDF. Both parties can download it from the
request screen. Editing and resending a request generates a new PDF for the revised
terms. Existing uploaded agreements remain available until their request is edited.

The PDF is a **draft for review**, not an executed or electronically signed contract.
Acceptance in the app does not capture a signature. The parties should obtain legal
advice on the terms and applicable Indian state-specific stamp duty, registration and
execution requirements. Email delivery is not configured.

Generated files are stored in the API's `App_Data/RentalAgreements` directory, and
are served only after checking that the signed-in user is a party to the lease.
Deployments must persist and protect this directory and include it in backups.
Apply the EF Core migrations before deploying the updated API; development startup
applies migrations automatically.

PDF generation needs a TrueType font. It defaults to Arial on Windows and
DejaVu Sans on Linux; configure `RentalAgreements:FontPath` to an installed `.ttf`
font when needed. The chosen font must support the characters used in agreement
details. The PDF renderer does not provide complex-script shaping; check rendered
output before using non-Latin text.