using UnityEngine;
using System.Collections;

public class BlobEyes : MonoBehaviour 
{
	SpriteRenderer m_SpriteRenderer;
	public Sprite m_ClosedSprite;
	public Sprite m_OpenSprite;
	public float m_MinBlinkInterval = 2.0f;
	public float m_MaxBlinkInterval = 3.0f;
	public float m_MinBlinkTime = 0.1f;
	public float m_MaxBlinkTime = 0.15f;
	public GameObject m_PupilLeft;
	public GameObject m_PupilRight;
	public float m_EyeRadius = 1.0f;

	float m_BlinkTimer;
	bool m_Blinking;
	Vector3 m_LookDirection;
	Vector3 m_PupilLeftCentre;
	Vector3 m_PupilRightCentre;
	Transform m_LookTarget;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () 
	{
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
		m_BlinkTimer = UnityEngine.Random.Range(m_MinBlinkInterval, m_MaxBlinkInterval);
		m_PupilLeftCentre = m_PupilLeft.transform.localPosition;
		m_PupilRightCentre = m_PupilRight.transform.localPosition;
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () 
	{	
		m_BlinkTimer -= Time.deltaTime;

		if(m_BlinkTimer < 0.0f)
		{
			m_Blinking = !m_Blinking;

			if(m_Blinking)
			{
				GameObject[] blobs = GameObject.FindGameObjectsWithTag("Blob");
				int randomBlobIndex = UnityEngine.Random.Range(0, blobs.Length);

				if(blobs[randomBlobIndex] != this.gameObject)
				{
					m_LookTarget = blobs[randomBlobIndex].transform;
				}

				m_BlinkTimer = UnityEngine.Random.Range(m_MinBlinkTime, m_MaxBlinkTime);
				m_SpriteRenderer.sprite = m_ClosedSprite;

				m_PupilLeft.SetActive(false);
				m_PupilRight.SetActive(false);
			}
			else
			{
				m_BlinkTimer = UnityEngine.Random.Range(m_MinBlinkInterval, m_MaxBlinkInterval);
				m_SpriteRenderer.sprite = m_OpenSprite;

				m_PupilLeft.SetActive(true);
				m_PupilRight.SetActive(true);
			}
		}

		float lookFilterFactor = 0.1f;
		Vector3 desiredLookDirection;

		if(m_LookTarget != null)
		{
			desiredLookDirection = m_LookTarget.transform.position - this.transform.position;
		}
		else
		{
			desiredLookDirection = Vector2.zero;
		}

		desiredLookDirection.Normalize();

		m_LookDirection.x = (m_LookDirection.x * (1.0f - lookFilterFactor)) + (desiredLookDirection.x * lookFilterFactor);
		m_LookDirection.y = (m_LookDirection.y * (1.0f - lookFilterFactor)) + (desiredLookDirection.y * lookFilterFactor);

		m_PupilLeft.transform.localPosition = m_PupilLeftCentre + (m_LookDirection * m_EyeRadius);
		m_PupilRight.transform.localPosition = m_PupilRightCentre + (m_LookDirection * m_EyeRadius);
	}
}
