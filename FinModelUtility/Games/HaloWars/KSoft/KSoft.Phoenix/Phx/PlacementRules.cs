
namespace KSoft.Phoenix.Phx
{
	public enum BPlacementRuleType
	{
		And,
		Or,

		DistanceAtMostFromType,
		DistanceAtLeastFromType,
		ObstructionAtLeastFromType,
	};
	public enum BPlacementRuleFromTypeKind
	{
		Builder,
		Unit,
	};
	public enum BPlacementRuleLifeType
	{
		Any,
		Alive,
		Dead,
	};
	public enum BPlacementRuleFoundationType
	{
		Any,
		Solid,
		FullyBuilt,
	};

	/*public*/ sealed class BPlacementRule
	{

	#region Xml constants
	#endregion
  };
	/*public*/ sealed class BPlacementRules;
}
