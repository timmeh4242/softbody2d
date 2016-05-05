using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JellySpriteDemoManager : MonoBehaviour 
{
	bool m_DrawPhysicsBodies = false;
	GameObject[] m_BlobObjects;
	bool m_DebugRenderersCreated;
	public Material m_DebugRenderMaterial;

	// Use this for initialization
	void Start () 
	{
		m_BlobObjects = GameObject.FindGameObjectsWithTag("Blob");
		Physics.gravity = new Vector3(0, -50, 0);
	}

	/// <summary>
	/// Turn all the debug sprite renderers on/off
	/// </summary>
	void SetSpriteRenderersEnabled(bool enabled)
	{
		foreach(GameObject gameObject in m_BlobObjects)
		{
			JellySprite jellySprite = gameObject.GetComponent<JellySprite>();

			if(jellySprite)
			{
				foreach(JellySprite.ReferencePoint referencePoint in jellySprite.ReferencePoints)
				{
					GameObject refPointObject = referencePoint.GameObject;
					
					if(refPointObject)
					{
						MeshRenderer meshRenderer = refPointObject.GetComponent<MeshRenderer>();

						if(meshRenderer)
						{
							meshRenderer.enabled = enabled;
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Draws the GUI
	/// </summary>
	void OnGUI () 
	{
		if(m_DrawPhysicsBodies)
		{
			if(GUI.Button(new Rect(20,20,200,20), "Render Physics Bodies: On")) 
			{
				m_DrawPhysicsBodies = !m_DrawPhysicsBodies;
				SetSpriteRenderersEnabled(false);
			}
		}
		else
		{
			if(GUI.Button(new Rect(20,20,200,20), "Render Physics Bodies: Off")) 
			{
				m_DrawPhysicsBodies = !m_DrawPhysicsBodies;

				if(!m_DebugRenderersCreated)
				{
					foreach(GameObject gameObject in m_BlobObjects)
					{
						JellySprite jellySprite = gameObject.GetComponent<JellySprite>();

						if(jellySprite)
						{
							// Go through and create a simple circle mesh for each body
							foreach(JellySprite.ReferencePoint referencePoint in jellySprite.ReferencePoints)
							{
								if(referencePoint.InitialOffset == Vector3.zero)
								{
									continue;
								}
								
								GameObject referencePointObject = referencePoint.GameObject;
								
								if(referencePointObject)
								{
									MeshRenderer meshRenderer = referencePointObject.AddComponent<MeshRenderer>();
									MeshFilter meshFilter = referencePointObject.AddComponent<MeshFilter>();
									
									Mesh newMesh = new Mesh();
									newMesh.name = "JellySprite Debug Mesh";
									
									List<Vector3> meshVertices = new List<Vector3>();
									List<int> meshTriangles = new List<int>();
									List<Vector2> meshUVs = new List<Vector2>();
									List<Color> meshColors = new List<Color>();
									
									meshVertices.Add(Vector3.zero);
									meshUVs.Add(Vector2.zero);
									meshColors.Add (Color.white);
									
									for(int loop = 0; loop <= 32; loop++)
									{
										float angle = ((Mathf.PI * 2.0f)/32.0f) * loop;
										float x = Mathf.Cos(angle) * referencePoint.Radius;
										float y = Mathf.Sin(angle) * referencePoint.Radius;
										meshVertices.Add(new Vector3(x, y, 0.0f));
										meshUVs.Add(Vector2.one);
										meshColors.Add (Color.white);
									}
									
									for(int loop = 1; loop <= 32; loop++)
									{
										meshTriangles.Add(0);
										meshTriangles.Add(loop);
										meshTriangles.Add(loop -1);
									}
									
									meshTriangles.Add(0);
									meshTriangles.Add(1);
									meshTriangles.Add(32);
									
									newMesh.Clear();
									newMesh.vertices = meshVertices.ToArray();
									newMesh.uv = meshUVs.ToArray();
									newMesh.colors = meshColors.ToArray();
									newMesh.triangles = meshTriangles.ToArray();
									newMesh.RecalculateBounds();
									newMesh.RecalculateNormals();
									meshFilter.mesh = newMesh;
									
									meshRenderer.sharedMaterial = m_DebugRenderMaterial;
									meshRenderer.sortingOrder = 500;
								}
							}
						}
					}
					
					m_DebugRenderersCreated = true;
				}

				SetSpriteRenderersEnabled(true);
			}
		}
	}
}
