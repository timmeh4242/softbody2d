using UnityEngine;
using System.Collections;

// Helper script to make a camera follow a JellySprite - simply attach to your main camera and
// then drop a JellySprite into the 'Follow Sprite' field
public class JellyCameraFollow : MonoBehaviour 
{
	public JellySprite m_FollowSprite;
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () 
	{
		// New camera position should be the position of the central reference point
		if(m_FollowSprite.ReferencePoints != null)
		{
			Vector3 newPosition = m_FollowSprite.ReferencePoints[0].GameObject.transform.position;
			
			// Keep the z coordinate the same
			newPosition.z = this.transform.position.z;
			
			// Update the camera position
			this.transform.position = newPosition;
		}
	}
}
