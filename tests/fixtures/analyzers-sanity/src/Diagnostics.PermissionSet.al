// Fixture: permission set with an object name and a caption that both
// deliberately exceed the 20-character limit. Exists so the test suite can
// assert that the bridge surfaces BOTH warnings (one per property). A
// regression that silences either warning should fail the test.
permissionset 50120 "Diag Sanity PermSet Name Way Too Long"
{
    Assignable = true;
    Caption = 'Diag Sanity PermSet Caption Exceeds Twenty Characters';
    Permissions = ;
}
