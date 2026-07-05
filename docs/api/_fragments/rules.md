**Domain notes:**
- Rules Engine (Enterprise). `RuleDefinition.Conditions`/`Actions`/`Triggers` are polymorphic
  (`object?`) — shape per the Akeneo rule format; the client does not validate them.
- **Caution:** an `Enabled` rule executes against real catalog data. When experimenting, set
  `Enabled = false` explicitly.
- `CreateOrReplaceRuleDefinitionAsync` uses PUT: it **replaces** the whole rule, unlike the
  PATCH-merge semantics everywhere else.