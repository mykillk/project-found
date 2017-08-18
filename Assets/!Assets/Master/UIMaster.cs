using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMaster
{
	public InventoryUI InventoryUI { get; set; }
	//public ActionBarUI ActionBarUI { get; set; }
	//public PauseMenuUI PauseMenuUI { get; set; }

	public UIMaster( )
	{
		InventoryUI = GameObject.FindObjectOfType<InventoryUI>( );
	}

	public void Loop( )
	{

	}

	public UnityEngine.UI.Button AddInventoryButton( Item item )
	{
		return InventoryUI.AddItem( item );
	}

	public void RemoveInventoryButton( Item item )
	{
		InventoryUI.RemoveItem( item );
	}

}