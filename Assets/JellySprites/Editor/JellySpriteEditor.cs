using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(JellySprite)), CanEditMultipleObjects]
public class JellySpriteEditor : Editor 
{
	public SerializedProperty m_PhysicsStyle;
	public SerializedProperty m_PhysicsMode;
	public SerializedProperty m_CircleRadiusPoints;
	public SerializedProperty m_GridRows;
	public SerializedProperty m_GridColumns;
	public SerializedProperty m_ColliderRadius;
	public SerializedProperty m_Stiffness;
	public SerializedProperty m_Mass;
	public SerializedProperty m_DampingRatio;
	public SerializedProperty m_VertexDensity;
	public SerializedProperty m_PhysicsMaterial2D;
	public SerializedProperty m_PhysicsMaterial;
	public SerializedProperty m_DistanceExponent;
	public SerializedProperty m_MassStyle;
	public SerializedProperty m_GravityScale;
	public SerializedProperty m_Drag;
	public SerializedProperty m_AngularDrag;
	public SerializedProperty m_SoftBodyScale;
	public SerializedProperty m_SoftBodyRotation;
    public SerializedProperty m_SoftBodyOffset;
    public SerializedProperty m_CentralBodyOffset;
	public SerializedProperty m_SpriteScale;
	public SerializedProperty m_AttachPoints;
	public SerializedProperty m_NumAttachPoints;
	public SerializedProperty m_FlipX;
	public SerializedProperty m_FlipY;
	public SerializedProperty m_CollideConnected;
	public SerializedProperty m_AttachNeighbors;
	public SerializedProperty m_LockRotation;
	public SerializedProperty m_CentralBodyKinematic;
    public SerializedProperty m_Color;

	bool m_InitialFlipX;
	bool m_InitialFlipY;
	bool m_InitialLockRotation;
	bool m_InitialCentralBodyKinematic;
    Color m_InitialColor;
	int m_InitialVertexDensity;
	int m_InitialMassStyle;
	int m_InitialNumAttachPoints;
	float m_InitialStiffness; 
	float m_InitialDamping;
	float m_InitialMass;
	float m_InitialDistanceExponent;
	float m_InitialGravityScale;
	float m_InitialDrag;
	float m_InitialAngularDrag;
	Vector2 m_InitialSpriteScale;

	protected virtual void OnEnable () 
	{
		// Setup the SerializedProperties
		m_PhysicsStyle = serializedObject.FindProperty("m_Style");
		m_PhysicsMode = serializedObject.FindProperty("m_2DMode");
		m_CircleRadiusPoints = serializedObject.FindProperty("m_RadiusPoints");
		m_GridRows = serializedObject.FindProperty("m_GridRows");
		m_GridColumns = serializedObject.FindProperty("m_GridColumns");
		m_ColliderRadius = serializedObject.FindProperty("m_SphereRadius");
		m_Stiffness = serializedObject.FindProperty("m_Stiffness");
		m_Mass = serializedObject.FindProperty("m_Mass");
		m_DampingRatio = serializedObject.FindProperty("m_DampingRatio");
		m_VertexDensity = serializedObject.FindProperty("m_VertexDensity");
		m_PhysicsMaterial = serializedObject.FindProperty("m_PhysicsMaterial");
		m_PhysicsMaterial2D = serializedObject.FindProperty("m_PhysicsMaterial2D");
		m_DistanceExponent = serializedObject.FindProperty("m_DistanceExponent");
		m_MassStyle = serializedObject.FindProperty("m_MassStyle");
		m_GravityScale = serializedObject.FindProperty("m_GravityScale");
		m_Drag = serializedObject.FindProperty("m_Drag");
		m_AngularDrag = serializedObject.FindProperty("m_AngularDrag");
		m_SoftBodyScale = serializedObject.FindProperty("m_SoftBodyScale");
		m_SoftBodyRotation = serializedObject.FindProperty("m_SoftBodyRotation");
        m_SoftBodyOffset = serializedObject.FindProperty("m_SoftBodyOffset");
        m_CentralBodyOffset = serializedObject.FindProperty("m_CentralBodyOffset");
		m_SpriteScale = serializedObject.FindProperty("m_SpriteScale");
		m_AttachPoints = serializedObject.FindProperty("m_AttachPoints");
		m_NumAttachPoints = serializedObject.FindProperty("m_NumAttachPoints");
		m_FlipX = serializedObject.FindProperty("m_FlipX");
		m_FlipY = serializedObject.FindProperty("m_FlipY");
		m_CollideConnected = serializedObject.FindProperty("m_CollideConnected");
		m_AttachNeighbors = serializedObject.FindProperty("m_AttachNeighbors");
		m_LockRotation = serializedObject.FindProperty("m_LockRotation");
		m_CentralBodyKinematic = serializedObject.FindProperty("m_CentralBodyKinematic");
        m_Color = serializedObject.FindProperty("m_Color");
	}

