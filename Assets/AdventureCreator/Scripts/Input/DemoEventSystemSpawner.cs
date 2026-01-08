/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2025
 *	
 *	"DemoEventSystemSpawner.cs"
 * 
 *	A script that spawns in an Input System-compatible Event System if the user is running the scene with Input System active handling.  This isn't intended for games - just for the demos.
 * 
 */

using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** A script that spawns in an Input System-compatible Event System if the user is running the scene with Input System active handling.  This isn't intended for games - just for the demos. */
	[DefaultExecutionOrder(-1000)]
	public class DemoEventSystemSpawner : MonoBehaviour
	{

#if UNITY_EDITOR

		[SerializeField] private EventSystem eventSystemToSpawn;


		private void Awake()
		{
			var existingEventSystem = FindFirstObjectByType<EventSystem>();
			if (existingEventSystem) return;

			var projectSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
			if (projectSettings != null)
			{
				var inputHandler = projectSettings.FindProperty("activeInputHandler");
				if (inputHandler.intValue == 1)
				{
					Instantiate(eventSystemToSpawn);
				}
			}
		}

#endif

	}

}