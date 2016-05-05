using UnityEngine;
using System.Collections;

public class BlobBehaviour : MonoBehaviour 
{
	public float m_MinBounceTime = 0.3f;
	public float m_MaxBounceTime = 1.0f;
	public float m_MinJumpForce = 10.0f;
	public float m_MaxJumpForce = 10.0f;
	public Vector2 m_MinJumpVector = new Vector2(-0.1f, 1.0f);
	public Vector2 m_MaxJumpVector = new Vector2(0.1f, 1.0f);
	public LayerMask m_GroundLayer;

	JellySprite m_JellySprite;
	float m_BounceTimer;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () 
	{
		m_JellySprite = GetComponent<JellySprite>();
		m_BounceTimer = UnityEngine.Random.Range(m_MinBounceTime, m_MaxBounceTime);
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () 
	{
		m_BounceTimer -= Time.deltaTime;

		// Randomly bounce around
		if(m_BounceTimer < 0.0f && m_JellySprite.IsGrounded(m_GroundLayer, 2))
		{
			Vector2 jumpVector = Vector2.zero;
			jumpVector.x = UnityEngine.Random.Range(m_MinJumpVector.x, m_MaxJumpVector.x);
			jumpVector.y = UnityEngine.Random.Range(m_MinJumpVector.y, m_MaxJumpVector.y);
			jumpVector.Normalize();
			m_JellySprite.AddForce(jumpVector * UnityEngine.Random.Range(m_MinJumpForce, m_MaxJumpForce));
			m_BounceTimer = UnityEngine.Random.Range(m_MinBounceTime, m_MaxBounceTime);
		}
	}
}