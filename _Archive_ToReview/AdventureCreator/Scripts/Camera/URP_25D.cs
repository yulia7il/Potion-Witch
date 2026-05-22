/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2025
 *	
 *	"URP_25D.cs"
 * 
 *	A helper script that adjusts camera behaviour when creating a 2.5D game.
 * 
 */
 
#if URPIsPresent

using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace AC
{

	// A helper script that adjusts camera behaviour when creating a 2.5D game.
	[ExecuteInEditMode]
	public class URP_25D : MonoBehaviour
	{

		#region Variables

		private void OnEnable() { EventManager.OnSwitchCamera += OnSwitchCamera; }
		private void OnDisable() { EventManager.OnSwitchCamera -= OnSwitchCamera; }

		#endregion


		#region UnityStandards

		private void Awake()
		{
			Camera backgroundCamera = BackgroundCamera.Instance.GetComponent<Camera>();
			var backgroundCameraData = backgroundCamera.GetUniversalAdditionalCameraData();
			backgroundCameraData.renderType = CameraRenderType.Overlay;

			var mainCameraData = KickStarter.mainCamera.Camera.GetUniversalAdditionalCameraData();
			if (!mainCameraData.cameraStack.Contains(backgroundCamera))
			{
				mainCameraData.cameraStack.Add(backgroundCamera);
			}
		}


		private void Start()
		{
			GameCamera25D[] gameCamera25Ds = UnityVersionHandler.FindObjectsOfType<GameCamera25D>();
			foreach (GameCamera25D gameCamera25D in gameCamera25Ds)
			{
				gameCamera25D.Camera.enabled = true;

				var cameraData = gameCamera25D.Camera.GetUniversalAdditionalCameraData();
				cameraData.renderType = CameraRenderType.Overlay;
			}
		}


		private void Update()
		{
			if (Application.isPlaying) return;
			ForceSetStack();
		}

		#endregion


		#region CustomEvents

		private void OnSwitchCamera(_Camera fromCamera, _Camera toCamera, float transitionTime)
		{
			var mainCameraData = KickStarter.mainCamera.Camera.GetUniversalAdditionalCameraData();
			if (fromCamera && mainCameraData.cameraStack.Contains(fromCamera.Camera))
			{
				mainCameraData.cameraStack.Remove(fromCamera.Camera);
			}

			if (!mainCameraData.cameraStack.Contains(toCamera.Camera))
			{
				var gameCameraData = toCamera.Camera.GetUniversalAdditionalCameraData();
				gameCameraData.renderType = CameraRenderType.Overlay;
				mainCameraData.cameraStack.Add(toCamera.Camera);
			}
		}

		#endregion


		#region PrivateFunctions

		private void ForceSetStack()
		{
			GameCamera25D[] gameCamera25Ds = UnityVersionHandler.FindObjectsOfType<GameCamera25D>();
			foreach (GameCamera25D gameCamera25D in gameCamera25Ds)
			{
				if (gameCamera25D.isActiveEditor)
				{
					var mainCameraData = KickStarter.mainCamera.Camera.GetUniversalAdditionalCameraData();
					foreach (GameCamera25D otherCamera in gameCamera25Ds)
					{
						if (otherCamera == gameCamera25D)
						{
							if (!mainCameraData.cameraStack.Contains(gameCamera25D.Camera))
							{
								var gameCameraData = gameCamera25D.Camera.GetUniversalAdditionalCameraData();
								gameCameraData.renderType = CameraRenderType.Overlay;
								mainCameraData.cameraStack.Add(gameCamera25D.Camera);
							}
							continue;
						}
						if (mainCameraData.cameraStack.Contains(otherCamera.Camera))
						{
							mainCameraData.cameraStack.Remove(otherCamera.Camera);
						}
					}
				}
			}
		}

		#endregion

	}

}

#endif