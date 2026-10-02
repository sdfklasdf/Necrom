# Vendor & Secret Ownership Register

Status: PROVIDERS SELECTED FOR FUTURE MILESTONES / NO SECRET MATERIAL

Founder is the single project owner.

| Capability | Selection/state | Secret rule |
|---|---|---|
| Supabase | SELECTED_FOR_Q4 / not implemented | service-role/private credentials trusted-side only; never Unity client |
| RevenueCat | SELECTED_FOR_Q5 / not implemented | privileged/admin credentials trusted-side; only documented client-safe config may ship |
| Apple IAP | platform rail / not configured | signing/store credentials external to source |
| Google Play Billing | platform rail / not configured | signing/private service credentials external to source |
| AdMob | SELECTED_FOR_Q5 / not implemented | test/prod ad config separated; account credentials external |
| Sentry | SELECTED_FOR_Q5 / not implemented | admin/org credentials external; client-safe runtime config only where documented |
| PostHog | SELECTED_FOR_Q5 / not implemented | admin credentials external; client-safe project config only where documented |
| Toss Payments | OUT_OF_SCOPE | no credential path in current architecture |

Rules:
- no privileged secret in source, Figma or committed config.
- rotate/revoke exposed secrets before compatibility concerns.
- Unity receives only client-safe configuration.
- provider SDK objects do not become domain truth.

Evidence boundary:
No Q4/Q5 vendor account, secret store, key rotation, SDK installation, runtime ingestion or sandbox transaction is proven by this register.
