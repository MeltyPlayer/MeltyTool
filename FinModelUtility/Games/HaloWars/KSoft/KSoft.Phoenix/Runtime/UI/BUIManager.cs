
namespace KSoft.Phoenix.Runtime
{
	// Since BWorld serialization hasn't been finished, this code hasn't even been tested

	sealed class BUIManager
		: IO.IEndianStreamSerializable
	{
		public BTimerManager TimerManager = new BTimerManager();
		public BUICallouts UICallouts = new BUICallouts();
		public BUIWidget UIWidgets = new BUIWidget();
		public bool WidgetsVisible, TalkingHeadShown, ObjectiveTrackerShown,
			TimerShown, ObjectiveWidgetsShown, HintsVisible,
			MinimapVisible, UnitStatsVisible, ReticleVisible,
			ResourcePanelVisible, DpadPanelVisible, GameTimeVisible;
		public float MinimapRotationOffset;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(this.TimerManager);
			s.Stream(this.UICallouts);
			s.Stream(this.UIWidgets);
			s.Stream(ref this.WidgetsVisible); s.Stream(ref this.TalkingHeadShown); s.Stream(ref this.ObjectiveTrackerShown);
			s.Stream(ref this.TimerShown); s.Stream(ref this.ObjectiveWidgetsShown); s.Stream(ref this.HintsVisible);
			s.Stream(ref this.MinimapVisible); s.Stream(ref this.UnitStatsVisible); s.Stream(ref this.ReticleVisible);
			s.Stream(ref this.ResourcePanelVisible); s.Stream(ref this.DpadPanelVisible); s.Stream(ref this.GameTimeVisible);
			s.Stream(ref this.MinimapRotationOffset);
		}
		#endregion
	};
}