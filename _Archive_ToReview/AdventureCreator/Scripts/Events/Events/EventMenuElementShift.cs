using UnityEngine;

namespace AC
{

	[System.Serializable]
	public class EventMenuElementShift : EventBase
	{

		[SerializeField] private string menuName;
		[SerializeField] private string elementName;


		public override string[] EditorNames { get { return new string[] { "Menu/Shift element" }; } }
		protected override string EventName { get { return "OnMenuElementShift"; } }
		protected override string ConditionHelp { get { return "Whenever " + (!string.IsNullOrEmpty (menuName) ? "menu " + menuName + "'s " : "a menu's ") + (!string.IsNullOrEmpty (menuName) && !string.IsNullOrEmpty (elementName) ? "'" + elementName + "' element is" : "elements are") + " shifted."; } }


		public EventMenuElementShift (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, string _menuName, string _elementName)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			menuName = _menuName;
			elementName = _elementName;
		}


		public EventMenuElementShift () {}


		public override void Register ()
		{
			EventManager.OnMenuElementShift += OnMenuElementShift;
		}


		public override void Unregister ()
		{
			EventManager.OnMenuElementShift -= OnMenuElementShift;
		}


		private void OnMenuElementShift (MenuElement element, AC_ShiftInventory shiftType)
		{
			var menu = element.ParentMenu;

			if (menu.title != menuName) return;
			if (!string.IsNullOrEmpty(elementName) && (element == null || element.title != elementName)) return;

			Run (new object[] { menu.title, element.title, shiftType == AC_ShiftInventory.ShiftNext });
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.String, "Menu name"),
				new ParameterReference (ParameterType.String, "Element name"),
				new ParameterReference (ParameterType.Boolean, "Shifting next"),
			};
		}


#if UNITY_EDITOR

		protected override bool HasConditions (bool isAssetFile) { return true; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			menuName = CustomGUILayout.TextField ("Menu name:", menuName);
			if (!string.IsNullOrEmpty (menuName))
				elementName = CustomGUILayout.TextField ("Element name:", elementName);
		}

#endif

	}

}