	private static System.Reflection.PropertyInfo sortingLayerNamesPropInfo = null;
	private static bool sortingLayerNamesChecked = false;
	
	private static string[] GetSortingLayerNames() 
	{
		if (sortingLayerNamesPropInfo == null && !sortingLayerNamesChecked) 
		{
			sortingLayerNamesChecked = true;
			try 
			{
				System.Type IEU = System.Type.GetType("UnityEditorInternal.InternalEditorUtility,UnityEditor");
				if (IEU != null) 
				{
					sortingLayerNamesPropInfo = IEU.GetProperty("sortingLayerNames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
				}
			}
			catch { }
			if (sortingLayerNamesPropInfo == null) 
			{
				Debug.Log("tk2dEditorUtility - Unable to get sorting layer names.");
			}
		}
		
		if (sortingLayerNamesPropInfo != null) 
		{ 
			return sortingLayerNamesPropInfo.GetValue(null, null) as string[];
		}
		else 
		{
			return new string[0];
		}
	}
	
	public static string SortingLayerNamePopup( string label, string value ) 
	{
		string[] names = GetSortingLayerNames();
		string finalValueToUse = value;		
		int hasValueInArray = System.Array.IndexOf(names, finalValueToUse);

		// If we can't find the value - set to default
		if(hasValueInArray == -1)			
		{			
			finalValueToUse = "Default";			
		}

		if (names.Length == 0) 
		{
			return EditorGUILayout.TextField(label, finalValueToUse);			
		}
		else 
		{
			int sel = 0;
			for (int i = 0; i < names.Length; ++i) 
			{
				if (names[i] == finalValueToUse) 
				{
					sel = i;
					break;
				}
			}
			sel = EditorGUILayout.Popup(label, sel, names);
			return names[sel];
		}
	}

	protected virtual void StoreInitialValues()
	{
		m_InitialVertexDensity = m_VertexDensity.intValue;
		m_InitialMassStyle = m_MassStyle.enumValueIndex;
		m_InitialStiffness = m_Stiffness.floatValue;
		m_InitialDamping = m_DampingRatio.floatValue;
		m_InitialMass = m_Mass.floatValue;
		m_InitialDistanceExponent = m_DistanceExponent.floatValue;
		m_InitialGravityScale = m_GravityScale.floatValue;
		m_InitialDrag = m_Drag.floatValue;
		m_InitialAngularDrag = m_AngularDrag.floatValue;
		m_InitialSpriteScale = m_SpriteScale.vector2Value;
		m_InitialNumAttachPoints = m_NumAttachPoints.intValue;
		m_InitialFlipX = m_FlipX.boolValue;
		m_InitialFlipY = m_FlipY.boolValue;
		m_InitialLockRotation = m_LockRotation.boolValue;
		m_InitialCentralBodyKinematic = m_CentralBodyKinematic.boolValue;
        m_InitialColor = m_Color.colorValue;
	}

	protected virtual void CheckForObjectChanges()
	{
		serializedObject.ApplyModifiedProperties();

		// Update the visible mesh if the sprite or vertex density was changed
		if(m_InitialVertexDensity != m_VertexDensity.intValue ||
		   m_InitialSpriteScale != m_SpriteScale.vector2Value)
		{
			foreach(UnityEngine.Object targetObject in targets)
			{
				JellySprite targetObjectSprite = targetObject as JellySprite;
				targetObjectSprite.RefreshMesh();
			}
		}

		// Update the springs if we altered any of their settings
		if(m_InitialStiffness != m_Stiffness.floatValue || m_InitialDamping != m_DampingRatio.floatValue)
		{
			foreach(UnityEngine.Object targetObject in targets)
			{
				JellySprite targetObjectSprite = targetObject as JellySprite;
				targetObjectSprite.UpdateJoints();
				targetObjectSprite.WakeUp();
			}
		}
		
		// Recalculate weighting values if the exponent changes
		if(m_InitialDistanceExponent != m_DistanceExponent.floatValue)
		{
			foreach(UnityEngine.Object targetObject in targets)
			{
				JellySprite targetObjectSprite = targetObject as JellySprite;
				targetObjectSprite.CalculateWeightingValues();
			}
		}
		
		// Update the mass of each body if the value changed
		if(m_InitialMassStyle != m_MassStyle.enumValueIndex ||
		   m_InitialMass != m_Mass.floatValue ||
		   m_InitialGravityScale != m_GravityScale.floatValue ||
		   m_InitialAngularDrag != m_AngularDrag.floatValue ||
		   m_InitialDrag != m_Drag.floatValue)
		{
			foreach(UnityEngine.Object targetObject in targets)
			{
				JellySprite targetObjectSprite = targetObject as JellySprite;
				targetObjectSprite.InitMass();
				targetObjectSprite.WakeUp();
			}
		}

		if(m_InitialNumAttachPoints != m_NumAttachPoints.intValue)
		{
			foreach(UnityEngine.Object targetObject in targets)
			{
				JellySprite targetObjectSprite = targetObject as JellySprite;
				targetObjectSprite.ResizeAttachPoints();
			}
		}

		if(m_InitialFlipX != m_FlipX.boolValue ||
		   m_InitialFlipY != m_FlipY.boolValue)
		{
			foreach(UnityEngine.Object targetObject in targets)
			{
				JellySprite targetObjectSprite = targetObject as JellySprite;
				targetObjectSprite.UpdateTextureCoords();
			}
		}

		if(m_InitialLockRotation != m_LockRotation.boolValue ||
		   m_InitialCentralBodyKinematic != m_CentralBodyKinematic.boolValue)
		{
			foreach(UnityEngine.Object targetObject in targets)
			{
				JellySprite targetObjectSprite = targetObject as JellySprite;
				targetObjectSprite.UpdateRotationLock();
			}
		}

        if (m_InitialColor != m_Color.colorValue)
        {
            foreach (UnityEngine.Object targetObject in targets)
            {
                JellySprite targetObjectSprite = targetObject as JellySprite;
                targetObjectSprite.GetComponent<Renderer>().sharedMaterial.color = m_Color.colorValue;
            }
        }        
	}

	protected virtual void DisplayInspectorGUI()
	{
		JellySprite targetObject = this.target as JellySprite;

		if (targetObject.GetComponent<Renderer>()!= null) 
		{
			string sortingLayerName = SortingLayerNamePopup("Sorting Layer", targetObject.GetComponent<Renderer>().sortingLayerName);
			
			if (sortingLayerName != targetObject.GetComponent<Renderer>().sortingLayerName) 
			{
				targetObject.GetComponent<Renderer>().sortingLayerName = sortingLayerName;
			}
			
			int sortingOrder = EditorGUILayout.IntField("Order In Layer", targetObject.GetComponent<Renderer>().sortingOrder);
			
			if (sortingOrder != targetObject.GetComponent<Renderer>().sortingOrder) 
			{
				targetObject.GetComponent<Renderer>().sortingOrder = sortingOrder;
			}
		}

		EditorGUILayout.PropertyField(m_FlipX, new GUIContent("Flip Horizontal"));
		EditorGUILayout.PropertyField(m_FlipY, new GUIContent("Flip Vertical"));

		if(Application.isPlaying)
		{
			GUI.enabled = false;
		}

		EditorGUILayout.PropertyField(m_SpriteScale, new GUIContent("Sprite Scale"));

		GUI.enabled = true;

		if(Application.isPlaying)
		{
			GUI.enabled = false;
		}

		GUILayout.Space(15);

		EditorGUILayout.PropertyField(m_PhysicsMode, new GUIContent("Use 2D Physics"));

		JellySprite.PhysicsStyle selectedStyle = (JellySprite.PhysicsStyle)m_PhysicsStyle.enumValueIndex;

		if(selectedStyle != JellySprite.PhysicsStyle.Grid)
		{
			EditorGUILayout.PropertyField(m_AttachNeighbors, new GUIContent("Attach Neighbors"));
		}

		int initialStyle = m_PhysicsStyle.enumValueIndex;
		EditorGUILayout.PropertyField(m_PhysicsStyle, new GUIContent("Body Configuration"));

		if(m_PhysicsStyle.enumValueIndex != initialStyle && (JellySprite.PhysicsStyle)m_PhysicsStyle.enumValueIndex == JellySprite.PhysicsStyle.Free)
		{
			m_PhysicsStyle.enumValueIndex = initialStyle;
			Debug.LogError("Please use the 'Copy Configuration to Free Mode' button to switch Jelly Sprites to Free Mode");
		}
		
		switch(selectedStyle)
		{
		case JellySprite.PhysicsStyle.Circle:
			EditorGUILayout.IntSlider(m_CircleRadiusPoints, 8, 128, new GUIContent("Num Colliders") );
			break;
		case JellySprite.PhysicsStyle.Grid:
			m_GridRows.intValue = EditorGUILayout.IntField("Grid Rows", m_GridRows.intValue);
			m_GridColumns.intValue = EditorGUILayout.IntField("Grid Columns", m_GridColumns.intValue);
			break;
		case JellySprite.PhysicsStyle.Rectangle:
			break;
		case JellySprite.PhysicsStyle.Triangle:
			break;
		case JellySprite.PhysicsStyle.Free:
			for(int i = 0; i < targetObject.m_FreeModeBodyPositions.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				Vector2 point = targetObject.m_FreeModeBodyPositions[i];
				float radius = targetObject.m_FreeModeBodyRadii[i];
				bool kinematic = targetObject.m_FreeModeBodyKinematic[i];

				Vector3 result = EditorGUILayout.Vector3Field(i == 0? "Centre" : "Body " + i .ToString(), new Vector3(point.x, point.y, radius), GUILayout.MaxWidth(1500f));
				Vector2 positionResult = new Vector2(result.x, result.y);
				bool kinematicResult = false;

				if(point != positionResult)
				{
					Undo.RecordObject(targetObject, "Adjusted Jelly Sprite Body Position");
					targetObject.m_FreeModeBodyPositions[i] = positionResult;
					SceneView.RepaintAll();
				}

				if(result.z != radius)
				{
					Undo.RecordObject(targetObject, "Adjusted Jelly Sprite Body Radius");
					targetObject.m_FreeModeBodyRadii[i] = result.z;
					SceneView.RepaintAll();
				}

				using (new FixedWidthLabel("Kinematic"))
				{
					kinematicResult = EditorGUILayout.Toggle(kinematic);
				}

				if(kinematicResult != kinematic)
				{
					Undo.RecordObject(targetObject, "Adjusted Jelly Sprite Body Kinematic Flag");
					targetObject.m_FreeModeBodyKinematic[i] = kinematicResult;
					SceneView.RepaintAll();
				}

				if(GUILayout.Button(new GUIContent("Delete", "delete this point"), EditorStyles.miniButtonRight, GUILayout.MaxWidth(100f)))
				{
					if(i == 0)
					{
						Debug.LogWarning("Cannot remove central Jelly Sprite control point!");
					}
					else
					{
						Undo.RecordObject(targetObject, "Deleted Jelly Sprite Body");
						targetObject.m_FreeModeBodyPositions.RemoveAt(i);
						targetObject.m_FreeModeBodyRadii.RemoveAt(i);
						SceneView.RepaintAll();
						i--;
					}
				}

				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(5);

			if(GUILayout.Button(new GUIContent("Add New Body", "Add new body"), EditorStyles.miniButtonLeft))
			{				
				Undo.RecordObject(targetObject, "Added New Jelly Sprite Body");
				targetObject.m_FreeModeBodyPositions.Add(new Vector2(0.0f, -1.0f));
				targetObject.m_FreeModeBodyRadii.Add(1.0f);
				targetObject.m_FreeModeBodyKinematic.Add(false);
				SceneView.RepaintAll();
			}

			break;
		}

		if(selectedStyle != JellySprite.PhysicsStyle.Free)
		{
			EditorGUILayout.Slider(m_ColliderRadius, 0.001f, 0.25f, new GUIContent("Collider Radius") );
			EditorGUILayout.PropertyField(m_SoftBodyScale, new GUIContent("Collider Scale"));
			EditorGUILayout.PropertyField(m_SoftBodyRotation, new GUIContent("Collider Rotation"));
            EditorGUILayout.PropertyField(m_SoftBodyOffset, new GUIContent("Collider Offset"));
            EditorGUILayout.PropertyField(m_CentralBodyOffset, new GUIContent("Central Body Offset"));

			GUILayout.Space(5);

			if(GUILayout.Button(new GUIContent("Copy Configuration To Free Mode", "Copy this configuration to the free mode layout"), EditorStyles.miniButtonRight))
			{
				Undo.RecordObject(target, "Converted Jelly Sprite to Free Mode");
				targetObject.OnCopyToFreeModeSelected();
				SceneView.RepaintAll();
			}
		}

		GUI.enabled = true;

		EditorGUILayout.PropertyField(m_LockRotation, new GUIContent("Lock Rotation"));

		if(selectedStyle != JellySprite.PhysicsStyle.Free)
		{
			EditorGUILayout.PropertyField(m_CentralBodyKinematic, new GUIContent("Kinematic Central Body"));
		}

		GUILayout.Space(15);

		if(m_PhysicsMode.boolValue)
		{
			EditorGUILayout.PropertyField(m_PhysicsMaterial2D, new GUIContent("Physics Material"));
			m_GravityScale.floatValue = EditorGUILayout.FloatField("Gravity Scale", m_GravityScale.floatValue);            
		}
		else
		{
			EditorGUILayout.PropertyField(m_PhysicsMaterial, new GUIContent("Physics Material"));			
		}

        m_Drag.floatValue = EditorGUILayout.FloatField("Drag", m_Drag.floatValue);
        m_AngularDrag.floatValue = EditorGUILayout.FloatField("Angular Drag", m_AngularDrag.floatValue);
        EditorGUILayout.PropertyField(m_CollideConnected, new GUIContent("Collide Connected"));

		m_Stiffness.floatValue = EditorGUILayout.FloatField("Spring Stiffness", m_Stiffness.floatValue);
		m_DampingRatio.floatValue = EditorGUILayout.FloatField("Spring Damping", m_DampingRatio.floatValue);
		EditorGUILayout.PropertyField(m_MassStyle, new GUIContent("Mass Type"));
		m_Mass.floatValue = EditorGUILayout.FloatField("Mass", m_Mass.floatValue);

		GUILayout.Space(15);

		if(Application.isPlaying)
		{
			GUI.enabled = false;
		}

		EditorGUILayout.Slider(m_DistanceExponent, 1.0f, 4.0f, new GUIContent("Ctrl Point Influence") );

		GUI.enabled = true;

		EditorGUILayout.IntSlider(m_VertexDensity, 2, 100, new GUIContent("Vertex Density"));

		GUILayout.Space(15);

		m_NumAttachPoints.intValue = EditorGUILayout.IntField("Num Attach Points", m_NumAttachPoints.intValue);

		for(int loop = 0; loop < targetObject.m_AttachPoints.Length; loop++)
		{
			targetObject.m_AttachPoints[loop] = (Transform)EditorGUILayout.ObjectField("Attach Point", targetObject.m_AttachPoints[loop], typeof(Transform), true, null);
		}

        m_Color.colorValue = EditorGUILayout.ColorField("Tint Color", m_Color.colorValue);
	}

	public override void OnInspectorGUI() 
	{
		GUILayout.Space(5);
		StoreInitialValues();
		serializedObject.Update();
		DisplayInspectorGUI();
		CheckForObjectChanges();
	}	

	protected void UpdateHandles()
	{
		if(!Application.isPlaying)
		{
			JellySprite jellySprite = (JellySprite)target;
			
			if(jellySprite.m_Style == JellySprite.PhysicsStyle.Free)
			{
				if(jellySprite.m_FreeModeBodyPositions != null)
				{
					Transform transform = jellySprite.transform;
					
					for(int i = 0; i < jellySprite.m_FreeModeBodyPositions.Count; i++)
					{
						Handles.color = jellySprite.m_FreeModeBodyKinematic[i]? Color.red : Color.green;
						
						Vector3 point = transform.TransformPoint(jellySprite.m_FreeModeBodyPositions[i]);
						Vector3 newPosition = transform.InverseTransformPoint(Handles.FreeMoveHandle(point, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotCap));
						newPosition.z = 0.0f;

						if(jellySprite.m_FreeModeBodyPositions[i] != newPosition)
						{
							Undo.RecordObject(target, "Adjusted Jelly Sprite Body Position");
							jellySprite.m_FreeModeBodyPositions[i] = newPosition;
						}

						float newRadius = Handles.RadiusHandle(Quaternion.identity, point, jellySprite.m_FreeModeBodyRadii[i]);

						if(jellySprite.m_FreeModeBodyRadii[i] != newRadius)
						{
							Undo.RecordObject(target, "Adjusted Jelly Sprite Body Radius");
							jellySprite.m_FreeModeBodyRadii[i] = newRadius;
						}
					}
				}
			}
		}
	}
}

public class FixedWidthLabel : IDisposable
{
	//helper class to clear and restore indentation
	private readonly ZeroIndent indentReset;

	public FixedWidthLabel(GUIContent label)
	{
		//Create a horizontal group
		EditorGUILayout.BeginHorizontal();
		//Display the label:
		EditorGUILayout.LabelField(label,
		//Fix its width:
		GUILayout.Width(GUI.skin.label.CalcSize(label).x +
		//Correct for previous indentation: (9 pixels per level)
		9 * EditorGUI.indentLevel));
		//Set following indentation to zero:
		indentReset = new ZeroIndent();
	}
	
	//alternative constructor, if we don't want to deal with GUIContents
	public FixedWidthLabel(string label) : this(new GUIContent(label))
	{
	}

	public void Dispose()
	{
		//restore indentation state:
		indentReset.Dispose();
		//finish horizontal group:
		EditorGUILayout.EndHorizontal();
	}
}

class ZeroIndent : IDisposable //helper class to clear indentation
{
	private readonly int originalIndent;//the original indentation value before we change the GUI state
	public ZeroIndent()
	{
		originalIndent = EditorGUI.indentLevel;//save original indentation
		EditorGUI.indentLevel = 0;//clear indentation
	}
	
	public void Dispose()
	{
		EditorGUI.indentLevel = originalIndent;//restore original indentation
	}
}
