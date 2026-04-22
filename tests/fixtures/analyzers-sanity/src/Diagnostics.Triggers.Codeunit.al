// Engineered to trip ALCops.LinterCop rules (CognitiveComplexity,
// BuiltInDateTimeMethod, …). Deliberately ugly — not shippable code.
codeunit 50101 "Diag Trigger Codeunit"
{
    procedure ComplexDecision(a: Integer; b: Integer; c: Integer): Integer
    var
        Result: Integer;
        i: Integer;
    begin
        // Nested conditionals + loop to inflate cognitive complexity.
        if a > 0 then begin
            if b > 0 then begin
                if c > 0 then begin
                    for i := 1 to a do begin
                        if i mod 2 = 0 then
                            Result += b
                        else
                            Result += c;
                        if Result > 100 then
                            Result -= 1;
                    end;
                end else begin
                    Result := -1;
                end;
            end else if a > b then begin
                Result := a - b;
            end else begin
                Result := 0;
            end;
        end else begin
            Result := -a;
        end;
        exit(Result);
    end;

    procedure Now(): DateTime
    begin
        // `CurrentDateTime` is the built-in LinterCop wants to call out.
        exit(CurrentDateTime());
    end;
}
