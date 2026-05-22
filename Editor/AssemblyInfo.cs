// AssemblyInfo.cs
//
// Grants the EditMode test assembly access to `internal` types and members
// in dev.whyknot.core.Editor. Used by WkLoggerTests to exercise the
// `internal static CullOldSessions(...)` helper without exposing it on
// the public surface for production callers.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("dev.whyknot.core.Tests.Editor")]
