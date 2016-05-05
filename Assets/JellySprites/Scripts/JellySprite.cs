//#define TOOLKIT2D_SUPPORT_ENABLED
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif 

using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
public abstract class JellySprite : MonoBehaviour 
{
#region PUBLIC_VARIABLES
	// Arrangement of the physics bodies
	public enum PhysicsStyle
	{
		Circle,
		Rectangle,
		Triangle,
		Grid,
		Free,
	}

	public PhysicsStyle m_Style = PhysicsStyle.Circle;

	/// <summary>
	/// Controls body positions in free mode
	/// </summary>
	public List<Vector3> m_FreeModeBodyPositions;
	public List<float> m_FreeModeBodyRadii;
	public List<bool> m_FreeModeBodyKinematic;

	// Whether the chosen mass value is applied per rigid body or
	// to the soft body as a whole
	public enum MassStyle
	{
		Global,
		PerBody
	}

	public MassStyle m_MassStyle = MassStyle.PerBody;

	// Physics materials for 2D/3D modes
	public PhysicMaterial m_PhysicsMaterial;
	public PhysicsMaterial2D m_PhysicsMaterial2D;
	
	// How many vertices make up the rendered physics mesh
	public int m_VertexDensity = 10;

	// Radius of the rigid body colliders (given as a % of sprite size)
	public float m_SphereRadius = 0.25f;

	// How strongly we map reference points to mesh vertices. Higher values will make the mesh vertex distortion
	// more accurately correspond to the rigid body movement, but may cause visual artefacts, especially on
	// soft bodies made up of only a few rigid bodies.
	public float m_DistanceExponent = 2.0f;

	// How stiff the soft body physics springs are
	public float m_Stiffness = 2.5f;

	// The mass of the entire sprite (each of the n reference point is 1/n times this value)
	public float m_Mass = 1.0f;

	// Whether or not to lock rotation around the Z-axis
	public bool m_LockRotation = true;

	// Gravity scale (in 2D mode)
	public float m_GravityScale = 1.0f;

	// Whether child bodies should collider with one another
	public bool m_CollideConnected = false;

	// Whether to make the central body kinematic (ie. not move)
	public bool m_CentralBodyKinematic = false;

	// Drag (in 3D mode)
	public float m_Drag = 0.0f;
	public float m_AngularDrag = 0.05f;

	// Circle-configuration only - how many rigid bodies are placed around the circle radius
	public int m_RadiusPoints = 8;

	// Grid-configuration only - how many rigid bodies make up the grid in each dimension
	public int m_GridColumns = 4;
	public int m_GridRows = 4;
	
	// The amount by which the spring force is reduced in proportion to the movement speed. The spring will oscillate
	// with a certain frequency as it attempts to reestablish the desired distance between the objects. The higher
	// the damping ratio, the quicker the oscillation will die down to zero.
	public float m_DampingRatio = 0.0f;

	/// <summary>
	/// Used to scale the collider up/down without adjusting the actual sprite size
	/// </summary>
	public Vector2 m_SoftBodyScale = Vector2.one;

	/// <summary>
	/// Used to rotate the collider independently of the sprite
	/// </summary>
	public float m_SoftBodyRotation = 0.0f;

    /// <summary>
    /// Used to offset the soft bodies
    /// </summary>
    public Vector2 m_SoftBodyOffset = Vector2.zero;

    /// <summary>
    /// Used to offset the central soft body
    /// </summary>
    public Vector2 m_CentralBodyOffset = Vector2.zero;

	/// <summary>
	/// Used to scale the sprite size
	/// </summary>
	public Vector2 m_SpriteScale = Vector2.one;

	// Whether to use 2D or 3D rigid bodies/colliders
	public bool m_2DMode = false;

	// Controls whether bodies are attached to their neighboring bodies as well as to
	// the central point
	public bool m_AttachNeighbors = false;

	// Flip the sprite horizontally/vertically
	public bool m_FlipX = false;
	public bool m_FlipY = false;

	// Array of attach points - used to attach child objects to this jelly sprite
	public int m_NumAttachPoints = 0;
	public Transform[] m_AttachPoints = new Transform[0];

    // Tint color
    public Color m_Color = Color.white;

	public List<ReferencePoint> ReferencePoints { get { return m_ReferencePoints; } }
	public ReferencePoint CentralPoint { get { return m_CentralPoint; } }
#endregion

#region PRIVATE_VARIABLES
	// Internal rendering data
	Vector3[] 	m_Vertices;
	Vector3[] 	m_InitialVertexPositions;
	Color[] 	m_Colors;
	Vector2[] 	m_TexCoords;
	int[] 		m_Triangles;
	Mesh 		m_SpriteMesh;    

	// Physics reference points
	public List<ReferencePoint> m_ReferencePoints;

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
	ReferencePoint m_CentralPoint;

	// List of reference point offset
	Vector3[] m_ReferencePointOffsets;

	// Cached transform
	Transform m_Transform;
#endregion

#region PUBLIC_CLASSES
	/// <summary>
	/// The ReferencePoint class encapsulates a rigid body (2D or 3D) and information about
	/// the bodies initial position. From there, we can work out how much the body has moved 
	/// from its initial position and then map the movement to the visible mesh.
	/// </summary>
	public class ReferencePoint
	{
		public Vector3 InitialOffset { get { return m_InitialOffset; } set { m_InitialOffset = value; } }

		public Rigidbody2D Body2D { get { return m_RigidBody2D; } }
		public Rigidbody Body3D { get { return m_RigidBody3D; } }

		public CircleCollider2D Collider2D { get { return m_CircleCollider2D; } }
		public SphereCollider Collider { get { return m_SphereCollider; } }

		public bool IsDummy { get { return m_IsDummy; } }

		Transform m_Transform;
		Rigidbody2D m_RigidBody2D;
		Rigidbody m_RigidBody3D;
		CircleCollider2D m_CircleCollider2D;
		SphereCollider m_SphereCollider;
		Vector3 m_InitialOffset;
		bool m_IsDummy = true;

		/// <summary>
		/// ReferencePoint 2D Constructor
		/// </summary>
		public ReferencePoint(Rigidbody2D body)
		{
            if (body != null)
            {
                m_RigidBody2D = body;
                m_CircleCollider2D = body.GetComponent<CircleCollider2D>();
                m_Transform = body.transform;
                m_IsDummy = false;
            }
            else
            {
                m_IsDummy = true;
            }			
		}

		/// <summary>
		/// ReferencePoint 3D Constructor
		/// </summary>
		public ReferencePoint(Rigidbody body)
		{
            if (body != null)
            {
                m_RigidBody3D = body;
                m_SphereCollider = body.GetComponent<SphereCollider>();
                m_Transform = body.transform;
                m_IsDummy = false;
            }
            else
            {
                m_IsDummy = true;
            }			
		}

		/// <summary>
		/// Get the radius of the rigid body
		/// </summary>
		public float Radius 
		{ 
			get 
			{ 
				if(m_CircleCollider2D != null)
				{
					return m_CircleCollider2D.radius;
				}
				else if(m_SphereCollider != null)
				{
					return m_SphereCollider.radius;
				}
				
				return 0.0f;
			} 
		}

		/// <summary>
		/// Gets the game object.
		/// </summary>
		public GameObject GameObject 
		{ 
			get 
			{
				if(m_RigidBody2D != null)
				{
					return m_RigidBody2D.gameObject;
				}
				else if(m_RigidBody3D != null)
				{
					return m_RigidBody3D.gameObject;
				}
				
				return null;
			}
		}

		/// <summary>
		/// Gets the transform.
		/// </summary>
		public Transform transform { get { return m_Transform; } }

		/// <summary>
		/// Set the kinematic flag on this object
		/// </summary>
		public void SetKinematic(bool kinematic)
		{
			if(m_RigidBody2D != null)
			{
				m_RigidBody2D.isKinematic = kinematic;
			}
			else if(m_RigidBody3D != null)
			{
				m_RigidBody3D.isKinematic = kinematic;
			}
		}
	}

	/// <summary>
	/// Helper class for passing information about collisions
	/// </summary>
	public class JellyCollision
	{
		public Collision Collision { get; set; }
		public JellySpriteReferencePoint ReferencePoint { get; set; }
	}

	/// <summary>
	/// Helper class for passing information about 2D collisions
	/// </summary>
	public class JellyCollision2D
	{
		public Collision2D Collision2D { get; set; }
		public JellySpriteReferencePoint ReferencePoint { get; set; }
	}

	/// <summary>
	/// Helper class for passing information about triggers
	/// </summary>
	public class JellyCollider
	{
		public Collider Collider { get; set; }
		public JellySpriteReferencePoint ReferencePoint { get; set; }
	}
	
	/// <summary>
	/// Helper class for passing information about 2D collisions
	/// </summary>
	public class JellyCollider2D
	{
		public Collider2D Collider2D { get; set; }
		public JellySpriteReferencePoint ReferencePoint { get; set; }
	}
#endregion

	/// <summary>
	/// JellySprite constructor
	/// </summary>
	void Awake()
	{
		m_Transform = this.transform;
	}

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		if(m_FreeModeBodyPositions == null)
		{
			m_FreeModeBodyPositions = new List<Vector3>();
			m_FreeModeBodyPositions.Add(Vector3.zero);

			m_FreeModeBodyRadii = new List<float>();
			m_FreeModeBodyRadii.Add(1.0f);

			m_FreeModeBodyKinematic = new List<bool>();
			m_FreeModeBodyKinematic.Add(false);
		}

