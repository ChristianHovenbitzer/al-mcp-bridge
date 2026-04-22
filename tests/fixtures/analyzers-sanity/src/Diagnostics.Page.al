// Surfaces UICop (AW) and potentially AppSourceCop (AS) and PerTenantExtensionCop
// (PTE) findings. Fields intentionally lack ToolTip / ApplicationArea so at
// least one AW rule fires; page-level Caption is missing as well.
page 50100 "Diag Sanity Page"
{
    PageType = Card;
    SourceTable = "Diag Sanity Table";

    layout
    {
        area(Content)
        {
            group(General)
            {
                field("No."; Rec."No.")
                {
                }
                field("Row Count"; Rec."Row Count")
                {
                }
            }
        }
    }

    actions
    {
        area(Processing)
        {
            action(DoThing)
            {
                trigger OnAction()
                begin
                end;
            }
        }
    }
}
