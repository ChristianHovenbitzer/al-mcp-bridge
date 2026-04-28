using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class InherentPermissionsObjectIdSymbolRecommender : PermissionsObjectIdSymbolRecommender
{
	protected override AttributeKind PermissionsAttributeKind => AttributeKind.InherentPermissions;

	public InherentPermissionsObjectIdSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}
}
