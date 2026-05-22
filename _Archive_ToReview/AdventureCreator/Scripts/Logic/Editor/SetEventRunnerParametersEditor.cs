#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (SetEventRunnerParameters))]
	public class SetEventRunnerParametersEditor : Editor
	{

		private SetEventRunnerParameters _target;


		public override void OnInspectorGUI ()
		{
			_target = (SetEventRunnerParameters) target;

			_target.ShowGUI ();

			UnityVersionHandler.CustomSetDirty (_target);
		}
		
	}

}

#endif