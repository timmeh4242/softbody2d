using UnityEngine;
using System.Collections;

// Helper script to create a jelly sprite that hovers in mid-air and functions like
// a bridge. This is done by taking a grid-style configuration and then making a row of
// bodies kinematic. Simply create a Jelly Sprite, set it to grid configuration, then
// attach this script to it.
[RequireComponent (typeof(JellySprite))]
public class JellyBridge : MonoBehaviour 
{
	public float fixedRowFraction = 0.5f;	
	bool isFirstUpdate = true;

	// Update is called once per frame
	void Update () 
	{
		// Need to wait until the jelly sprite has initialised	
		if(isFirstUpdate)
		{
			JellySprite jellySprite = GetComponent<JellySprite>();

			if(jellySprite.m_Style == JellySprite.PhysicsStyle.Grid)				
			{
				// Work out the row of rigidbodies to fix - so 0.5 means halfway	
				int fixedRow = (int)(jellySprite.m_GridRows * fixedRowFraction);

				for(int x = 0; x < jellySprite.m_GridColumns; x++)	
				{
					// Work out the point index in the array from the x and y index	
					int pointIndex = (fixedRow * jellySprite.m_GridColumns) + x;

					// Check its not a dummy point
					if(jellySprite.ReferencePoints[pointIndex].GameObject)
					{
						// Set kinematic (might be a 2D or 3D point, so check for both	
						if(jellySprite.ReferencePoints[pointIndex].Body3D)
						{
							jellySprite.ReferencePoints[pointIndex].Body3D.isKinematic = true;	
						}

						if(jellySprite.ReferencePoints[pointIndex].Body2D)	
						{
							jellySprite.ReferencePoints[pointIndex].Body2D.isKinematic = true;	
						}
					}
				}
			}
			else	
			{
				Debug.LogWarning("JellyBridges can only be used on JellySprites using a Grid style configuration");	
			}

			isFirstUpdate = false;
		}
	}
}