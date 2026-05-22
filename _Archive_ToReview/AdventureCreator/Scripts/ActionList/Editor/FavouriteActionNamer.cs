#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AC
{

	public class FavouriteActionNamer : EditorWindow
	{

		private string label;
		private AC.Action action;	
		private int ID;


		public static void Show(AC.Action action, int ID, string label = "")
		{
			FavouriteActionNamer window = CreateInstance<FavouriteActionNamer>();
			window.action = action;
			window.ID = ID;
			window.label = string.IsNullOrEmpty(label) ? (action.Category.ToString() + ": " + action.Title) : label;
			window.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 80);
			window.minSize = new Vector2 (400, 80);
			window.maxSize = new Vector2 (400, 80);
			window.ShowUtility();
		}


		private void OnGUI()
		{
			label = EditorGUILayout.TextField("Favourite label:", label);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Create"))
			{
				FavouriteActionData existingFavouriteActionData = KickStarter.actionsManager.GetFavouriteActionData (ID);
				if (existingFavouriteActionData != null)
				{
					existingFavouriteActionData.Update (action, label);
				}
				else
				{
					FavouriteActionData newFavouriteActionData = new FavouriteActionData (action, ID, label);
					KickStarter.actionsManager.allFavouriteActionData.Add (newFavouriteActionData);
				}
				EditorUtility.SetDirty (KickStarter.actionsManager);
				Close();
			}

			if (GUILayout.Button("Cancel"))
			{
				Close();
			}
			EditorGUILayout.EndHorizontal();
		}

	}

}

#endif