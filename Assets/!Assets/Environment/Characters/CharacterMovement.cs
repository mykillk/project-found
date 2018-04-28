using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

namespace ProjectFound.Environment.Characters
{


	[RequireComponent( typeof(NavMeshAgent) )]
	public class CharacterMovement : MonoBehaviour
	{
		[SerializeField] float m_movingTurnSpeed = 360;
		[SerializeField] float m_stationaryTurnSpeed = 180;
		[SerializeField] float m_moveSpeedMultiplier = 1f;
		[SerializeField] float m_animSpeedMultiplier = 1f;

		private Animator m_animator;
		private NavMeshAgent m_agent;
		private Rigidbody m_rigidBody;
		private NavMeshPath m_path;

		private float m_turnAmount;
		private float m_forwardAmount;

		public float StoppingDistance
		{
			get { return m_agent.stoppingDistance; }
		}

		void Start( )
		{
			m_animator = GetComponent<Animator>( );
			m_agent = GetComponent<NavMeshAgent>( );
			m_rigidBody = GetComponent<Rigidbody>( );
			m_rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
			m_path = new NavMeshPath( );

			m_agent.updateRotation = false;
			m_agent.updatePosition = true;
			ResetMoveTarget( );

			CombatEncounter.singleton.DelegateEncounterBegin += OnCombatEncounterBegin;
		}

		void LateUpdate( )
		{
			m_animator.speed = m_animSpeedMultiplier;

			if ( m_agent.remainingDistance > m_agent.stoppingDistance )
			{
				AnimatorMove( m_agent.desiredVelocity );
			}
			else
			{
				AnimatorHalt( );
			}
		}

		public float RemainingDistance( )
		{
			return m_agent.remainingDistance;
		}

		public float CheckVelocity( )
		{
			return m_agent.velocity.magnitude;
		}

		public bool CanMoveTo( Vector3 destination )
		{
			m_path.ClearCorners( );

			// 1 == Walkable
			NavMesh.CalculatePath( transform.position, destination, 1, m_path );

			return m_path.status == NavMeshPathStatus.PathComplete;
		}

		public float CalculatePathDistance( )
		{
			float distance = 0.0f;

			for ( int i = 1; i < m_path.corners.Length; ++i )
			{
				distance += Vector3.Distance( m_path.corners[i-1], m_path.corners[i] );
			}

			return distance;
		}

		public void SetMoveTarget( Vector3 destination )
		{
			m_agent.SetDestination( destination );
		}

		public void ResetMoveTarget( )
		{
			m_agent.SetDestination( transform.position );
		}

		public void TranslateMoveTarget( float h, float v )
		{
			if ( h != 0f || v != 0f )
			{
				Transform xform = Camera.main.transform;

				Vector3 camForward = Vector3.Scale( xform.forward, new Vector3( 1, 0, 1 ) ).normalized;
				Vector3 movement = v * camForward + h * xform.right;

				SetMoveTarget( transform.position + movement );
			}
		}

		private void AnimatorMove( Vector3 movement )
		{
			CalculateLocalMovement( ref movement );
			ApplyRootRotation( );
			UpdateAnimator( );
		}

		private void AnimatorHalt( )
		{
			m_turnAmount = 0f;
			m_forwardAmount = 0f;

			UpdateAnimator( );
		}

		private void CalculateLocalMovement( ref Vector3 worldSpaceMovement )
		{
			if ( worldSpaceMovement.magnitude > 1f )
			{
				worldSpaceMovement.Normalize( );
			}

			Vector3 localSpaceMovement = transform.InverseTransformDirection( worldSpaceMovement );
			m_turnAmount = Mathf.Atan2( localSpaceMovement.x, localSpaceMovement.z);
			m_forwardAmount = localSpaceMovement.z;
		}

		private void ApplyRootRotation( )
		{
			float turnSpeed =
				Mathf.Lerp( m_stationaryTurnSpeed, m_movingTurnSpeed, m_forwardAmount );

			transform.Rotate( 0, m_turnAmount * turnSpeed * Time.deltaTime, 0 );
		}

		private void UpdateAnimator( )
		{
			m_animator.SetFloat( "Forward", m_forwardAmount, 0.1f, Time.deltaTime );
			m_animator.SetFloat( "Turn", m_turnAmount, 0.1f, Time.deltaTime );
		}

		private void OnCombatEncounterBegin( List<Combatant> combatants )
		{
			ResetMoveTarget( );
			AnimatorHalt( );
		}

		void OnAnimatorMove( )
		{
			if ( Time.deltaTime > 0 )
			{
				Vector3 velocity =
					(m_animator.deltaPosition * m_moveSpeedMultiplier) / Time.deltaTime;

				velocity.y = m_rigidBody.velocity.y;
				m_rigidBody.velocity = velocity;
			}
		}

		void OnDrawGizmos( )
		{
			if ( m_agent && m_agent.hasPath )
			{
				Gizmos.color = Color.black;
				Gizmos.DrawLine( transform.position, m_agent.destination );
				Gizmos.DrawSphere( m_agent.destination, 0.08f );
			}
		}
	}


}