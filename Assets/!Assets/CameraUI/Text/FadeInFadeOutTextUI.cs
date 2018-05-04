using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFound.CameraUI
{


	public class FadeInFadeOutTextUI : TextUI
	{
		private bool m_isFadingOut = false;
		private float m_fadeOutAlpha = 0f;

		private bool m_isFadingIn = false;
		private float m_fadeInAlpha = 1f;

		new void Start( )
		{
			base.Start( );

			m_tmPro.canvasRenderer.SetAlpha( 0f );
		}

		new void LateUpdate( )
		{
			base.LateUpdate( );

			if ( m_isFadingOut == true )
			{
				float alpha = m_tmPro.canvasRenderer.GetAlpha( );

				if ( Misc.Floater.Equal( alpha, m_fadeOutAlpha ) )
				{
					m_isFadingOut = false;
					HideText( );
				}
			}
			if ( m_isFadingIn == true )
			{
				float alpha = m_tmPro.canvasRenderer.GetAlpha( );

				if ( Misc.Floater.Equal( alpha, m_fadeInAlpha ) )
				{
					m_isFadingIn = false;
				}
			}
		}

		protected IEnumerator<float> FadeInThenOut( float inStagnation, float inSeconds, float finalInAlpha,
			float outStagnation, float outSeconds, float finalOutAlpha )
		{
			yield return
				MEC.Timing.WaitUntilDone( FadeIn( inStagnation, inSeconds, finalInAlpha ) );

			yield return MEC.Timing.WaitForSeconds( inSeconds );

			yield return
				MEC.Timing.WaitUntilDone( FadeOut( outStagnation, outSeconds, finalOutAlpha ) );
		}

		protected IEnumerator<float> FadeIn( float stagnation, float seconds, float finalAlpha = 1f )
		{
			DisplayText( );

			m_fadeInAlpha = finalAlpha;
			m_isFadingIn = true;

			if ( stagnation != 0f )
			{
				yield return MEC.Timing.WaitForSeconds( stagnation );
			}

			m_tmPro.CrossFadeAlpha( finalAlpha, seconds, false );

			yield break;
		}

		protected IEnumerator<float> FadeOut( float stagnation, float seconds, float finalAlpha = 0f )
		{
			m_fadeOutAlpha = finalAlpha;
			m_isFadingOut = true;

			if ( stagnation != 0f )
			{
				yield return MEC.Timing.WaitForSeconds( stagnation );
			}

			m_tmPro.CrossFadeAlpha( finalAlpha, seconds, false );

			yield break;
		}
	}


}
