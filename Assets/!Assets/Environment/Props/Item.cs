using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
//using TMPro;

namespace ProjectFound.Environment.Items {


	public class Item : Prop
	{
		[SerializeField] Sprite m_icon;

		public Sprite Icon { get { return m_icon; } }
		public GameObject Prompt { get; set; }

		void Awake( )
		{
			Assert.IsNotNull( m_icon );
		}

		new void Start( )
		{
			base.Start( );

			//GenerateIcon( );
		}

		public override bool ValidateAction( ActionType actionType )
		{
			switch ( actionType )
			{
				case ActionType.PickUp:
					m_currentActionType = actionType;
					return true;
				default:
					return base.ValidateAction( actionType );
			}
		}

		public override void Reaction( )
		{
			switch ( m_currentActionType )
			{
				case ActionType.PickUp:
					this.gameObject.SetActive( false );
					break;
				case ActionType.Use:
					this.gameObject.SetActive( true );
					break;
				default:
					base.Reaction( );
					break;
			}
		}
		/*
		public void GenerateIcon( )
		{
			int counter = 0;

			while ( m_icon == null && counter < 75 )
			{
				m_icon = AssetPreview.GetAssetPreview( m_iconPrefab );
				counter++;
				System.Threading.Thread.Sleep( 15 );
			}

			//m_icon = AssetPreview.GetMiniThumbnail( m_iconPrefab );

			//m_icon = AssetPreview.GetAssetPreview( gameObject );
		}*/
	}


}