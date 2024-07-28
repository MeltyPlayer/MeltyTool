
namespace KSoft.Phoenix.Phx
{
	public enum LocStringCategory
	{
		None,

		Code,
		Techs,
		Squads,
		Powers,
		Abilities,
		Leaders,
		Objects,
		UI,
		Campaign,
		Cinematics,
		Skirmish,

		steamVersion,

		kNumberOf
	};

	public sealed class LocString
		: ObjectModel.BasicViewModel
		, IO.ITagElementStringNameStreamable
	{
		#region ID
		int mID = TypeExtensions.kNone;
		public int ID
		{
			get { return this.mID; }
			private set { this.SetFieldVal(ref this.mID, value); }
		}
		#endregion

		#region Category
		// #NOTE this engine doesn't specifically limit the category values to the stuff in the enum, but, to reduce memory overhead, I am
		LocStringCategory mCategory = LocStringCategory.None;
		public LocStringCategory Category
		{
			get { return this.mCategory; }
			set { this.SetFieldEnum(ref this.mCategory, value); }
		}
		#endregion

		#region Scenario
		string mScenario;
		public string Scenario
		{
			get { return this.mScenario; }
			set { this.SetFieldObj(ref this.mScenario, value); }
		}
		#endregion

		#region IsSubtitle
		bool mIsSubtitle;
		public bool IsSubtitle
		{
			get { return this.mIsSubtitle; }
			set { this.SetFieldVal(ref this.mIsSubtitle, value); }
		}
		#endregion

		#region IsUpdate
		bool mIsUpdate;
		public bool IsUpdate
		{
			get { return this.mIsUpdate; }
			set { this.SetFieldVal(ref this.mIsUpdate, value); }
		}
		#endregion

		#region MouseKeyboardID
		int mMouseKeyboardID = TypeExtensions.kNone;
		public int MouseKeyboardID
		{
			get { return this.mMouseKeyboardID; }
			set { this.SetFieldVal(ref this.mMouseKeyboardID, value); }
		}
		#endregion

		#region OriginalID
		// this is a string because there are cases with "and" in them. eg:
		// "25045 and 23441"
		string mOriginalID;
		public string OriginalID
		{
			get { return this.mOriginalID; }
			set { this.SetFieldObj(ref this.mOriginalID, value); }
		}
		#endregion

		#region Text
		string mText;
		public string Text
		{
			get { return this.mText; }
			set { this.SetFieldObj(ref this.mText, value); }
		}
		#endregion

		public LocString()
		{
		}

		public LocString(int id)
		{
			this.mID = id;
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("_locID", ref this.mID);
			s.StreamAttributeEnumOpt("category", ref this.mCategory, e => e != LocStringCategory.None);
			s.StreamAttributeOpt("scenario", ref this.mScenario, Predicates.IsNotNullOrEmpty);
			s.StreamAttributeOpt("subtitle", ref this.mIsSubtitle, Predicates.IsTrue);
			s.StreamAttributeOpt("Update", ref this.mIsUpdate, Predicates.IsTrue);
			s.StreamAttributeOpt("_mouseKeyboard", ref this.mMouseKeyboardID, Predicates.IsNotNone);
			s.StreamAttributeOpt("originally", ref this.mOriginalID, Predicates.IsNotNullOrEmpty);
			if (s.IsReading || this.mText.IsNotNullOrEmpty())
				s.StreamCursor(ref this.mText);
		}
		#endregion

		public override string ToString()
		{
			return string.Format("({0}) '{1}'",
			                     this.ID,
			                     this.Text ?? "");
		}
	};
}