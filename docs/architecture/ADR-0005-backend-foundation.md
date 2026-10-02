# ADR-0005 — Backend / Auth / Database foundation

Status: SELECTED_FOR_Q4 / NOT IMPLEMENTED
Date: 2026-10-03
Mode: QUALITY_GAME

## Decision
Select Supabase as the first managed backend for Q4 Backend / Online Foundation.

Intended Q4 capabilities:
- Supabase Auth for account/session identity.
- PostgreSQL for remote player/profile/progression state and transaction-safe records.
- Row Level Security where direct client data access is appropriate.
- trusted server/Edge Function boundaries for privileged mutations, webhooks, entitlement reconciliation and ad reward verification.
- cloud-save/version records and conflict metadata.

## Why now
The project has reached a point where future cloud save, purchase webhooks, server-verified rewards and multi-device authority need a concrete trusted backend. Selecting it now prevents designing payment/idempotency against an imaginary runtime.

## What is NOT decided yet
- exact schema.
- exact cloud-save merge policy.
- idempotency retention TTL.
- lock/queue technology.
- anti-cheat scope.
- realtime multiplayer architecture.
- production capacity plan.

Those are decided from Q4 implementation evidence, not before.

## Trust boundary
Unity remains untrusted for privileged authority.
Never ship service-role/private credentials in the client.
Gameplay-domain contracts remain provider-neutral so Supabase can be replaced without rewriting core combat/raise rules.

## Evidence boundary
SELECTED_FOR_Q4 only.
No Supabase project/account, Auth flow, table, RLS policy, Edge Function, cloud-save sync, webhook or production traffic is implemented by this ADR.
