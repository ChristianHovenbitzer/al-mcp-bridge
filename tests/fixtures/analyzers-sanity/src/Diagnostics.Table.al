// Trigger: LinterCop LC0001 — FlowFields should not be editable.
table 50100 "Diag Sanity Table"
{
    Caption = 'Diag Sanity Table';
    DataClassification = CustomerContent;

    fields
    {
        field(1; "No."; Code[20])
        {
            Caption = 'No.';
            DataClassification = CustomerContent;
        }
        field(2; "Row Count"; Integer)
        {
            Caption = 'Row Count';
            FieldClass = FlowField;
            CalcFormula = count("Diag Sanity Table");
            Editable = true;
        }
    }

    keys
    {
        key(PK; "No.") { Clustered = true; }
    }
}
