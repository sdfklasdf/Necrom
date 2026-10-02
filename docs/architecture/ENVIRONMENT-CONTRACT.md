# Environment & Configuration Contract

Status: ACTIVE PRINCIPLE / IMPLEMENT WHEN Q4-Q5 BEGIN

Logical environments:
- local-dev
- test
- staging
- production

Rules:
- no privileged secret in Unity source, Git repository or Figma.
- test/sandbox configuration must never silently hit production.
- production must never silently fall back to test/local.
- unknown or incomplete privileged configuration fails closed.
- development/synthetic telemetry must be separable from production metrics.

Q4 Supabase:
- separate non-production and production resources/config before public launch.
- service-role/private credentials stay trusted-side.

Q5 RevenueCat / Apple / Google:
- sandbox products and production products must be explicitly separated.

Q5 AdMob:
- development/test uses test-safe ads.
- production ad units require explicit release configuration.

Q5 Sentry:
- environment and release/build identity must distinguish test from production.

Q5 PostHog:
- test/synthetic events must not pollute production analytics.

Evidence boundary:
This is a configuration principle, not proof that any vendor environment/account currently exists.
