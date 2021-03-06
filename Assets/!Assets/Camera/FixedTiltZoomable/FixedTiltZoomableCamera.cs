namespace ProjectFound.CameraUI
{


	using System;

	using UnityEngine;
	using Autelia.Serialization;

	using ProjectFound.Environment.Characters;

	public class FixedTiltZoomableCamera : PivotBasedCameraRig
	{

		// This script is designed to be placed on the root object of a camera rig,
		// comprising 3 gameobjects, each parented to the next:

		// 	Camera Rig
		// 		Pivot
		// 			Camera

		// How fast the rig will move to keep up with the target's position.
		[SerializeField] private float m_MoveSpeed = 3f;

		[Range(0.05f, 2f)]
		[SerializeField] private float _zoomSpeed;

		// How fast the rig will rotate from user input.
		[Range(0.5f, 8f)]
		[SerializeField] private float m_TurnSpeed;

		// How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
		[SerializeField] private float m_TurnSmoothing = 0f;

		// The maximum value of the x axis rotation of the pivot.
		[SerializeField] private float m_TiltMax = 60f;

		// The minimum value of the x axis rotation of the pivot.
		[SerializeField] private float m_TiltMin = 30f;

		// Whether the cursor should be hidden and locked.
		//[SerializeField] private bool m_LockCursor = false;
		// set wether or not the vertical axis should auto return
		//[SerializeField] private bool m_VerticalAutoReturn = false;

		[SerializeField] private float m_defaultTilt = 50f;
		[SerializeField] private float m_minDollyDistance = 4f;
		[SerializeField] private float m_maxDollyDistance = 13f;
		[SerializeField] private Texture2D m_crosshairTexture;

		public bool IsAttached { get; set; }

		// The rig's y axis rotation.
		private float m_LookAngle;
		// The pivot's x axis rotation.
		private float m_TiltAngle;

		private Vector3 m_PivotEulers;
		private Quaternion m_PivotTargetRot;
		private Quaternion m_TransformTargetRot;

		Transform _playerTransform;

		bool m_isAtMinTilt = false;
		bool m_isAtMaxTilt = false;

		[System.NonSerialized] public bool IsRotateModEnabled;

		// TODO Tilt adjusts slightly at every zoom level instead of all at the ends
		// TODO Time delay & aggregation of multiple tilt adjustments, looks smoother

		protected override void Awake()
		{
			IsRotateModEnabled = false;

			if (Serializer.IsLoading) return;
			base.Awake();


			IsAttached = true;

			// Lock or unlock the cursor.
			//Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
			//Cursor.visible = !m_LockCursor;
			m_PivotEulers = _pivotTransform.rotation.eulerAngles;

			m_PivotTargetRot = _pivotTransform.transform.localRotation;
			m_TransformTargetRot = transform.localRotation;

			_playerTransform = FindObjectOfType<Protagonist>( ).transform;
			_cameraTransform = GetComponentInChildren<Camera>( ).transform;
			m_Target = _playerTransform;
			m_TiltAngle = m_defaultTilt;

			transform.position = _playerTransform.position;
		}

		protected new void LateUpdate()
		{
			if (Serializer.IsLoading) return;
			base.LateUpdate( );

			AdjustTilt( );

			/*
			if (m_LockCursor && Input.GetMouseButtonUp(0))
			{
				Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
				Cursor.visible = !m_LockCursor;
			}
			*/
		}

		private void OnDisable()
		{
			//Cursor.lockState = CursorLockMode.None;
			//Cursor.visible = true;
		}

		protected override void FollowTarget( float deltaTime )
		{
			if ( m_Target == null )
				return ;

			float cameraTargetDelta = deltaTime * m_MoveSpeed;

			// Move the rig towards target position.
			transform.position = Vector3.Lerp(
				transform.position, m_Target.transform.position, cameraTargetDelta );
		}

		public void HandleMovement( float forwardMovementAmt, float sideMovementAmt )
		{
			IsAttached = false;
			m_Target = null;

			transform.Translate( Vector3.forward * forwardMovementAmt * .1f );
			transform.Translate( Vector3.right * sideMovementAmt * .1f );
		}

		public void HandleAttachment( )
		{
			IsAttached = true;

			m_Target = _playerTransform;
		}

		public void HandleDetachment( )
		{
			IsAttached = false;

			m_Target = null;
		}

		public void HandleZoom( float zoomAmount )
		{
			//Debug.Log( "Made it to HandleZoom! x = " + mouseWheelDelta.x + " & y = " +
			//mouseWheelDelta.y );

			float closestZValue = -m_minDollyDistance;
			float furthestZValue = -m_maxDollyDistance; 

			zoomAmount *= _zoomSpeed;
			Vector3 translation = new Vector3( 0, 0, zoomAmount );
			Vector3 newPosition = _cameraTransform.localPosition + translation;

			// Negate the dolly distance values because z.forward faces towards the Target
			float clampedHeight = Mathf.Clamp( newPosition.z, furthestZValue ,closestZValue );
			newPosition = new Vector3( newPosition.x, newPosition.y, clampedHeight );

			// Fast way to compare two floating point numbers that accounts for precision issues
			// Compare prior to clamping to only add/subtract the tilt angle once at a time

			if ( Misc.Floater.Equal( newPosition.z, closestZValue ) )
			{
				if ( !m_isAtMinTilt )
				{
					//Debug.Log( "Arrived at the closest zoom level" );
					m_TiltAngle = m_TiltMin;
					m_isAtMinTilt = true;
				}
			}
			else if ( Misc.Floater.Equal( newPosition.z, furthestZValue ) )
			{
				if ( !m_isAtMaxTilt )
				{
					//Debug.Log( "Arrived at the furthest zoom level" );
					m_TiltAngle = m_TiltMax;
					m_isAtMaxTilt = true;
				}
			}
			else
			{
				m_TiltAngle = m_defaultTilt;
				m_isAtMinTilt = m_isAtMaxTilt = false;
			}

			_cameraTransform.localPosition = newPosition;
		}

		public void HandleRotation( float rotation )
		{
			// Adjust the look angle by an amount proportional to the turn speed and horizontal input.
			m_LookAngle += rotation * m_TurnSpeed * 35f * Time.deltaTime;

			// Rotate the rig (the root object) around Y axis only:
			m_TransformTargetRot = Quaternion.Euler( 0f, m_LookAngle, 0f );

			/*
			if (m_VerticalAutoReturn)
			{
				// For tilt input, we need to behave differently depending on whether we're using mouse or touch input:
				// on mobile, vertical input is directly mapped to tilt value, so it springs back automatically when the look input is released
				// we have to test whether above or below zero because we want to auto-return to zero even if min and max are not symmetrical.
				m_TiltAngle = y > 0 ? Mathf.Lerp(0, -m_TiltMin, y) : Mathf.Lerp(0, m_TiltMax, -y);
			}
			else
			{
				// on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
				m_TiltAngle -= y*m_TurnSpeed;
				// and make sure the new value is within the tilt range
				m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);
			}
			*/
			// TODO Figure out what this vertical auto return thingie is
			//m_TiltAngle = m_TiltMax; // Just set it to the default amount for now.

			// Tilt input around X is applied to the pivot (the child of this object)
			//m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, m_PivotEulers.y , m_PivotEulers.z);

			if ( m_TurnSmoothing > 0 )
			{
				transform.localRotation = Quaternion.Slerp(
					transform.localRotation, m_TransformTargetRot, m_TurnSmoothing * Time.deltaTime );
			}
			else
			{
				transform.localRotation = m_TransformTargetRot;
			}
		}

		private void AdjustTilt( )
		{
			m_PivotTargetRot = Quaternion.Euler( m_TiltAngle, m_PivotEulers.y, m_PivotEulers.z );

			if ( m_TurnSmoothing > 0 )
			{
				_pivotTransform.localRotation = Quaternion.Slerp(
					_pivotTransform.localRotation, m_PivotTargetRot, m_TurnSmoothing * Time.deltaTime );
			}
			else
			{
				_pivotTransform.localRotation = m_PivotTargetRot;
			}
		}

		void OnGUI( )
		{
if (Serializer.IsLoading) return;
			/*if ( IsAttached == false )
			{
				int crosshairPixelSize = 32;

				GUI.DrawTexture( new Rect(
					(Screen.width - crosshairPixelSize) / 2 ,
					(Screen.height - crosshairPixelSize) / 2,
					crosshairPixelSize, crosshairPixelSize ),
					m_crosshairTexture );
			}*/
		}
	}


}
