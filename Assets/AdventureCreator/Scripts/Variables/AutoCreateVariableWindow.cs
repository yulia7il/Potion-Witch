#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AC
{

	public class AutoCreateVariableWindow : EditorWindow
	{
	
		private string variableName;
		private VariableLocation location;
		private Variables variables;
		private VariableType variableType;
		private Action actionFor;


		public static void Init (string _variableName, VariableLocation _location, Variables _variables, VariableType _variableType, Action _actionFor)
		{
			AutoCreateVariableWindow window = (AutoCreateVariableWindow) EditorWindow.GetWindow (typeof (AutoCreateVariableWindow));
			window.titleContent.text = "Auto-create Variable";
			window.position = new Rect (300, 200, 320, 100);

			window.location = _location;
			window.variables = _variables;
			window.variableName = _variableName;
			window.variableType = _variableType;
			window.actionFor = _actionFor;
		}
		
		
		private void OnGUI ()
		{
			variableName = EditorGUILayout.TextField ("New Variable name:", variableName);

			if (GUILayout.Button ("Create variable"))
			{
				OnCreateVariable ();
				GUIUtility.ExitGUI ();
			}
		}
		
	

		private void OnCreateVariable ()
		{
			CreateVariable(variableName, variableType, location, variables);
			Close ();
		}
		
		
		public static GVar CreateVariable(string variableName, VariableType variableType, VariableLocation location, Variables variables)
		{
			variableName = variableName.Trim ();
			
			if (string.IsNullOrEmpty (variableName))
			{
				EditorUtility.DisplayDialog ("Unable to create Variable", "Please specify a valid Variable name.", "Close");
				return null;
			}
			
			GVar newVariable = null;
			if (location == VariableLocation.Global && KickStarter.variablesManager != null)
			{
				Undo.RecordObject (KickStarter.variablesManager, "Create variable");
				newVariable = CreateNewVariable (KickStarter.variablesManager.vars, variableName, variableType, location);

				UnityVersionHandler.CustomSetDirty (KickStarter.variablesManager);
			}
			else if (location == VariableLocation.Local && KickStarter.localVariables != null)
			{
				Undo.RecordObject (KickStarter.localVariables, "Create variable");
				newVariable = CreateNewVariable (KickStarter.localVariables.localVars, variableName, variableType, location);

				UnityVersionHandler.CustomSetDirty (KickStarter.localVariables);
			}
			else if (location == VariableLocation.Component && variables)
			{
				Undo.RecordObject (variables, "Create variable");
				newVariable = CreateNewVariable (variables.vars, variableName, variableType, location);

				UnityVersionHandler.CustomSetDirty (variables);
			}
			
			return newVariable;
		}


		private static GVar CreateNewVariable (List<GVar> _vars, string variableName, VariableType variableType, VariableLocation location)
		{
			if (_vars == null) return null;

			GVar newVariable = new GVar (GetIDArray (_vars));
			newVariable.label = variableName;
			newVariable.type = variableType;

			_vars.Add (newVariable);
			ACDebug.Log ("Created new " + location.ToString () + " variable '" + variableName + "'");

			return newVariable;
		}


		private static int[] GetIDArray (List<GVar> _vars)
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (GVar variable in _vars)
			{
				idArray.Add (variable.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}

	}

}

#endif