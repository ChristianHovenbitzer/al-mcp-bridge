# TODO

## LinterCop AD0001 — missing `Microsoft.Dynamics.Nav.Analyzers.Common`

**Symptom.** After wiring `backgroundCodeAnalysis` + `ruleSetPath` on
`al/setActiveWorkspace`, `publishDiagnostics` now fires for every `.al`
file in the project and AL compiler diagnostics (e.g. `AL0792`) surface
correctly. But no LinterCop rule (`LC0001`, `LC0015`, `LC0033`, `LC0052`,
`LC0054`, …) ever appears — `al_get_diagnostics` on a file that clearly
violates them returns nothing LinterCop-shaped.

The cause shows up as four `AD0001` warnings on `app.json`:

```
Analyzer 'BusinessCentral.LinterCop.Design.Rule0054FollowInterfaceObjectNameGuide'
threw an exception of type 'System.IO.FileNotFoundException' with message
'Could not load file or assembly
 "Microsoft.Dynamics.Nav.Analyzers.Common, Version=17.0.34.45391,
  Culture=neutral, PublicKeyToken=31bf3856ad364e35"'
```

Same stack for `Rule0052`, `Rule0033`, `Rule0015`. LinterCop's
`CodeFixProvider` still registers (which is why `al_list_code_actions`
surfaces `Fix0001…` quickfixes), but the `DiagnosticAnalyzer` side fails
at first use because its helpers reach into
`Microsoft.Dynamics.Nav.Analyzers.Common.dll` and the CLR can't find that
assembly at probe time.

**Why this is a bridge problem, not a user-config problem.** The DLL
*does* exist next to LinterCop in the AL extension's `Analyzers/` folder:

```
%USERPROFILE%/.vscode/extensions/ms-dynamics-smb.al-<ver>/bin/Analyzers/
  BusinessCentral.LinterCop.dll
  Microsoft.Dynamics.Nav.Analyzers.Common.dll   ← present, not loaded
  ALCops.*.dll
  ...
```

When VS Code itself hosts the AL LS, resolution succeeds because the
hosting process has the Analyzers folder on its probing path / AssemblyLoadContext.
The bridge invokes the LS the same way, but users typically point
`AL_CODE_ANALYZERS` at a single DLL (`…/Analyzers/BusinessCentral.LinterCop.dll`)
and the LS's analyzer load context apparently doesn't pick up sibling
DLLs in the same folder automatically — or the version pinning
(`Version=17.0.34.45391`) doesn't match the shipped one and needs an
explicit bind redirect.

**What to investigate.**

1. Confirm the shipped `Microsoft.Dynamics.Nav.Analyzers.Common.dll`
   version against the one LinterCop pins to (`17.0.34.45391`). If they
   differ, the analyzer was built against a different AL LS build.
2. Check whether the AL LS accepts a directory in `codeAnalyzers` (load
   everything in the folder) vs. only individual DLL paths. If directory
   loading works, recommending that in README would sidestep the probe
   failure.
3. Look at how the VS Code extension itself passes `codeAnalyzers` —
   does it list every DLL in `Analyzers/` explicitly, or does the LS
   enumerate the folder?
4. Sanity-check: run with `assemblyProbingPaths` set to the Analyzers
   folder and see whether resolution then succeeds (currently we send
   `assemblyProbingPaths: []`).

**Proposed fix directions, cheapest first.**

- **Auto-populate `assemblyProbingPaths`** with the directory containing
  each analyzer DLL when the caller sets `AL_CODE_ANALYZERS` to a file
  path. Zero user-facing change, likely resolves the probe failure
  because `Microsoft.Dynamics.Nav.Analyzers.Common.dll` lives in the
  same folder.
- **Accept a directory** in `AL_CODE_ANALYZERS` and, if given, expand to
  every `*.dll` under it (mirroring the `${analyzerFolder}…` pattern
  from `.vscode/settings.json`). Update README.
- **Document the failure mode** in the troubleshooting table: "if you
  see `AD0001 … Could not load file or assembly
  Microsoft.Dynamics.Nav.Analyzers.Common`, your analyzer DLL load
  context is missing its sibling assemblies — set
  `AL_CODE_ANALYZERS` to the full folder, not a single DLL."

**Acceptance check.** On the repro project (General Customizations), a
call to `al_get_diagnostics` on
`src/Dispatch/Carrier/Log/DispatchLogEntry.Table.al` should return one
`LC0001` warning on the `User Name` FlowField at line 83 ("FlowFields
should not be editable"), in addition to the compiler-level `AL0792`
entries already present on other files. No `AD0001` should remain on
`app.json`.
