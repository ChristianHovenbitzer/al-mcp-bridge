// Trigger: CodeCop AA0137 — local variable declared but never used.
codeunit 50100 "Diag Sanity Codeunit"
{
    procedure UnusedLocalDemo()
    var
        UnusedBuffer: Integer;
    begin
        // Body intentionally empty so UnusedBuffer has no read/write.
    end;
}