		// Maintaining support for users upgrading from 1.07 to 1.08
		if(m_FreeModeBodyKinematic.Count != m_FreeModeBodyPositions.Count)
		{
			m_FreeModeBodyKinematic = new List<bool>();

			for(int loop = 0; loop < m_FreeModeBodyPositions.Count; loop++)
			{
				m_FreeModeBodyKinematic.Add(false);
			}
		}

		Bounds spriteBounds = new Bounds();

		if(IsSpriteValid())
		{
			spriteBounds = GetSpriteBounds();
			InitVertices(spriteBounds);
			InitMaterial();
			InitMesh();
		}
		else
		{
			MeshFilter meshFilter = GetComponent<MeshFilter>();

			// If the user hasn't supplied a mesh, attempt to extract it from the meshfilter
			if(Application.isPlaying && meshFilter.sharedMesh != null)
			{
				m_SpriteMesh = meshFilter.sharedMesh;
				m_Vertices = m_SpriteMesh.vertices;
				m_InitialVertexPositions = m_SpriteMesh.vertices;
				m_Triangles = m_SpriteMesh.triangles;
				m_TexCoords = m_SpriteMesh.uv;
				m_Colors = m_SpriteMesh.colors;
				spriteBounds = m_SpriteMesh.bounds;
				m_SpriteScale = Vector3.one;
			}
			else if(Application.isPlaying)
			{
				Debug.LogError("Failed to initialize Jelly Sprite " + name + " - no valid sprite or mesh");
				this.enabled = false;
				return;
			}
		}

		m_InitialAttachPointPositions = new Vector3[m_AttachPoints.Length];
		m_IsAttachPointJellySprite = new bool[m_AttachPoints.Length];

