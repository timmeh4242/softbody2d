namespace FinerGames {

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
public class Softmesh2D : MonoBehaviour {

	public Sprite m_Sprite;

	#region PRIVATE_VARIABLES
	// Internal rendering data
	public Vector3[] 	m_Vertices;
	Vector3[] 	m_InitialVertexPositions;
	Color[] 	m_Colors;
	Vector2[] 	m_TexCoords;
	int[] 		m_Triangles;
	Mesh 		m_SpriteMesh;    

	public Softbody2D m_Softbody2D;
//	public List<ReferencePoint> m_Softbody2D.m_PointMasses;

	// Reference point->vertex weighting values
	float[,] m_ReferencePointWeightings;

	// Reference point->attach point weighting valuse
	float[,] m_AttachPointWeightings;

	// Initial attach point positions
	Vector3[] m_InitialAttachPointPositions = new Vector3[0];

	// Saves us checking components every frame to see if an
	// attached object is actually another Jelly Sprite
	bool[] m_IsAttachPointJellySprite = new bool[0];

	// Parent object for rigidbodies
	GameObject m_ReferencePointParent;

	// Central body point
//	ReferencePoint m_CentralPoint;

	// List of reference point offset
	Vector3[] m_ReferencePointOffsets;

	// Cached transform
	Transform m_Transform;
	#endregion

	public Vector2 m_SpriteScale = Vector2.one;
	public int m_VertexDensity = 10;
	public float m_DistanceExponent = 2.0f;

	static List<Material> s_MaterialList = new List<Material>();

	void Start () {

		m_Transform = this.transform;
		Bounds spriteBounds = new Bounds();

		if (IsSpriteValid ()) {
			spriteBounds = GetSpriteBounds ();
			InitVertices (spriteBounds);
			InitMaterial ();
			InitMesh ();
		} else {
			MeshFilter meshFilter = GetComponent<MeshFilter> ();

			// If the user hasn't supplied a mesh, attempt to extract it from the meshfilter
			if (Application.isPlaying && meshFilter.sharedMesh != null) {
				m_SpriteMesh = meshFilter.sharedMesh;
				m_Vertices = m_SpriteMesh.vertices;
				m_InitialVertexPositions = m_SpriteMesh.vertices;
				m_Triangles = m_SpriteMesh.triangles;
				m_TexCoords = m_SpriteMesh.uv;
				m_Colors = m_SpriteMesh.colors;
				spriteBounds = m_SpriteMesh.bounds;
				m_SpriteScale = Vector3.one;
			} else if (Application.isPlaying) {
				Debug.LogError ("Failed to initialize Jelly Sprite " + name + " - no valid sprite or mesh");
				this.enabled = false;
				return;
			}
		}
		
		m_ReferencePointOffsets = new Vector3[m_Softbody2D.m_PointMasses.Count];
		CalculateWeightingValues ();
	}

	public static void ClearMaterials()
	{
		s_MaterialList.Clear();
	}
		
	Bounds GetSpriteBounds()
	{
		return m_Sprite.bounds;
	}

	bool IsSpriteValid()
	{
		return m_Sprite != null;
	}

	bool IsSourceSpriteRotated()
	{
		return false;
	}

	void GetMinMaxTextureRect(out Vector2 min, out Vector2 max)
	{
		Rect textureRect = m_Sprite.textureRect;
		min = new Vector2(textureRect.xMin/(float)m_Sprite.texture.width, textureRect.yMin/(float)m_Sprite.texture.height);
		max = new Vector2(textureRect.xMax/(float)m_Sprite.texture.width, textureRect.yMax/(float)m_Sprite.texture.height);
	}

	/// <summary>
	/// Adjust our texture coordinates from a 0-1 scale to point at the correct offset into the
	/// sprite rectangle</summary>
	void FixupTextureCoordinates(Vector2 minTextureCoords, Vector2 maxTextureCoords)
	{
		for(int vertexIndex = 0; vertexIndex < m_Vertices.Length; vertexIndex++)
		{
			Vector2 spriteOffset = maxTextureCoords - minTextureCoords;
			spriteOffset.Scale(m_TexCoords[vertexIndex]);
			m_TexCoords[vertexIndex] = minTextureCoords + spriteOffset;
		}
	}

	/// <summary>
	/// Initialise the grid of vertices that will be used to render this object.
	/// </summary>
	void InitVertices(Bounds spriteBounds)
	{
		float width = spriteBounds.size.x * m_SpriteScale.x;
		float height = spriteBounds.size.y * m_SpriteScale.y;

		// Work out how many nodes we need in each direction
		float nodeDistance = Mathf.Min(width, height)/m_VertexDensity;
		int vertexGridWidth = Mathf.CeilToInt(width/nodeDistance);
		int vertexGridHeight = Mathf.CeilToInt(height/nodeDistance);

		// Set up our texture coordinates for each vertex
		int numVertices = vertexGridWidth * vertexGridHeight;
		m_Vertices = new Vector3[numVertices];
		m_InitialVertexPositions = new Vector3[numVertices];
		m_Colors = new Color[numVertices];
		m_TexCoords = new Vector2[numVertices];
		m_Triangles = new int[numVertices * 6];

		bool rotated = IsSourceSpriteRotated();

		// Work out vertex positions and texture coordinates
		for(int x = 0; x < vertexGridWidth; x++)
		{
			for(int y = 0; y < vertexGridHeight; y++)
			{
				int vertexIndex = (x * vertexGridHeight) + y;
				Vector2 uv = Vector2.zero;
				uv.x = x/((float)vertexGridWidth - 1);
				uv.y = y/((float)vertexGridHeight - 1);

//				if(m_FlipX)
//				{
//					uv.x = 1.0f - uv.x;
//				}
//
//				if(m_FlipY)
//				{
//					uv.y = 1.0f - uv.y;
//				}

				if(rotated)
				{
					float temp = uv.x;
					uv.x = 1.0f;
					uv.y = temp;
				}

				m_TexCoords[vertexIndex] = uv;
				m_Colors[vertexIndex] = Color.white;

				Vector3 vertexPosition = Vector3.zero;
				vertexPosition.x = (-width * 0.5f) + ((width/((float)vertexGridWidth - 1)) * x);
				vertexPosition.y = (-height * 0.5f) + ((height/((float)vertexGridHeight - 1)) * y);
				m_Vertices[vertexIndex] = vertexPosition;
			}
		}

		m_Vertices.CopyTo(m_InitialVertexPositions, 0);

		// Generate triangle indices
		int numTriangles = 0;

		for(int x = 0; x < vertexGridWidth - 1; x++)
		{
			for(int y = 0; y < vertexGridHeight - 1; y++)
			{
				int p0 = (x * vertexGridHeight) + y;
				int p1 = (x * vertexGridHeight) + (y + 1);
				int p2 = ((x + 1) * vertexGridHeight) + (y + 1);
				int p3 = ((x + 1) * vertexGridHeight) + y;

				m_Triangles[numTriangles++] = p0;
				m_Triangles[numTriangles++] = p1;
				m_Triangles[numTriangles++] = p2;

				m_Triangles[numTriangles++] = p3;
				m_Triangles[numTriangles++] = p0;
				m_Triangles[numTriangles++] = p2;
			}
		}

		Vector2 minTextureCoords;
		Vector2 maxTextureCoords;

		GetMinMaxTextureRect(out minTextureCoords, out maxTextureCoords);
		FixupTextureCoordinates(minTextureCoords, maxTextureCoords);
	}

	void InitMaterial()
	{
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		Material material = null;

		// Grab a material from the cache, generate a new one if none exist
		for(int loop = 0; loop < s_MaterialList.Count; loop++)
		{
			if(s_MaterialList[loop] != null && s_MaterialList[loop].mainTexture.GetInstanceID() == m_Sprite.texture.GetInstanceID())
			{
				material = s_MaterialList[loop];
			}
		}

		if(material == null)
		{
			material = new Material(Shader.Find("Sprites/Default"));
			material.mainTexture = m_Sprite.texture;
			material.name = m_Sprite.texture.name + "_Softbody";
			s_MaterialList.Add(material);
		}

		meshRenderer.sharedMaterial = material;
	}

	/// <summary>
	/// First time setup of the mesh
	/// </summary>
	public void InitMesh()
	{
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		m_SpriteMesh = new Mesh();
		m_SpriteMesh.name = "Softbody Mesh";
		m_SpriteMesh.MarkDynamic();
		meshFilter.mesh = m_SpriteMesh;

		m_SpriteMesh.Clear();
		m_SpriteMesh.vertices = m_Vertices;
		m_SpriteMesh.uv = m_TexCoords;
		m_SpriteMesh.triangles = m_Triangles;
		m_SpriteMesh.colors = m_Colors;
		m_SpriteMesh.RecalculateBounds();
		m_SpriteMesh.RecalculateNormals();
	}

	public void CalculateWeightingValues()
	{
		float inverseScaleX = 1.0f / m_Transform.localScale.x;
		float inverseScaleY = 1.0f / m_Transform.localScale.y;

		if(m_Softbody2D.m_PointMasses != null)
		{
			m_ReferencePointWeightings = new float[m_Vertices.Length, m_Softbody2D.m_PointMasses.Count];

			for(int vertexIndex = 0; vertexIndex < m_Vertices.Length; vertexIndex++)
			{
				float distanceSum = 0.0f;

				for(int referencePointIndex = 0; referencePointIndex < m_Softbody2D.m_PointMasses.Count; referencePointIndex++)
				{
//					if(!m_ReferencePoints[referencePointIndex].IsDummy)
//					{
//						Vector3 offset = m_ReferencePoints[referencePointIndex].InitialOffset;
						Vector3 offset = Vector3.zero;
						
						offset.x = offset.x * inverseScaleX;
						offset.y = offset.y * inverseScaleY;
						float distance = Vector2.Distance(offset, m_Vertices[vertexIndex]);
						distance = Mathf.Pow(distance, m_DistanceExponent);
						float invDistance = float.MaxValue;

						if(distance > 0.0f)
						{
							invDistance = 1.0f/distance;
						}

						distanceSum += invDistance;
//					}
				}                

				for(int referencePointIndex = 0; referencePointIndex < m_Softbody2D.m_PointMasses.Count; referencePointIndex++)
				{
//					if(!m_ReferencePoints[referencePointIndex].IsDummy)
//					{
//						Vector3 offset = m_ReferencePoints[referencePointIndex].InitialOffset;
						Vector3 offset = Vector3.zero;

						offset.x = offset.x * inverseScaleX;
						offset.y = offset.y * inverseScaleY;
						float distance = Vector2.Distance(offset, m_Vertices[vertexIndex]);
						distance = Mathf.Pow(distance, m_DistanceExponent);
						float invDistance = float.MaxValue;

						if(distance > 0.0f)
						{
							invDistance = 1.0f/distance;
						}

						m_ReferencePointWeightings[vertexIndex, referencePointIndex] = invDistance/distanceSum;                   
//					}
				}
			}
		}

//		if(m_AttachPoints != null && m_ReferencePoints != null)
//		{
//			m_AttachPointWeightings = new float[m_AttachPoints.Length, m_ReferencePoints.Count];
//
//			for(int attachPointIndex = 0; attachPointIndex < m_AttachPoints.Length; attachPointIndex++)
//			{
//				float distanceSum = 0.0f;
//
//				for(int referencePointIndex = 0; referencePointIndex < m_ReferencePoints.Count; referencePointIndex++)
//				{
//					if(!m_ReferencePoints[referencePointIndex].IsDummy)
//					{
//						Vector3 offset = m_ReferencePoints[referencePointIndex].InitialOffset;
//						offset.x = offset.x * inverseScaleX;
//						offset.y = offset.y * inverseScaleY;
//						float distance = Vector2.Distance(offset, m_AttachPoints[attachPointIndex].localPosition);
//						distance = Mathf.Pow(distance, m_DistanceExponent);
//						float invDistance = float.MaxValue;
//
//						if(distance > 0.0f)
//						{
//							invDistance = 1.0f/distance;
//						}
//
//						distanceSum += invDistance;
//					}
//				}
//
//				for(int referencePointIndex = 0; referencePointIndex < m_ReferencePoints.Count; referencePointIndex++)
//				{
//					if(!m_ReferencePoints[referencePointIndex].IsDummy)
//					{
//						Vector3 offset = m_ReferencePoints[referencePointIndex].InitialOffset;
//						offset.x = offset.x * inverseScaleX;
//						offset.y = offset.y * inverseScaleY;
//						float distance = Vector2.Distance(offset, m_AttachPoints[attachPointIndex].localPosition);
//						distance = Mathf.Pow(distance, m_DistanceExponent);
//						float invDistance = float.MaxValue;
//
//						if(distance > 0.0f)
//						{
//							invDistance = 1.0f/distance;
//						}
//
//						m_AttachPointWeightings[attachPointIndex, referencePointIndex] = invDistance/distanceSum;
//					}
//				}
//			}
//		}
	}

	public void UpdateMesh() {
		Bounds spriteBounds = new Bounds();
		spriteBounds = GetSpriteBounds ();
		InitVertices (spriteBounds);
		InitMaterial ();
		InitMesh ();
	}

	public void UpdateMesh2()
	{
			// For each vertex, look at the offset values of each reference point and apply the same offset
			// (scaled by the weighting value) to the vertex's position
			if (Application.isPlaying) {
				// Calculate reference point offsets
				bool haveAnyPointsMoved = false;

				for (int referencePointIndex = 0; referencePointIndex < m_Softbody2D.m_PointMasses.Count; referencePointIndex++) {				
//					if (!m_Softbody2D.m_PointMasses [referencePointIndex].IsDummy && m_Softbody2D.m_PointMasses [referencePointIndex] != m_CentralPoint) {					
						PointMass referencePoint = m_Softbody2D.m_PointMasses [referencePointIndex];					
						Vector3 offset = m_Softbody2D.m_CentralPointMass.transform.InverseTransformPoint (referencePoint.transform.position);
//						offset -= referencePoint.InitialOffset;

//						if (haveAnyPointsMoved || m_ReferencePointOffsets [referencePointIndex] != offset) {
							m_ReferencePointOffsets [referencePointIndex] = offset;
							haveAnyPointsMoved = true;
//						}
//					} else {					
//						m_ReferencePointOffsets [referencePointIndex] = Vector3.zero;					
//					}
				}

				if (!haveAnyPointsMoved) {
					return;
				}

				int numVertices = m_Vertices.Length;
				int numReferencePoints = m_Softbody2D.m_PointMasses.Count;
//				int centralPointIndex = GetCentralPointIndex ();

				for (int vertexIndex = 0; vertexIndex < numVertices; vertexIndex++) {
					Vector3 totalOffset = Vector3.zero;

					for (int referencePointIndex = 0; referencePointIndex < numReferencePoints; referencePointIndex++) {
//						if (referencePointIndex != centralPointIndex && !m_Softbody2D.m_PointMasses [referencePointIndex].IsDummy) {
							totalOffset += m_ReferencePointOffsets [referencePointIndex] * m_ReferencePointWeightings [vertexIndex, referencePointIndex];
//							totalOffset += m_ReferencePointOffsets [referencePointIndex] * m_Softbody2D.m_PointMasses [referencePointIndex].GetComponent<Rigidbody2D>().mass;
//						}
					}

//					m_Vertices [vertexIndex] = m_InitialVertexPositions [vertexIndex] + totalOffset + new Vector3 (m_CentralBodyOffset.x, m_CentralBodyOffset.y, 0);
					m_Vertices [vertexIndex] = m_InitialVertexPositions [vertexIndex] + totalOffset + new Vector3 (0, 0, 0);
				}

				// Update the mesh
				m_SpriteMesh.vertices = m_Vertices;
				m_SpriteMesh.RecalculateBounds ();
				m_SpriteMesh.RecalculateNormals ();
			}
		}
	}
}
