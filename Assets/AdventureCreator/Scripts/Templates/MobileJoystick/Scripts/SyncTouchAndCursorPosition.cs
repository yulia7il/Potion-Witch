using UnityEngine;

namespace AC.Templates.MobileJoystick
{

	public class SyncTouchAndCursorPosition : MonoBehaviour
	{

		#region Variables

		private int cursorFingerID = -1;
		private Vector2 touchCursorPosition;

		#endregion


		#region UnityStandards

		private void Start()
		{
#if InputSystemIsPresent
			UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable ();
#endif
		}


		private void OnEnable()
		{
			PlayerInput.InputMousePositionDelegates.Add (InputMousePosition);
		}


		private void OnDisable()
		{
			PlayerInput.InputMousePositionDelegates.Remove (InputMousePosition);
		}


		private void Update()
		{
			UpdateCursor();
		}

		#endregion


		#region PrivateFunctions

		private void UpdateCursor()
		{
#if InputSystemIsPresent
			if (cursorFingerID < 0)
			{
				foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
				{
					if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
					{
						touchCursorPosition = touch.screenPosition;
						cursorFingerID = touch.touchId;
						return;
					}
				}
				return;
			}

			foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
			{
				if (touch.touchId == cursorFingerID && touch.phase != UnityEngine.InputSystem.TouchPhase.Ended)
				{
					return;
				}
			}
#else
			if (cursorFingerID < 0)
			{
				for (int i = 0; i < Input.touchCount; i++)
				{
					Touch touch = Input.GetTouch(i);
					if (touch.phase == TouchPhase.Began)
					{
						touchCursorPosition = touch.position;
						cursorFingerID = touch.fingerId;
						return;
					}
				}
				return;
			}

			for (int i = 0; i < Input.touchCount; i++)
			{
				if (Input.GetTouch(i).fingerId == cursorFingerID && Input.GetTouch(i).phase != TouchPhase.Ended)
				{
					return;
				}
			}
#endif
			cursorFingerID = -1;
		}


		private Vector2 InputMousePosition(bool isLocked)
		{
			if (isLocked)
			{
				return KickStarter.playerInput.LockedCursorPosition;
			}

#if InputSystemIsPresent
			if (cursorFingerID >= 0)
			{
				foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
				{
					if (touch.touchId == cursorFingerID)
					{
						touchCursorPosition = touch.screenPosition;
					}
				}
				return touchCursorPosition;
			}
			return UnityEngine.InputSystem.Mouse.current.position.ReadValue ();
#else
			if (cursorFingerID >= 0)
			{
				for (int i = 0; i < Input.touchCount; i++)
				{
					if (Input.GetTouch(i).fingerId == cursorFingerID)
					{
						touchCursorPosition = Input.GetTouch(i).position;
					}
				}
				return touchCursorPosition;
			}
			return Input.mousePosition;
#endif
		}

#endregion

	}

}