		if(Application.isPlaying)
		{
			Vector3 spriteAngle = m_Transform.eulerAngles;
			m_Transform.eulerAngles = Vector3.zero;

#if UNITY_4_3
			if(m_2DMode && !Physics2D.GetIgnoreLayerCollision(this.gameObject.layer, this.gameObject.layer))
			{
				Debug.LogError("Layer '" + LayerMask.LayerToName(this.gameObject.layer) + "' is set to collide with itself - soft body physics will not work as intended. Please disable collisions between this layer and itself (Edit->Project Settings->Physics 2D)");
				return;
			}
#endif

			m_ReferencePointParent = new GameObject();
			m_ReferencePointParent.name = this.name + " Reference Points";

			m_ReferencePoints = new List<ReferencePoint>();

			switch(m_Style)
			{
			case PhysicsStyle.Circle:
				CreateRigidBodiesCircle(spriteBounds);
				break;
			case PhysicsStyle.Triangle:
				CreateRigidBodiesTriangle(spriteBounds);
				break;
			case PhysicsStyle.Rectangle:
				CreateRigidBodiesRectangle(spriteBounds);
				break;
			case PhysicsStyle.Grid:
				CreateRigidBodiesGrid(spriteBounds);
				break;
			case PhysicsStyle.Free:
				CreateRigidBodiesFree(spriteBounds);
				break;
			}

			if(m_CentralPoint != null)
			{
				m_CentralPoint.GameObject.name = this.name + " Central Ref Point";
			}

			if(m_Style != PhysicsStyle.Free)
			{
				UpdateRotationLock();
			}

			CalculateInitialOffsets();
			InitMass();
			CalculateWeightingValues();
			SetupCollisions();

			m_ReferencePointOffsets = new Vector3[m_ReferencePoints.Count];

			foreach(ReferencePoint referencePoint in m_ReferencePoints)
			{
				if(!referencePoint.IsDummy)
				{
					Vector3 referencePointPosition = referencePoint.transform.position;
					Vector3 centralPointPosition = m_Transform.position;
					referencePoint.transform.position = centralPointPosition + (Quaternion.Euler(spriteAngle) * (referencePointPosition - centralPointPosition));
				}
			}

			m_CentralPoint.transform.eulerAngles = spriteAngle;
		}
	}

	/// <summary>
	/// Calculates the initial offsets of each reference point
	/// </summary>
	void CalculateInitialOffsets()
	{
		foreach(ReferencePoint referencePoint in m_ReferencePoints)
		{
			if(referencePoint.GameObject && referencePoint != m_CentralPoint)
			{
				referencePoint.InitialOffset = m_CentralPoint.transform.InverseTransformPoint(referencePoint.transform.position);
			}
		}
	
		int index = 0;

		foreach(Transform attachPointTransform in m_AttachPoints)
		{
			JellySprite attachedJellySprite = attachPointTransform.GetComponent<JellySprite>();
            Vector3 position = m_CentralPoint.transform.InverseTransformPoint(attachPointTransform.position);
            position.x /= m_Transform.localScale.x;
            position.y /= m_Transform.localScale.y;
			
			if(attachedJellySprite)
			{
				m_IsAttachPointJellySprite[index] = true;
                m_InitialAttachPointPositions[index++] = position;
			}
			else
			{
				m_IsAttachPointJellySprite[index] = false;                
                m_InitialAttachPointPositions[index++] = position;
				attachPointTransform.parent = m_Transform;
			}
		}

		for(int loop = 0; loop < m_Vertices.Length; loop++)
		{
			m_InitialVertexPositions[loop] -= m_Transform.InverseTransformPoint(m_CentralPoint.transform.position);
			m_Vertices[loop] -= m_Transform.InverseTransformPoint(m_CentralPoint.transform.position);
		}
	}

	/// <summary>
	/// Raises the enable event.
	/// </summary>
	void OnEnable()
	{
        if(m_ReferencePointParent)
        {
            m_ReferencePointParent.SetActive(true);
        }

		// Collisions need to be set up again each time the object is activated
		SetupCollisions();
	}

    /// <summary>
    /// Raises the disable event
    /// </summary>
    void OnDisable()
    {
        if (m_ReferencePointParent)
        {
            m_ReferencePointParent.SetActive(false);
        }
    }

	/// <summary>
	/// Get the bounds of the sprite
	/// </summary>
	protected abstract Bounds GetSpriteBounds();

	/// <summary>
	/// Check if the sprite is valid
	/// </summary>
	protected abstract bool IsSpriteValid();
	
	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		if(m_ReferencePointParent != null)
		{
			Destroy(m_ReferencePointParent);
		}
	}

	/// <summary>
	/// Create reference points in a circular formation around the central body. Each point is linked to
	/// its neighbors and to the center
	/// </summary>
	void CreateRigidBodiesCircle(Bounds spriteBounds)
	{
		int numPoints = m_RadiusPoints;
		float width = spriteBounds.size.x * m_SpriteScale.x;
		float radius = width * 0.5f;
        float sphereRadius = m_SphereRadius * m_Transform.localScale.x;

        m_CentralPoint = AddReferencePoint(m_CentralBodyOffset, width * sphereRadius, m_LockRotation);

		// Add nodes in a circle around the centre
		for(int loop = 0; loop < numPoints; loop++)
		{
			// Work out the correct offset to place the node
			float angle = ((Mathf.PI * 2)/numPoints) * loop;
			Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
			offset *= radius;
			offset.x *= m_SoftBodyScale.x;
			offset.y *= m_SoftBodyScale.y;
            Vector3 bodyPosition = offset * (1.0f - ((sphereRadius * width) / (m_Transform.localScale.x * offset.magnitude))) + m_SoftBodyOffset;
            ReferencePoint referencePoint = AddReferencePoint(bodyPosition, width * sphereRadius, true);
			AttachPoint(referencePoint, m_CentralPoint);
		}

		if(m_AttachNeighbors)
		{
			for(int loop = 2; loop < m_ReferencePoints.Count; loop++)
			{
				AttachPoint(m_ReferencePoints[loop], m_ReferencePoints[loop - 1]);
			}

			AttachPoint(m_ReferencePoints[m_ReferencePoints.Count - 1], m_ReferencePoints[1]);
		}
	}

	/// <summary>
	/// Create reference points in a triangle formation. Each point is connected to the central point
	/// and to its neighbors
	/// </summary>
	void CreateRigidBodiesTriangle(Bounds spriteBounds)
	{
		float width = spriteBounds.size.x * m_SoftBodyScale.x * m_SpriteScale.x;
		float height = spriteBounds.size.y * m_SoftBodyScale.y * m_SpriteScale.y;
        float radius = spriteBounds.size.y * m_SphereRadius * m_SpriteScale.y * m_Transform.localScale.y;
		float offsetFactor = 0.5f - m_SphereRadius;

		m_CentralPoint = AddReferencePoint(m_CentralBodyOffset, radius, m_LockRotation);

		ReferencePoint bottomLeftPoint = AddReferencePoint(new Vector2(-width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset, radius, true);
		AttachPoint(bottomLeftPoint, m_CentralPoint);

        ReferencePoint bottomRightPoint = AddReferencePoint(new Vector2(width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset, radius, true);
		AttachPoint(bottomRightPoint, m_CentralPoint);

        ReferencePoint topCentrePoint = AddReferencePoint(new Vector2(0.0f, height * offsetFactor) + m_SoftBodyOffset, radius, true);
		AttachPoint(topCentrePoint, m_CentralPoint);

		if(m_AttachNeighbors)
		{
			AttachPoint(m_ReferencePoints[1], m_ReferencePoints[2]);
			AttachPoint(m_ReferencePoints[2], m_ReferencePoints[3]);
			AttachPoint(m_ReferencePoints[3], m_ReferencePoints[1]);
		}
	}

	/// <summary>
	/// Create reference points in a rectangular formation with each point connected to the central
	/// point and to its neighbors
	/// </summary>
	void CreateRigidBodiesRectangle(Bounds spriteBounds)
	{
		float width = spriteBounds.size.x * m_SoftBodyScale.x * m_SpriteScale.x;
		float height = spriteBounds.size.y * m_SoftBodyScale.y * m_SpriteScale.y;
        float radius = spriteBounds.size.y * m_SphereRadius * m_SpriteScale.y * m_Transform.localScale.y;
		float offsetFactor = 0.5f - m_SphereRadius;

		m_CentralPoint = AddReferencePoint(m_CentralBodyOffset, radius, m_LockRotation);

		ReferencePoint bottomLeftPoint = AddReferencePoint(new Vector2(-width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset, radius, true);
		AttachPoint(bottomLeftPoint, m_CentralPoint);

        ReferencePoint bottomRightPoint = AddReferencePoint(new Vector2(width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset, radius, true);
		AttachPoint(bottomRightPoint, m_CentralPoint);

        ReferencePoint topRightPoint = AddReferencePoint(new Vector2(width * offsetFactor, height * offsetFactor) + m_SoftBodyOffset, radius, true);
		AttachPoint(topRightPoint, m_CentralPoint);

        ReferencePoint topLeftPoint = AddReferencePoint(new Vector2(-width * offsetFactor, height * offsetFactor) + m_SoftBodyOffset, radius, true);
		AttachPoint(topLeftPoint, m_CentralPoint);

		if(m_AttachNeighbors)
		{
			AttachPoint(m_ReferencePoints[1], m_ReferencePoints[2]);
			AttachPoint(m_ReferencePoints[2], m_ReferencePoints[3]);
			AttachPoint(m_ReferencePoints[3], m_ReferencePoints[4]);
			AttachPoint(m_ReferencePoints[4], m_ReferencePoints[1]);
		}
	}

	/// <summary>
	/// Create reference points in a diamond grid formation around the central body. Each point is linked to
	/// its neighbors
	/// </summary>
	void CreateRigidBodiesGrid(Bounds spriteBounds)
	{
		float radius = spriteBounds.size.x * m_SphereRadius * m_SpriteScale.x;
		float width = (spriteBounds.size.x * m_SoftBodyScale.x * m_SpriteScale.x) - (m_SphereRadius * 4);
		float height = (spriteBounds.size.y * m_SoftBodyScale.y * m_SpriteScale.y) - (m_SphereRadius * 4);

		int columns = m_GridColumns;
		int rows = m_GridRows;

		columns = Mathf.Max(1, columns);
		rows = Mathf.Max(1, rows);

		for(int y = 0; y < rows; y++)
		{
			for(int x = 0; x < columns; x++)
			{ 
				if(y % 2 != 0 && x == columns - 1)
				{
					Rigidbody2D dummyBody = null;
					ReferencePoint dummyPoint = new ReferencePoint(dummyBody);
					m_ReferencePoints.Add(dummyPoint);
				}
				else
				{
					Vector2 position;
					position.x = (-width * 0.5f) + ((width/(float)(columns - 1)) * x);
					
					if(y % 2 != 0)
					{
						position.x += ((width/(float)(columns - 1)) * 0.5f);
					}
					
					position.y = (-height * 0.5f) + ((height/(float)(rows - 1)) * y);
                    position += m_SoftBodyOffset;
					ReferencePoint refPoint = AddReferencePoint(position, radius, true);

					if(x == columns/2 && y == rows/2)
					{
						m_CentralPoint = refPoint;
					}
				}
			}
		}

		for(int y = 0; y < rows - 1; y++)
		{
			for(int x = 0; x < columns; x++)
			{ 
				int thisPoint = (y * columns) + x;
				int nextPoint = ((y + 1) * columns) + x;

				if(!m_ReferencePoints[thisPoint].IsDummy && !m_ReferencePoints[nextPoint].IsDummy)
				{
					AttachPoint(m_ReferencePoints[thisPoint], m_ReferencePoints[nextPoint]);
				}
			}
		}

		for(int y = 0; y < rows - 1; y++)
		{
			for(int x = 1; x < columns - 1; x++)
			{ 
				if(y % 2 == 0)
				{
					int thisPoint = (y * columns) + x;
					int nextPoint = ((y + 1) * columns) + (x - 1);

					if(!m_ReferencePoints[thisPoint].IsDummy && !m_ReferencePoints[nextPoint].IsDummy)
					{
						AttachPoint(m_ReferencePoints[thisPoint], m_ReferencePoints[nextPoint]);
					}
				}
			}
		}

		for(int y = 0; y < rows - 1; y++)
		{
			for(int x = 0; x < columns - 1; x++)
			{ 
				if(y % 2 != 0)
				{
					int thisPoint = (y * columns) + x;
					int nextPoint = ((y + 1) * columns) + (x + 1);

					if(!m_ReferencePoints[thisPoint].IsDummy && !m_ReferencePoints[nextPoint].IsDummy)
					{
						AttachPoint(m_ReferencePoints[thisPoint], m_ReferencePoints[nextPoint]);
					}
				}
			}
		}

		for(int y = 0; y < rows - 1; y++)
		{
			if(y % 2 == 0)
			{
				int x = columns - 1;
				int thisPoint = (y * columns) + x;
				int nextPoint = ((y + 1) * columns) + (x - 1);

				if(!m_ReferencePoints[thisPoint].IsDummy && !m_ReferencePoints[nextPoint].IsDummy)
				{
					AttachPoint(m_ReferencePoints[thisPoint], m_ReferencePoints[nextPoint]);
				}
			}
		}
	}

	/// <summary>
	/// Creates the rigid bodies in a free configuration based around a central point
	/// </summary>
	/// <param name="">.</param>
	void CreateRigidBodiesFree(Bounds spriteBounds)
	{
        m_CentralBodyOffset = m_FreeModeBodyPositions[0];
		m_CentralPoint = AddReferencePoint(m_FreeModeBodyPositions[0], m_FreeModeBodyRadii[0], m_LockRotation);
		m_CentralPoint.SetKinematic(m_FreeModeBodyKinematic[0]);

		for(int loop = 1; loop < m_FreeModeBodyPositions.Count; loop++)
		{
			ReferencePoint referencePoint = AddReferencePoint(m_FreeModeBodyPositions[loop], m_FreeModeBodyRadii[loop], true);
			AttachPoint(referencePoint, m_CentralPoint);
			referencePoint.SetKinematic(m_FreeModeBodyKinematic[loop]);
		}

		if(m_AttachNeighbors)
		{
			for(int loop = 2; loop < m_ReferencePoints.Count; loop++)
			{
				AttachPoint(m_ReferencePoints[loop], m_ReferencePoints[loop - 1]);
			}
			
			AttachPoint(m_ReferencePoints[m_ReferencePoints.Count - 1], m_ReferencePoints[1]);
		}
	}

	/// <summary>
	/// Update the sprite after changing the rotation lock flag
	/// </summary>
	public void UpdateRotationLock()
	{
		if(m_CentralPoint != null)
		{
			if(m_2DMode)
			{
				Rigidbody2D centreRigidBody = m_CentralPoint.Body2D;

#if !UNITY_5
				centreRigidBody.fixedAngle = m_LockRotation;
#else
				RigidbodyConstraints2D constraints = centreRigidBody.constraints;
				
				if(m_LockRotation)
				{
					constraints |= RigidbodyConstraints2D.FreezeRotation;
				}
				else
				{
					constraints &= ~RigidbodyConstraints2D.FreezeRotation;
				}
				
				centreRigidBody.constraints = constraints;
#endif

				centreRigidBody.isKinematic = m_CentralBodyKinematic;
			}
			else
			{
				Rigidbody centreRigidBody = m_CentralPoint.Body3D;
				
				// Fix the body to the 2D plane
				RigidbodyConstraints constraints = centreRigidBody.constraints;

				if(m_LockRotation)
				{
					constraints |= RigidbodyConstraints.FreezeRotationZ;
				}
				else
				{
					constraints &= ~RigidbodyConstraints.FreezeRotationZ;
				}
				
				centreRigidBody.constraints = constraints;
				centreRigidBody.isKinematic = m_CentralBodyKinematic;
			}
		}
	}

	/// <summary>
	/// Add a reference point - essentially just a rigid body + circle collider - at the given
	/// position and with the given properties
	/// </summary>
	ReferencePoint AddReferencePoint(Vector3 position, float radius, bool lockRotation)
	{
		position = Quaternion.Euler(0, 0, m_SoftBodyRotation) * position;
		GameObject referencePointObject = new GameObject();
		referencePointObject.name = this.name + " Ref Point " + m_ReferencePoints.Count.ToString();
		referencePointObject.transform.parent = m_ReferencePointParent.transform;
		referencePointObject.transform.position = m_Transform.TransformPoint(position);
		referencePointObject.layer = gameObject.layer;
		referencePointObject.tag = gameObject.tag;

		JellySpriteReferencePoint refPointBehaviour = referencePointObject.AddComponent<JellySpriteReferencePoint>();
		refPointBehaviour.ParentJellySprite = this.gameObject;
		refPointBehaviour.Index = m_ReferencePoints.Count;

		ReferencePoint referencePoint = null;

		if(m_2DMode)
		{
			CircleCollider2D circleCollider = referencePointObject.AddComponent<CircleCollider2D>();
			circleCollider.radius = radius;
			circleCollider.sharedMaterial = m_PhysicsMaterial2D;

			Rigidbody2D newRigidBody = referencePointObject.AddComponent<Rigidbody2D>();

#if !UNITY_5
            newRigidBody.fixedAngle = lockRotation;
#else
            if(lockRotation)
            {
                newRigidBody.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }
#endif

			referencePoint = new ReferencePoint(newRigidBody);
		}
		else
		{
			SphereCollider circleCollider = referencePointObject.AddComponent<SphereCollider>();
			circleCollider.radius = radius;
			circleCollider.sharedMaterial = m_PhysicsMaterial;
			
			Rigidbody newRigidBody = referencePointObject.AddComponent<Rigidbody>();

			// Fix the body to the 2D plane
			RigidbodyConstraints constraints = newRigidBody.constraints;
			constraints |= RigidbodyConstraints.FreezePositionZ;
			constraints |= RigidbodyConstraints.FreezeRotationX;
			constraints |= RigidbodyConstraints.FreezeRotationY;

			// Prevent rotation unless desired
			constraints |= RigidbodyConstraints.FreezeRotationX;
			constraints |= RigidbodyConstraints.FreezeRotationY;
			
			if(lockRotation)
			{
				constraints |= RigidbodyConstraints.FreezeRotationZ;
			}

			newRigidBody.constraints = constraints;

			referencePoint = new ReferencePoint(newRigidBody);
		}

		m_ReferencePoints.Add(referencePoint);
		return referencePoint;
	}

	/// <summary>
	/// Attach two reference points together using a spring joint
	/// </summary>
	void AttachPoint(ReferencePoint point1, ReferencePoint point2)
	{
		if(m_2DMode)
		{
			SpringJoint2D joint = point1.Body2D.gameObject.AddComponent<SpringJoint2D>();
			joint.connectedBody = point2.Body2D;
			joint.connectedAnchor = point1.Body2D.transform.localPosition - point2.Body2D.transform.localPosition;
			joint.distance = 0.0f;

#if UNITY_5
			joint.enableCollision = m_CollideConnected;
#else
			joint.collideConnected = m_CollideConnected;
#endif

			joint.frequency = m_Stiffness;
			joint.dampingRatio = m_DampingRatio;            
		}
		else
		{
			SpringJoint joint = point1.Body3D.gameObject.AddComponent<SpringJoint>();
			joint.connectedBody = point2.Body3D;
			joint.connectedAnchor = point1.Body3D.transform.localPosition - point2.Body3D.transform.localPosition;
			joint.minDistance = 0.0f;
			joint.maxDistance = 0.0f;
            joint.enableCollision = m_CollideConnected;            

			joint.spring = m_Stiffness;
			joint.damper = m_DampingRatio;
            joint.autoConfigureConnectedAnchor = false;
		}
	}

	/// <summary>
	/// Each vertex takes its position from the movement of the reference points, with closer reference
	/// points contributing more to the final position than those far away. We can pre-calculate these weighting
	/// values as they remain constant.
	/// </summary>
	public void CalculateWeightingValues()
	{
        float inverseScaleX = 1.0f / m_Transform.localScale.x;
        float inverseScaleY = 1.0f / m_Transform.localScale.y;

		if(m_ReferencePoints != null)
		{
			m_ReferencePointWeightings = new float[m_Vertices.Length, m_ReferencePoints.Count];
			
			for(int vertexIndex = 0; vertexIndex < m_Vertices.Length; vertexIndex++)
			{
				float distanceSum = 0.0f;
				
				for(int referencePointIndex = 0; referencePointIndex < m_ReferencePoints.Count; referencePointIndex++)
				{
					if(!m_ReferencePoints[referencePointIndex].IsDummy)
					{
                        Vector3 offset = m_ReferencePoints[referencePointIndex].InitialOffset;
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
					}
				}                
				
				for(int referencePointIndex = 0; referencePointIndex < m_ReferencePoints.Count; referencePointIndex++)
				{
					if(!m_ReferencePoints[referencePointIndex].IsDummy)
					{
                        Vector3 offset = m_ReferencePoints[referencePointIndex].InitialOffset;
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
					}
				}
			}
		}

		if(m_AttachPoints != null && m_ReferencePoints != null)
		{
			m_AttachPointWeightings = new float[m_AttachPoints.Length, m_ReferencePoints.Count];

			for(int attachPointIndex = 0; attachPointIndex < m_AttachPoints.Length; attachPointIndex++)
			{
				float distanceSum = 0.0f;
				
				for(int referencePointIndex = 0; referencePointIndex < m_ReferencePoints.Count; referencePointIndex++)
				{
					if(!m_ReferencePoints[referencePointIndex].IsDummy)
					{
                        Vector3 offset = m_ReferencePoints[referencePointIndex].InitialOffset;
                        offset.x = offset.x * inverseScaleX;
                        offset.y = offset.y * inverseScaleY;
                        float distance = Vector2.Distance(offset, m_AttachPoints[attachPointIndex].localPosition);
						distance = Mathf.Pow(distance, m_DistanceExponent);
						float invDistance = float.MaxValue;

						if(distance > 0.0f)
						{
							invDistance = 1.0f/distance;
						}

						distanceSum += invDistance;
					}
				}
				
				for(int referencePointIndex = 0; referencePointIndex < m_ReferencePoints.Count; referencePointIndex++)
				{
					if(!m_ReferencePoints[referencePointIndex].IsDummy)
					{
                        Vector3 offset = m_ReferencePoints[referencePointIndex].InitialOffset;
                        offset.x = offset.x * inverseScaleX;
                        offset.y = offset.y * inverseScaleY;
                        float distance = Vector2.Distance(offset, m_AttachPoints[attachPointIndex].localPosition);
						distance = Mathf.Pow(distance, m_DistanceExponent);
						float invDistance = float.MaxValue;

						if(distance > 0.0f)
						{
							invDistance = 1.0f/distance;
						}

						m_AttachPointWeightings[attachPointIndex, referencePointIndex] = invDistance/distanceSum;
					}
				}
			}
		}
	}

	/// <summary>
	/// Disable reference points from colliding with one another
	/// </summary>
	void SetupCollisions()
	{
		if(m_ReferencePoints != null)
		{
			for(int referencePointIndex = 0; referencePointIndex < m_ReferencePoints.Count; referencePointIndex++)
			{
				for(int comparisonPointIndex = 0; comparisonPointIndex < m_ReferencePoints.Count; comparisonPointIndex++)
				{
					if(!m_ReferencePoints[referencePointIndex].IsDummy && !m_ReferencePoints[comparisonPointIndex].IsDummy)
					{
						if(m_2DMode)
						{
							if(referencePointIndex != comparisonPointIndex)
							{
#if UNITY_4_3
								// No support for 2D IgnoreCollision in Unity < 4.5
								if(!Physics2D.GetIgnoreLayerCollision(this.gameObject.layer, this.gameObject.layer))
								{
									Debug.LogError("Layer '" + LayerMask.LayerToName(this.gameObject.layer) + "' is set to collide with itself - soft body physics will not work as intended. Please disable collisions between this layer and itself (Edit->Project Settings->Physics 2D)");
									return;
								}
#else
								Physics2D.IgnoreCollision(m_ReferencePoints[referencePointIndex].Collider2D, m_ReferencePoints[comparisonPointIndex].Collider2D);
#endif
							}

						}
						else
						{
							if(referencePointIndex != comparisonPointIndex)
							{
								Physics.IgnoreCollision(m_ReferencePoints[referencePointIndex].Collider, m_ReferencePoints[comparisonPointIndex].Collider);
							}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Initialise the render material
	/// </summary>
	protected abstract void InitMaterial();

	/// <summary>
	/// Check if the source sprite is rotated
	/// </summary>
	protected abstract bool IsSourceSpriteRotated();

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

				if(m_FlipX)
				{
					uv.x = 1.0f - uv.x;
				}

				if(m_FlipY)
				{
					uv.y = 1.0f - uv.y;
				}

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

	/// <summary>
	/// Called if you need to reinitialise the material at runtime (eg. animating the 
	/// sprite's texture
	/// </summary>
	public void ReInitMaterial()
	{
		InitMaterial();
	}

	/// <summary>
	/// Updates the texture coords.
	/// </summary>
	public void UpdateTextureCoords()
	{		
		if(m_SpriteMesh)
		{
			Bounds spriteBounds = GetSpriteBounds();
			float width = spriteBounds.size.x * m_SpriteScale.x;
			float height = spriteBounds.size.y * m_SpriteScale.y;
			
			// Work out how many nodes we need in each direction
			float nodeDistance = Mathf.Min(width, height)/m_VertexDensity;
			int vertexGridWidth = Mathf.CeilToInt(width/nodeDistance);
			int vertexGridHeight = Mathf.CeilToInt(height/nodeDistance);

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
					
					if(m_FlipX)
					{
						uv.x = 1.0f - uv.x;
					}
					
					if(m_FlipY)
					{
						uv.y = 1.0f - uv.y;
					}

					if(rotated)
					{
						float temp = uv.x;
						uv.x = 1.0f;
						uv.y = temp;
					}
					
					m_TexCoords[vertexIndex] = uv;
				}
			}

			Vector2 minTextureCoords;
			Vector2 maxTextureCoords;
			
			GetMinMaxTextureRect(out minTextureCoords, out maxTextureCoords);
			FixupTextureCoordinates(minTextureCoords, maxTextureCoords);

			m_SpriteMesh.uv = m_TexCoords;
		}
	}

	/// <summary>
	/// Flip the sprite horizontally
	/// </summary>
	public void SetFlipHorizontal(bool flipHorizontal)
	{
		if(flipHorizontal != m_FlipX)
		{
			m_FlipX = flipHorizontal;
			UpdateTextureCoords();

			for(int loop = 0; loop < m_AttachPoints.Length; loop++)
			{
				Vector3 offset = m_InitialAttachPointPositions[loop];
				Vector3 rotation = m_AttachPoints[loop].localEulerAngles;
				offset.x *= -1.0f;
				rotation.y = m_FlipX ? 180.0f : 0.0f;
				m_InitialAttachPointPositions[loop] = offset;
				m_AttachPoints[loop].localEulerAngles = rotation;
			}
		}
	}

	/// <summary>
	/// Flip the sprite vertically
	/// </summary>
	public void SetFlipVertical(bool flipVertical)
	{
		if(flipVertical != m_FlipY)
		{
			m_FlipY = flipVertical;
			UpdateTextureCoords();

			for(int loop = 0; loop < m_AttachPoints.Length; loop++)
			{
				Vector3 offset = m_InitialAttachPointPositions[loop];
				Vector3 rotation = m_AttachPoints[loop].localEulerAngles;
				offset.y *= -1.0f;
				rotation.x = m_FlipY ? 180.0f : 0.0f;
				m_InitialAttachPointPositions[loop] = offset;
				m_AttachPoints[loop].localEulerAngles = rotation;
			}
		}
	}
	
	/// <summary>
	/// Sets the position of the Jelly Sprite
	/// </summary>
	public void SetPosition(Vector3 position, bool resetVelocity)		
	{		
		Vector3 offset = position - CentralPoint.transform.position;

		foreach(JellySprite.ReferencePoint referencePoint in ReferencePoints)		
		{			
			if(!referencePoint.IsDummy)			
			{				
				referencePoint.transform.position = referencePoint.transform.position + offset;

				if(resetVelocity)					
				{					
					if(referencePoint.Body2D)						
					{						
						referencePoint.Body2D.angularVelocity = 0.0f;					
						referencePoint.Body2D.velocity = Vector2.zero;						
					}					
					else if(referencePoint.Body3D)						
					{						
						referencePoint.Body3D.angularVelocity = Vector3.zero;
						referencePoint.Body3D.velocity = Vector3.zero;
					}
				}
			}
		}
	}

	/// <summary>
	/// Reset the jelly sprite bodies back to their original offsets, and places the Jelly Sprite at
	/// the given positon/rotation
	/// </summary>
	public void Reset(Vector3 position, Vector3 rotation)
	{
		m_CentralPoint.transform.position = position;
		m_CentralPoint.transform.eulerAngles = rotation;
		
		for(int referencePointIndex = 0; referencePointIndex < m_ReferencePoints.Count; referencePointIndex++)                
		{                
			if(!m_ReferencePoints[referencePointIndex].IsDummy && m_ReferencePoints[referencePointIndex] != m_CentralPoint)                    
			{                    
				ReferencePoint referencePoint = m_ReferencePoints[referencePointIndex];
				referencePoint.transform.localPosition = m_CentralPoint.transform.TransformPoint(referencePoint.InitialOffset);
				
				if(referencePoint.Body2D)                        
				{                        
					referencePoint.Body2D.angularVelocity = 0.0f;                    
					referencePoint.Body2D.velocity = Vector2.zero;                        
				}                    
				else if(referencePoint.Body3D)                        
				{                        
					referencePoint.Body3D.angularVelocity = Vector3.zero;
					referencePoint.Body3D.velocity = Vector3.zero;
				}
			}
		}
		
		UpdateMesh();
		UpdateAttachPoints();
	}



	/// <summary>
	/// Sets whether or not the Jelly Sprite is kinematic
	/// </summary>
	public void SetKinematic(bool isKinematic, bool centralPointOnly)		
	{		
		foreach(JellySprite.ReferencePoint referencePoint in ReferencePoints)		
		{			
			if(!referencePoint.IsDummy)			
			{				
				if(referencePoint == m_CentralPoint || !centralPointOnly)
				{
					if(referencePoint.Body2D)						
					{						
						referencePoint.Body2D.isKinematic = isKinematic;
					}					
					else if(referencePoint.Body3D)						
					{						
						referencePoint.Body3D.isKinematic = isKinematic;
					}
				}
			}
		}
	}

	/// <summary>
	/// Rotate the whole jelly sprite by the given angle
	/// </summary>
	public void Rotate(float angleChange)
	{
		Vector3 eulerAngleChange = new Vector3(0.0f, 0.0f, angleChange);

		// Rotate the central body by the required amount	
		CentralPoint.transform.localEulerAngles = CentralPoint.transform.localEulerAngles + eulerAngleChange;
		
		// Now go through all the reference points and orbit them around the central body by the required amount
		foreach(ReferencePoint referencePoint in ReferencePoints)			
		{			
			if(!referencePoint.IsDummy)			
			{				
				Vector3 referencePointPosition = referencePoint.transform.position;				
				Vector3 centralPointPosition = m_Transform.position;				
				referencePoint.transform.position = centralPointPosition + (Quaternion.Euler(eulerAngleChange) * (referencePointPosition - centralPointPosition));				
			}			
		}
	}

	/// <summary>
	/// Check if the Jelly Sprite is touching the given layer. You can specify how many physics bodies need to be touching for the
	/// whole Jelly Sprite to be classes as grounded
	/// </summary>
	public bool IsGrounded(LayerMask groundLayer, int minGroundedBodies)
	{
		int numGroundedBodies = 0;
		
		foreach(JellySprite.ReferencePoint referencePoint in ReferencePoints)
		{
			if(!referencePoint.IsDummy)
			{
				if(referencePoint.Collider)
				{
					SphereCollider sphereCollider = referencePoint.Collider;
						
					if(Physics.CheckSphere(sphereCollider.bounds.center + new Vector3(0, -sphereCollider.radius * 0.1f, 0), sphereCollider.radius, groundLayer))
					{
						numGroundedBodies++;
						
						if(numGroundedBodies >= minGroundedBodies)
						{
							return true;
						}
					}
				}
				else if(referencePoint.Collider2D)
				{		
					CircleCollider2D circleCollider = referencePoint.Collider2D;
					Vector2 bodyPosition = referencePoint.transform.position;

					if(Physics2D.OverlapCircle(bodyPosition + new Vector2(0, -circleCollider.radius * 0.1f), circleCollider.radius, groundLayer))
					{
						numGroundedBodies++;
						
						if(numGroundedBodies >= minGroundedBodies)
						{
							return true;
						}
					}
				}
			}
		}
		
		return false;
	}

	protected abstract void GetMinMaxTextureRect(out Vector2 min, out Vector2 max);

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
	/// Resizes the attach point array
	/// </summary>
	public void ResizeAttachPoints()
	{
		Transform[] oldAttachPoints = new Transform[m_AttachPoints.Length];
		bool[] oldIsAttachPointJellySprite = new bool[m_AttachPoints.Length];
		Vector3[] oldInitialAttachPointPositions = new Vector3[m_AttachPoints.Length];

		m_AttachPoints.CopyTo(oldAttachPoints, 0);
		m_IsAttachPointJellySprite.CopyTo(oldIsAttachPointJellySprite, 0);
		m_InitialAttachPointPositions.CopyTo(oldInitialAttachPointPositions, 0);

		m_AttachPoints = new Transform[m_NumAttachPoints];
		m_IsAttachPointJellySprite = new bool[m_NumAttachPoints];
		m_InitialAttachPointPositions = new Vector3[m_NumAttachPoints];

		for(int loop = 0; loop < m_NumAttachPoints && loop < oldAttachPoints.Length; loop++)
		{
			m_AttachPoints[loop] = oldAttachPoints[loop];
			m_IsAttachPointJellySprite[loop] = oldIsAttachPointJellySprite[loop];
			m_InitialAttachPointPositions[loop] = oldInitialAttachPointPositions[loop];
		}

		if(m_AttachPointWeightings != null)
		{
			float[,] oldAttachPointWeightings = new float[m_AttachPointWeightings.GetLength(0),m_AttachPointWeightings.GetLength(1)];

			for(int x = 0; x < m_AttachPointWeightings.GetLength(0); x++)
			{
				for(int y = 0; y < m_AttachPointWeightings.GetLength(1); y++)
				{
					oldAttachPointWeightings[x, y] = m_AttachPointWeightings[x, y];
				}
			}

			m_AttachPointWeightings = new float[m_AttachPoints.Length, m_ReferencePoints.Count];

			for(int x = 0; x < oldAttachPointWeightings.GetLength(0); x++)
			{
				for(int y = 0; y < oldAttachPointWeightings.GetLength(1); y++)
				{
                    if(x < m_AttachPoints.Length)
                    {
					    m_AttachPointWeightings[x, y] = oldAttachPointWeightings[x, y];
                    }
				}
			}
		}
	}

	/// <summary>
	/// Called when free mode is selected for the first time - copies all existing points to 
	/// the free mode configuration
	/// </summary>
	public void OnCopyToFreeModeSelected()
	{
		if(IsSpriteValid())
		{
			m_FreeModeBodyPositions.Clear();
			m_FreeModeBodyRadii.Clear();
			m_FreeModeBodyKinematic.Clear();

			Bounds spriteBounds = GetSpriteBounds();
			float width = spriteBounds.size.x * m_SoftBodyScale.x * m_SpriteScale.x;
			float height = spriteBounds.size.y * m_SoftBodyScale.y * m_SpriteScale.y;
							
			switch(m_Style)
			{
				case PhysicsStyle.Circle:
				{
					width = spriteBounds.size.x * m_SpriteScale.x;
					height = spriteBounds.size.y * m_SpriteScale.x;
					
					int numPoints = m_RadiusPoints;
					float radius = width * 0.5f;
                    float sphereRadius = m_SphereRadius * transform.localScale.x;

                    AddFreeModeBodyDefinition(m_CentralBodyOffset, width * sphereRadius, m_CentralBodyKinematic);

					for(int loop = 0; loop < numPoints; loop++)
					{
						float angle = ((Mathf.PI * 2)/numPoints) * loop;
						Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
						offset *= radius;
						offset.x *= m_SoftBodyScale.x;
						offset.y *= m_SoftBodyScale.y;
                        Vector3 bodyPosition = offset * (1.0f - ((sphereRadius * width) / (transform.localScale.x * offset.magnitude))) + m_SoftBodyOffset;
                        AddFreeModeBodyDefinition(bodyPosition, width * sphereRadius, false);
					}
				}
				break;
					
				case PhysicsStyle.Triangle:
				{
                    float radius = spriteBounds.size.y * m_SphereRadius * m_SpriteScale.y * transform.localScale.y;
					float offsetFactor = 0.5f - m_SphereRadius;
					AddFreeModeBodyDefinition(m_CentralBodyOffset, radius, m_CentralBodyKinematic);
					AddFreeModeBodyDefinition(new Vector2(-width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset, radius, false);
                    AddFreeModeBodyDefinition(new Vector2(width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset, radius, false);
                    AddFreeModeBodyDefinition(new Vector2(0.0f, height * offsetFactor) + m_SoftBodyOffset, radius, false);
				}
				break;
					
				case PhysicsStyle.Rectangle:
				{
                    float radius = spriteBounds.size.y * m_SphereRadius * m_SpriteScale.y * transform.localScale.y;
					float offsetFactor = 0.5f - m_SphereRadius;
					AddFreeModeBodyDefinition(m_CentralBodyOffset, radius, m_CentralBodyKinematic);
                    AddFreeModeBodyDefinition(new Vector2(-width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset, radius, false);
                    AddFreeModeBodyDefinition(new Vector2(width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset, radius, false);
                    AddFreeModeBodyDefinition(new Vector2(-width * offsetFactor, height * offsetFactor) + m_SoftBodyOffset, radius, false);
					AddFreeModeBodyDefinition(new Vector2(width * offsetFactor, height * offsetFactor) + m_SoftBodyOffset, radius, false);
				}
				break;		

				case PhysicsStyle.Free:
				break;
					
				case PhysicsStyle.Grid:
				{
					width -= (m_SphereRadius * 4);
					height -= (m_SphereRadius * 4);
										
					float radius = spriteBounds.size.x * m_SphereRadius * m_SpriteScale.x * m_Transform.localScale.x;
					AddFreeModeBodyDefinition(Vector2.zero, radius, m_CentralBodyKinematic);
					int columns = m_GridColumns;
					int rows = m_GridRows;
					
					for(int y = 0; y < rows; y++)
					{
						for(int x = 0; x < (y % 2 == 0? columns : columns - 1); x++)
						{
							Vector2 position;
							position.x = (-width * 0.5f) + ((width/(float)(columns - 1)) * x);
							
							if(y % 2 != 0)
							{
								position.x += width/(float)(columns - 1) * 0.5f;
							}
							
							position.y = (-height * 0.5f) + ((height/(float)(rows - 1)) * y);
                            position += m_SoftBodyOffset;
							AddFreeModeBodyDefinition(position, radius, false);								
						}
					}
				}
				break;
			}

			m_Style = PhysicsStyle.Free;
            m_SoftBodyOffset = Vector3.zero;
            m_SoftBodyRotation = 0.0f;
            m_SoftBodyScale = Vector3.one;
		}
	}

	/// <summary>
	/// First time setup of the mesh
	/// </summary>
	void InitMesh()
	{
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		m_SpriteMesh = new Mesh();
		m_SpriteMesh.name = "JellySprite Mesh";
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

	/// <summary>
	/// Update the vertex positions of the mesh
	/// </summary>
	void UpdateMesh()
	{
		// For each vertex, look at the offset values of each reference point and apply the same offset
		// (scaled by the weighting value) to the vertex's position
		if(Application.isPlaying)
		{
			// Calculate reference point offsets
			bool haveAnyPointsMoved = false;

			for(int referencePointIndex = 0; referencePointIndex < m_ReferencePoints.Count; referencePointIndex++)				
			{				
				if(!m_ReferencePoints[referencePointIndex].IsDummy && m_ReferencePoints[referencePointIndex] != m_CentralPoint)					
				{					
					ReferencePoint referencePoint = m_ReferencePoints[referencePointIndex];					
					Vector3 offset = m_CentralPoint.transform.InverseTransformPoint(referencePoint.transform.position);
					offset -= referencePoint.InitialOffset;

					if(haveAnyPointsMoved || m_ReferencePointOffsets[referencePointIndex] != offset)
					{
						m_ReferencePointOffsets[referencePointIndex] = offset;
						haveAnyPointsMoved = true;
					}
				}
				else
				{					
					m_ReferencePointOffsets[referencePointIndex] = Vector3.zero;					
				}
			}

			if(!haveAnyPointsMoved)
			{
				return;
			}

			int numVertices = m_Vertices.Length;
			int numReferencePoints = m_ReferencePoints.Count;
			int centralPointIndex = GetCentralPointIndex();

			for(int vertexIndex = 0; vertexIndex < numVertices; vertexIndex++)
			{
				Vector3 totalOffset = Vector3.zero;
				
				for(int referencePointIndex = 0; referencePointIndex < numReferencePoints; referencePointIndex++)
				{
					if(referencePointIndex != centralPointIndex && !m_ReferencePoints[referencePointIndex].IsDummy)
					{
						totalOffset += m_ReferencePointOffsets[referencePointIndex] * m_ReferencePointWeightings[vertexIndex, referencePointIndex];
					}
				}

                m_Vertices[vertexIndex] = m_InitialVertexPositions[vertexIndex] + totalOffset + new Vector3(m_CentralBodyOffset.x, m_CentralBodyOffset.y, 0);
			}
			
			// Update the mesh
			m_SpriteMesh.vertices = m_Vertices;
			m_SpriteMesh.RecalculateBounds();
			m_SpriteMesh.RecalculateNormals();
		}
	}

	/// <summary>
	/// Gets the index of the central point.
	/// </summary>
	int GetCentralPointIndex()
	{
		int numReferencePoints = m_ReferencePoints.Count;
		
		for(int referencePointIndex = 0; referencePointIndex < numReferencePoints; referencePointIndex++)
		{
			if(m_ReferencePoints[referencePointIndex] == m_CentralPoint)
			{
				return referencePointIndex;
			}
		}
		
		return -1;
	}

	/// <summary>
	/// Update the attach point positions
	/// </summary>
	void UpdateAttachPoints()
	{
		// For each vertex, look at the offset values of each reference point and apply the same offset
		// (scaled by the weighting value) to the vertex's position
		if(Application.isPlaying)
		{
            Quaternion additionalBodyRotation = Quaternion.Euler(0, 0, m_SoftBodyRotation);
            Vector3 rotatedBodyOffset = additionalBodyRotation * m_CentralBodyOffset;

			int numAttachPoints = m_AttachPoints.Length;
			int numReferencePoints = m_ReferencePoints.Count;
			int centralPointIndex = GetCentralPointIndex();

			for(int attachPointIndex = 0; attachPointIndex < numAttachPoints; attachPointIndex++)
			{
				Vector3 totalOffset = Vector3.zero;
				
				for(int referencePointIndex = 0; referencePointIndex < numReferencePoints; referencePointIndex++)
				{
					if(referencePointIndex != centralPointIndex && !m_ReferencePoints[referencePointIndex].IsDummy)
					{
						ReferencePoint referencePoint = m_ReferencePoints[referencePointIndex];
						Vector3 offset = m_CentralPoint.transform.InverseTransformPoint(referencePoint.transform.position);
						offset -= referencePoint.InitialOffset;
						totalOffset += offset * m_AttachPointWeightings[attachPointIndex, referencePointIndex];
					}
				}

				// Attached Jelly Sprites need to behave slightly differently from regular objects - we set the central
				// body to be kinematic and then adjust the position of this, which allows the Jelly Sprite to track the
				// attach point position while still being able to wobble around
				if(m_IsAttachPointJellySprite[attachPointIndex])
				{
					JellySprite attachedJellySprite = m_AttachPoints[attachPointIndex].GetComponent<JellySprite>();
					attachedJellySprite.CentralPoint.transform.parent = m_Transform;
					attachedJellySprite.CentralPoint.SetKinematic(true);
                    attachedJellySprite.CentralPoint.transform.localPosition = m_InitialAttachPointPositions[attachPointIndex] + totalOffset + rotatedBodyOffset;
				}
				else
				{
                    m_AttachPoints[attachPointIndex].transform.localPosition = m_InitialAttachPointPositions[attachPointIndex] + totalOffset + rotatedBodyOffset;
				}
			}
		}
	}

	/// <summary>
	/// Add a force to every reference point
	/// </summary>
	public void AddForce(Vector2 force)
	{
		if(m_ReferencePoints != null)
		{
			foreach(ReferencePoint referencePoint in m_ReferencePoints)
			{
				if(referencePoint.Body2D)
				{
					referencePoint.Body2D.AddForce(force);
				}
				
				if(referencePoint.Body3D)
				{
					referencePoint.Body3D.AddForce(force);
				}
			}
		}
	}

	/// <summary>
	/// Add a force at a given position to every reference point
	/// </summary>
	public void AddForceAtPosition(Vector2 force, Vector2 position)
	{
		if(m_ReferencePoints != null)
		{
			foreach(ReferencePoint referencePoint in m_ReferencePoints)
			{
				if(referencePoint.Body2D)
				{
					referencePoint.Body2D.AddForceAtPosition(force, position);
				}
				
				if(referencePoint.Body3D)
				{
					referencePoint.Body3D.AddForceAtPosition(force, position);
				}
			}
		}
	}

	/// <summary>
	/// Called when the editor wants to update the visible mesh
	/// </summary>
	public void RefreshMesh()
	{
		if(IsSpriteValid())
		{
			InitVertices(GetSpriteBounds());
			InitMaterial();
			InitMesh();

			if(m_ReferencePoints != null)
			{
				CalculateInitialOffsets();
				CalculateWeightingValues();
			}

			UpdateMesh();
		}
	}

	/// <summary>
	/// Set up the mass of each rigidbody
	/// </summary>
	public void InitMass()
	{
		if(m_ReferencePoints != null)
		{
			float mass = m_Mass;

			// If the mass is being defined on a global scale, then for n rigid
			// bodies, each one has 1/n of the total mass.
			if(m_MassStyle == MassStyle.Global)
			{
				int numNonDummyReferencePoints = 0;
				
				foreach(ReferencePoint referencePoint in m_ReferencePoints)
				{
					if(!referencePoint.IsDummy)
					{
						numNonDummyReferencePoints++;
					}
				}
				
				mass /= numNonDummyReferencePoints;
			}
			
			
			foreach(ReferencePoint referencePoint in m_ReferencePoints)
			{
				if(!referencePoint.IsDummy)
				{
					if(referencePoint.Body2D)
					{
						referencePoint.Body2D.mass = mass;
						referencePoint.Body2D.gravityScale = m_GravityScale;
                        referencePoint.Body2D.angularDrag = m_AngularDrag;
                        referencePoint.Body2D.drag = m_Drag;
					}
					
					if(referencePoint.Body3D)
					{
						referencePoint.Body3D.mass = mass;
						referencePoint.Body3D.angularDrag = m_AngularDrag;
						referencePoint.Body3D.drag = m_Drag;
					}
				}
			}
		}
	}

	/// <summary>
	/// Reapply our spring/damping values to each joint
	/// </summary>
	public void UpdateJoints()
	{
		if(m_ReferencePoints != null)
		{
			foreach(ReferencePoint referencePoint in m_ReferencePoints)
			{
				if(!referencePoint.IsDummy)
				{
					if(referencePoint.Body2D != null)
					{
						SpringJoint2D[] joints = referencePoint.Body2D.gameObject.GetComponents<SpringJoint2D>();

						if(joints != null)
						{
							for(int jointIndex = 0; jointIndex < joints.Length; jointIndex++)
							{
								joints[jointIndex].frequency = m_Stiffness;
								joints[jointIndex].dampingRatio = m_DampingRatio;
							}
						}
					}
					
					if(referencePoint.Body3D != null)
					{
						SpringJoint[] joints = referencePoint.Body3D.gameObject.GetComponents<SpringJoint>();

						if(joints != null)
						{
							for(int jointIndex = 0; jointIndex < joints.Length; jointIndex++)
							{
								joints[jointIndex].spring = m_Stiffness;
								joints[jointIndex].damper = m_DampingRatio;
							}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Use this function to scale the Jelly Sprite at runtime. Scales the rigid bodies and
	/// rendered mesh by the given amount
	/// </summary>
    public void Scale(float scaleRatio, bool scaleAttachedObjects = true)
    {
        int index = 0;
        Vector3[] refPointPositions = new Vector3[m_ReferencePoints.Count];

        foreach (ReferencePoint refPoint in m_ReferencePoints)
        {
            if (refPoint.GameObject)
            {
                refPointPositions[index] = refPoint.transform.position;
            }

            index++;
        }

        m_Transform.localScale = m_Transform.localScale * scaleRatio;
        index = 0;

        foreach (ReferencePoint refPoint in m_ReferencePoints)
        {
            if (refPoint.GameObject)
            {
                if (!refPoint.IsDummy)
                {
                    if (refPoint.Body2D)
                    {
                        CircleCollider2D circleCollider = refPoint.GameObject.GetComponent<CircleCollider2D>();

                        if (circleCollider)
                        {
                            circleCollider.radius = circleCollider.radius * scaleRatio;
                        }
                    }
                    else
                    {
                        SphereCollider sphereCollider = refPoint.GameObject.GetComponent<SphereCollider>();

                        if (sphereCollider)
                        {
                            sphereCollider.radius = sphereCollider.radius * scaleRatio;
                        }
                    }
                }

                refPoint.transform.position = refPointPositions[0] + ((refPointPositions[index] - refPointPositions[0]) * scaleRatio);
                refPoint.InitialOffset *= scaleRatio;

                if (m_2DMode)
                {
                    SpringJoint2D[] springJoints = refPoint.GameObject.GetComponents<SpringJoint2D>();

                    for (int jointLoop = 0; jointLoop < springJoints.Length; jointLoop++)
                    {
                        springJoints[jointLoop].connectedAnchor = springJoints[jointLoop].connectedAnchor * scaleRatio;
                    }
                }
                else
                {
                    SpringJoint[] springJoints = refPoint.GameObject.GetComponents<SpringJoint>();

                    for (int jointLoop = 0; jointLoop < springJoints.Length; jointLoop++)
                    {
                        springJoints[jointLoop].connectedAnchor = springJoints[jointLoop].connectedAnchor * scaleRatio;
                    }
                }
            }

            index++;
        }

        if (!scaleAttachedObjects && scaleRatio > 0)
        {
            float inverseScale = 1.0f / scaleRatio;

            for (int attachPointIndex = 0; attachPointIndex < m_AttachPoints.Length; attachPointIndex++)
            {
                m_AttachPoints[attachPointIndex].localScale *= inverseScale;
            }
        }
    }

	/// <summary>
	/// Attaches a new object to the Jelly Sprite at runtime
	/// </summary>
	public void AddAttachPoint(Transform newAttachedObject)
	{
		m_NumAttachPoints++;
		ResizeAttachPoints();

		m_AttachPoints[m_NumAttachPoints - 1] = newAttachedObject;

		JellySprite attachedJellySprite = newAttachedObject.GetComponent<JellySprite>();
        Vector3 position = m_CentralPoint.transform.InverseTransformPoint(newAttachedObject.position);
        position.x /= m_Transform.localScale.x;
        position.y /= m_Transform.localScale.y;
			
		if(attachedJellySprite)
		{
			m_IsAttachPointJellySprite[m_NumAttachPoints - 1] = true;
            m_InitialAttachPointPositions[m_NumAttachPoints - 1] = position;
		}
		else
		{
			m_IsAttachPointJellySprite[m_NumAttachPoints - 1] = false;
            m_InitialAttachPointPositions[m_NumAttachPoints - 1] = position;
			newAttachedObject.parent = m_Transform;
		}

		float distanceSum = 0.0f;
		
		for(int referencePointIndex = 0; referencePointIndex < m_ReferencePoints.Count; referencePointIndex++)
		{
			if(!m_ReferencePoints[referencePointIndex].IsDummy)
			{
				float distance = Vector2.Distance(m_ReferencePoints[referencePointIndex].InitialOffset, m_AttachPoints[m_NumAttachPoints - 1].localPosition);
				distance = Mathf.Pow(distance, m_DistanceExponent);
				float invDistance = float.MaxValue;
				
				if(distance > 0.0f)
				{
					invDistance = 1.0f/distance;
				}
				
				distanceSum += invDistance;
			}
		}
		
		for(int referencePointIndex = 0; referencePointIndex < m_ReferencePoints.Count; referencePointIndex++)
		{
			if(!m_ReferencePoints[referencePointIndex].IsDummy) 
			{
				float distance = Vector2.Distance(m_ReferencePoints[referencePointIndex].InitialOffset, m_AttachPoints[m_NumAttachPoints - 1].localPosition);
				distance = Mathf.Pow(distance, m_DistanceExponent);
				float invDistance = float.MaxValue;
				
				if(distance > 0.0f)
				{
					invDistance = 1.0f/distance;
				}
				
				m_AttachPointWeightings[m_NumAttachPoints - 1, referencePointIndex] = invDistance/distanceSum;
			}
		}
	}

	/// <summary>
	/// Wake up the whole body - useful for editor controls when they update a value
	/// </summary>
	public void WakeUp()
	{
		if(m_ReferencePoints != null)
		{
			foreach(ReferencePoint referencePoint in m_ReferencePoints)
			{
				if(referencePoint.Body2D != null)
				{
					referencePoint.Body2D.WakeUp();
				}

				if(referencePoint.Body3D != null)
				{
					referencePoint.Body3D.WakeUp();
				}
			}
		}
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		if(m_ReferencePoints != null)
		{
#if UNITY_EDITOR
			// Debug draw the joints that connect each node
			if(Selection.activeGameObject == this.gameObject)
			{
				foreach(ReferencePoint referencePoint in m_ReferencePoints)
				{
					if(!referencePoint.IsDummy)
					{
						if(m_2DMode)
						{
							SpringJoint2D[] springJoints = referencePoint.Body2D.GetComponents<SpringJoint2D>();
							
							for(int jointIndex = 0; jointIndex < springJoints.Length; jointIndex++)
							{
								Debug.DrawLine(springJoints[jointIndex].transform.position, springJoints[jointIndex].connectedBody.transform.position, Color.green);
							}
						}
						else
						{
							SpringJoint[] springJoints = referencePoint.Body3D.GetComponents<SpringJoint>();
							
							for(int jointIndex = 0; jointIndex < springJoints.Length; jointIndex++)
							{
								Debug.DrawLine(springJoints[jointIndex].transform.position, springJoints[jointIndex].connectedBody.transform.position, Color.green);
							}
						}
					}
				}
			}
#endif

            Quaternion additionalBodyRotation = Quaternion.Euler(0, 0, m_SoftBodyRotation);
            Vector3 rotatedPostion = additionalBodyRotation * new Vector3(-m_CentralBodyOffset.x * m_Transform.localScale.x, -m_CentralBodyOffset.y * m_Transform.localScale.y, 0);
            m_Transform.position = m_CentralPoint.transform.TransformPoint(rotatedPostion);
			m_Transform.rotation = m_CentralPoint.transform.rotation;

			// Apply our rigid body movements to the rendered mesh
			UpdateMesh();
			UpdateAttachPoints();
		}
	}

	/// <summary>
	/// Add a position/radius pair to the free mode bodies
	/// </summary>
	void AddFreeModeBodyDefinition(Vector2 position, float radius, bool kinematic)
	{
		position = Quaternion.Euler(0, 0, m_SoftBodyRotation) * position;
		m_FreeModeBodyPositions.Add(position);
		m_FreeModeBodyRadii.Add(radius);
		m_FreeModeBodyKinematic.Add(kinematic);
	}

	/// <summary>
	/// Helper function to draw a sphere with a line connecting to it from the object's origin
	/// </summary>
	void DrawSphereWithCentreConnection(Vector3 position, float radius, bool kinematic)
	{
		position = Quaternion.Euler(0, 0, m_SoftBodyRotation) * position;
        Vector3 centralBodyPosition = Quaternion.Euler(0, 0, m_SoftBodyRotation) * m_CentralBodyOffset;

		Vector3 worldPoint = this.transform.localToWorldMatrix.MultiplyPoint(position);
        Vector3 originPoint = this.transform.localToWorldMatrix.MultiplyPoint(centralBodyPosition);

		Gizmos.color = kinematic? Color.red : Color.green;
		Gizmos.DrawWireSphere(worldPoint, radius);
		
		Gizmos.color = Color.white;
		Gizmos.DrawLine(worldPoint, originPoint);
	}

	/// <summary>
	/// Helper function to draw a sphere with a line connecting to it from the object's origin
	/// </summary>
	void DrawCentreConnection(Vector3 position, Vector3 centre)
	{
		position = Quaternion.Euler(0, 0, m_SoftBodyRotation) * position;
        Vector3 centralBodyPosition = Quaternion.Euler(0, 0, m_SoftBodyRotation) * m_CentralBodyOffset;

		Vector3 worldPoint = this.transform.localToWorldMatrix.MultiplyPoint(position);
        Vector3 originPoint = this.transform.localToWorldMatrix.MultiplyPoint(centralBodyPosition);
		
		Gizmos.color = Color.white;
		Gizmos.DrawLine(worldPoint, originPoint);
	}

	/// <summary>
	/// Helper function to draw a sphere with a line connecting to it from the object's origin
	/// </summary>
	void DrawCentreConnectionWithoutRotation(Vector3 position, Vector3 centre)
	{
		Vector3 worldPoint = this.transform.localToWorldMatrix.MultiplyPoint(position);
		Vector3 originPoint = this.transform.localToWorldMatrix.MultiplyPoint(centre);
		
		Gizmos.color = Color.white;
		Gizmos.DrawLine(worldPoint, originPoint);
	}

	/// <summary>
	/// Draw the positions of the colliders when we select objects in the hierarchy
	/// </summary>
	void OnDrawGizmosSelected () 
	{
		if(!Application.isPlaying && IsSpriteValid())
		{
			Bounds spriteBounds = GetSpriteBounds();
			float width = spriteBounds.size.x * m_SoftBodyScale.x * m_SpriteScale.x;
			float height = spriteBounds.size.y * m_SoftBodyScale.y * m_SpriteScale.y;
			
			switch(m_Style)
			{
			case PhysicsStyle.Circle:
				{
                    float sphereRadius = m_SphereRadius * transform.localScale.x;
					width = spriteBounds.size.x * m_SpriteScale.x;
					height = spriteBounds.size.y * m_SpriteScale.x;

					int numPoints = m_RadiusPoints;
					float radius = width * 0.5f;
					Vector3 prevPosition = Vector3.zero;
					Vector3 startPosition = Vector3.zero;
                    DrawSphereWithCentreConnection(m_CentralBodyOffset, width * sphereRadius, m_CentralBodyKinematic);
					
					for(int loop = 0; loop < numPoints; loop++)
					{
						float angle = ((Mathf.PI * 2)/numPoints) * loop;
						Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
						offset *= radius;
						offset.x *= m_SoftBodyScale.x;
						offset.y *= m_SoftBodyScale.y;
                        Vector3 bodyPosition = offset * (1.0f - ((sphereRadius * width) / (transform.localScale.x * offset.magnitude))) + m_SoftBodyOffset;
                        DrawSphereWithCentreConnection(bodyPosition, width * sphereRadius, false);
						
						if(m_AttachNeighbors)
						{
							if(loop == 0)
							{
								startPosition = bodyPosition;
							}
							else
							{
								DrawCentreConnection(bodyPosition, prevPosition);
							}
						}
						
						prevPosition = bodyPosition;
					}

					if(m_AttachNeighbors)
					{
						DrawCentreConnection(prevPosition, startPosition);
					}
				}
				break;
				
			case PhysicsStyle.Triangle:
				{                    
					float radius = spriteBounds.size.y * m_SphereRadius * m_SpriteScale.y * transform.localScale.y;
					float offsetFactor = 0.5f - m_SphereRadius;

                    Vector2 point1 = new Vector2(-width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset;
                    Vector3 point2 = new Vector2(width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset;
                    Vector3 point3 = new Vector2(0.0f, height * offsetFactor) + m_SoftBodyOffset;

                    DrawSphereWithCentreConnection(m_CentralBodyOffset, radius, m_CentralBodyKinematic);
					DrawSphereWithCentreConnection(point1, radius, false);
					DrawSphereWithCentreConnection(point2, radius, false);
					DrawSphereWithCentreConnection(point3, radius, false);

					if(m_AttachNeighbors)
					{
						DrawCentreConnection(point1, point2);
						DrawCentreConnection(point2, point3);
						DrawCentreConnection(point3, point1);
					}
				}
				break;
				
			case PhysicsStyle.Rectangle:
				{
					float radius = spriteBounds.size.y * m_SphereRadius * m_SpriteScale.y * transform.localScale.y;
					float offsetFactor = 0.5f - m_SphereRadius;

                    Vector2 point1 = new Vector2(-width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset;
                    Vector2 point2 = new Vector2(width * offsetFactor, -height * offsetFactor) + m_SoftBodyOffset;
                    Vector2 point3 = new Vector2(width * offsetFactor, height * offsetFactor) + m_SoftBodyOffset;
                    Vector2 point4 = new Vector2(-width * offsetFactor, height * offsetFactor) + m_SoftBodyOffset;

                    DrawSphereWithCentreConnection(m_CentralBodyOffset, radius, m_CentralBodyKinematic);
					DrawSphereWithCentreConnection(point1, radius, false);
					DrawSphereWithCentreConnection(point2, radius, false);
					DrawSphereWithCentreConnection(point3, radius, false);
					DrawSphereWithCentreConnection(point4, radius, false);

					if(m_AttachNeighbors)
					{
						DrawCentreConnection(point1, point2);
						DrawCentreConnection(point2, point3);
						DrawCentreConnection(point3, point4);
						DrawCentreConnection(point4, point1);
					}
				}
				break;

			case PhysicsStyle.Free:
				{
					if(m_FreeModeBodyPositions != null)
					{
						for(int loop = 1; loop < m_FreeModeBodyPositions.Count; loop++)
						{
							DrawCentreConnectionWithoutRotation(m_FreeModeBodyPositions[loop], m_FreeModeBodyPositions[0]);
						}
					}
				}
				break;
				
			case PhysicsStyle.Grid:
				{
					width -= (m_SphereRadius * 4);
					height -= (m_SphereRadius * 4);

                    float radius = spriteBounds.size.x * m_SphereRadius * m_SpriteScale.x * m_Transform.localScale.x;
					int columns = m_GridColumns;
					int rows = m_GridRows;
						
					for(int y = 0; y < rows; y++)
					{
						for(int x = 0; x < (y % 2 == 0? columns : columns - 1); x++)
						{
							Vector2 position;
							position.x = (-width * 0.5f) + ((width/(float)(columns - 1)) * x);

							if(y % 2 != 0)
							{
								position.x += width/(float)(columns - 1) * 0.5f;
							}

							position.y = (-height * 0.5f) + ((height/(float)(rows - 1)) * y);
                            position += m_SoftBodyOffset;

							Vector3 worldPoint = this.transform.localToWorldMatrix.MultiplyPoint(position);
							Gizmos.color = Color.green;

							if(m_CentralBodyKinematic && x == columns/2 && y == rows/2)
							{
								Gizmos.color = Color.red;
							}

							Gizmos.DrawWireSphere(worldPoint, radius);
						}
					}
				}
				break;
			}
		}
	}
}
