using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class RequiredPermissionsObjectIdSymbolRecommender : PermissionsObjectIdSymbolRecommender
{
	protected override AttributeKind PermissionsAttributeKind => AttributeKind.RequiredPermissions;

	public RequiredPermissionsObjectIdSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}
}
