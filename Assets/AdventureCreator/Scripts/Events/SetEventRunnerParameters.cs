/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2025
 *	
 *	"SetEventRunnerParameters.cs"
 * 
 *	A component used to set all of an Event Runnder event's parameters when run.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** A component used to set all of an Event Runnder event's parameters when run. */
	[AddComponentMenu("Adventure Creator/Hotspots/Set Event Runner parameters")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_set_event_runner_parameters.html")]
	public class SetEventRunnerParameters : SetParametersBase
	{

		#region Variables

		[SerializeField] protected EventRunner eventRunner = null;
		[SerializeField] protected int eventID;
		
		#endregion


		#region UnityStandards

		protected void OnEnable ()
		{
			EventManager.OnEventFire += OnEventFire;
		}


		protected void OnDisable ()
		{
			EventManager.OnEventFire -= OnEventFire;
		}

		#endregion


		#region CustomEvents

		protected void OnEventFire(EventBase eventBase)
		{
			if (eventRunner && eventRunner.GetEvent(eventID) == eventBase)
			{
				if (eventRunner.source == ActionListSource.AssetFile)
				{
					AssignParameterValues (eventBase.ActionListAsset);
				}
				else if (eventRunner.source == ActionListSource.InScene)
				{
					AssignParameterValues (eventBase.Cutscene);
				}
			}
		}

		#endregion


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			if (eventRunner == null)
			{
				eventRunner = GetComponent <EventRunner>();
			}

			eventRunner = (EventRunner) EditorGUILayout.ObjectField ("Event Runner:", eventRunner, typeof (EventRunner), true);
			if (eventRunner)
			{
				if (eventRunner.Events == null || eventRunner.Events.Count == 0)
				{
					EditorGUILayout.HelpBox("No Events defined", MessageType.Info);
					return;
				}

				List<string> labelList = new List<string>();
				int selectedIndex = 0;
							
				for (int i = 0; i < eventRunner.Events.Count; i++)
				{
					labelList.Add (eventRunner.Events[i].ID + ": " + eventRunner.Events[i].Label);
					if (eventRunner.Events[i].ID == eventID)
					{
						selectedIndex = i;
					}
				}
				selectedIndex = EditorGUILayout.Popup ("Event:", selectedIndex, labelList.ToArray ());
				eventID = eventRunner.Events[selectedIndex].ID;

				ShowParametersGUI (eventRunner.Events[selectedIndex]);
			}
		}


		protected void ShowParametersGUI (EventBase _event)
		{
			if (eventRunner.source == ActionListSource.InScene && _event.Cutscene)
			{
				ShowParametersGUI (_event.Cutscene);
			}
			else if (eventRunner.source == ActionListSource.AssetFile && _event.ActionListAsset)
			{
				ShowParametersGUI (_event.ActionListAsset);
			}
			else
			{
				EditorGUILayout.HelpBox ("Cannot set parameters for Event '" + _event.Label + "', since it has no associated ActionList.", MessageType.Warning);
			}
		}


		protected void ShowParametersGUI (Cutscene cutscene)
		{
			if (cutscene == null) return;

			if (cutscene.source == ActionListSource.AssetFile && cutscene.assetFile && cutscene.assetFile.NumParameters > 0)
			{
				ShowActionListReference (cutscene.assetFile);
				ShowParametersGUI (cutscene.assetFile.DefaultParameters, cutscene.syncParamValues);
			}
			else if (cutscene.source == ActionListSource.InScene && cutscene.NumParameters > 0)
			{
				ShowActionListReference (cutscene);
				ShowParametersGUI (cutscene.parameters, false);
			}
			else
			{
				EditorGUILayout.HelpBox ("No parameters defined for Cutscene '" + cutscene.gameObject.name + "'.", MessageType.Warning);
			}
		}


		protected void ShowParametersGUI (ActionListAsset actionListAsset)
		{
			if (actionListAsset == null) return;

			if (actionListAsset.NumParameters > 0)
			{
				ShowActionListReference (actionListAsset);
				ShowParametersGUI (actionListAsset.DefaultParameters, true);
			}
			else
			{
				EditorGUILayout.HelpBox ("No parameters defined for ActionList Assset '" + actionListAsset.name + "'.", MessageType.Warning);
			}
		}


		protected void ShowActionListReference (Cutscene cutscene)
		{
			if (cutscene)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Cutscene: " + cutscene);
				if (GUILayout.Button ("Ping", GUILayout.Width (50f)))
				{
					EditorGUIUtility.PingObject (cutscene);
				}
				EditorGUILayout.EndHorizontal ();
			}
		}


		protected void ShowActionListReference (ActionListAsset actionListAsset)
		{
			if (actionListAsset)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Asset file: " + actionListAsset);
				if (GUILayout.Button ("Ping", GUILayout.Width (50f)))
				{
					EditorGUIUtility.PingObject (actionListAsset);
				}
				EditorGUILayout.EndHorizontal ();
			}
		}

		#endif

	}

}