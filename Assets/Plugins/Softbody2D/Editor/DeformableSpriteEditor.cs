namespace FinerGames {
	
	using UnityEngine;
	using System.Collections;
	using UnityEditor;

	[CustomEditor(typeof(DeformableSprite))]
	public class DeformableSpriteEditor : Editor 
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if(GUILayout.Button("Initialize"))
			{
				DeformableSprite script = (DeformableSprite)target;
				script.Initialize (true);
			}
		}
//		GUIContent updateMeshContent = new GUIContent("Update Mesh", "Make the mesh conform to the collider");
//
//		SerializedProperty eTexture;
//		SerializedProperty eSelectedSprite;
//		DeformableSprite spriteMeshLink;
//
//		Object[] sprites;
//
//		protected override void Enabled()
//		{
//			base.Enabled();
//
//			eTexture = serializedObject.FindProperty("texture");
//			eSelectedSprite = serializedObject.FindProperty("selectedSprite");
//			spriteMeshLink = (DeformableSprite)target;//serializedObject.targetObject;
//
//			if(eTexture.objectReferenceValue != null)
//				sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(eTexture.objectReferenceValue));
//		}
//
//		protected override void DrawInspectorGUI()
//		{
//			base.DrawInspectorGUI();
//
//			serializedObject.Update();
//
//	//		EditorGUI.showMixedValue = eOffset.hasMultipleDifferentValues;
//	//		EditorGUI.BeginChangeCheck();
//	//		EditorGUILayout.PropertyField(eOffset, new GUIContent("Offset"));
//	//		if(EditorGUI.EndChangeCheck())
//	//		{
//	//			for(int i = 0; i < serializedObject.targetObjects.Length; i++)
//	//			{
//	//				MeshLink link = (MeshLink)serializedObject.targetObjects[i];
//	//				
//	//				link.ApplyNewOffset(eOffset.vector2Value);
//	//
//	//				EditorUtility.SetDirty(link.body);
//	//			}
//	//		}
//	//		EditorGUI.showMixedValue = false;
//	//		
//	//		EditorGUI.showMixedValue = eScale.hasMultipleDifferentValues;
//	//		EditorGUI.BeginChangeCheck();
//	//		EditorGUILayout.PropertyField(eScale, new GUIContent("Scale"));
//	//		if(EditorGUI.EndChangeCheck())
//	//		{
//	//			for(int i = 0; i < serializedObject.targetObjects.Length; i++)
//	//			{
//	//				MeshLink link = (MeshLink)serializedObject.targetObjects[i];
//	//				
//	//				link.Initialize(true);
//	//
//	//				EditorUtility.SetDirty(link.body);
//	//			}
//	//		}
//	//		EditorGUI.showMixedValue = false;
//	//		
//	//		EditorGUI.showMixedValue = eAngle.hasMultipleDifferentValues;
//	//		EditorGUI.BeginChangeCheck();
//	//		EditorGUILayout.PropertyField(eAngle, new GUIContent("Angle"));
//	//		if(EditorGUI.EndChangeCheck())
//	//		{
//	//			for(int i = 0; i < serializedObject.targetObjects.Length; i++)
//	//			{
//	//				MeshLink link = (MeshLink)serializedObject.targetObjects[i];
//	//				
//	//				link.Initialize(true);
//	//
//	//				EditorUtility.SetDirty(link.body);
//	//			}
//	//		}
//
//			DrawOffsetScaleAngleEditorGUI();
//
//	//		serializedObject.ApplyModifiedProperties(); //why is this here????
//
//			EditorGUI.showMixedValue = eTexture.hasMultipleDifferentValues;
//			EditorGUI.BeginChangeCheck();
//			EditorGUILayout.PropertyField(eTexture); //may need to protect against textures without sprites...
//			if(EditorGUI.EndChangeCheck())
//			{
//				eSelectedSprite.intValue = 0;
//
//				Undo.RecordObjects(serializedObject.targetObjects, "SML change");
//
//				for(int i = 0; i < serializedObject.targetObjects.Length; i++)
//				{
//					DeformableSprite link = (DeformableSprite)serializedObject.targetObjects[i];
//
//					Undo.RecordObject(link.GetComponent<MeshRenderer>(), "SML renderer");
//					Undo.RecordObject(link.GetComponent<MeshFilter>(), "SML filter");
//					
//					if(eTexture.objectReferenceValue != null)
//					{//if set to null, then maybe change to default sprite for renderer????
//						link.texture = (Texture2D)eTexture.objectReferenceValue;
//
//						sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(eTexture.objectReferenceValue));
//				
//						if(sprites == null || sprites.Length == 0)
//						{
//							Debug.LogWarning("There are no sprites assosiated with the selected texture, change the import setings of this texture to sprite and try again or use the TextureMeshLink class instead");
//							return;
//						}
//				
//						if(link.sprites == null || link.sprites.Length != sprites.Length)
//							link.sprites = new Sprite[sprites.Length];
//						for(int s = 0; s < sprites.Length; s++)
//							link.sprites[s] = sprites[s] as Sprite;
//
//						//select sprite
//						link.SelectSprite(eSelectedSprite.intValue);
//
//						MeshRenderer renderer = link.GetComponent<MeshRenderer>();
//						if(renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture != (Texture2D)eTexture.objectReferenceValue)
//						{
//							if(renderer.sharedMaterial == null)
//								link.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
//
//							link.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = (Texture2D)eTexture.objectReferenceValue;
//						}
//					} 
//					else
//					{
//						for(int s = 0; s < link.sprites.Length; s++)
//							link.sprites[s] = null;
//						link.sprites = null;
//						link.GetComponent<MeshRenderer>().sharedMaterial = null;
//						link.LinkedMeshFilter.mesh = null;
//					}
//
//					link.Initialize(true);
//					eOffset.vector2Value = link.offset;
//
//					EditorUtility.SetDirty(link);
//				}
//
//				serializedObject.UpdateIfDirtyOrScript();
//			}
//			EditorGUI.showMixedValue = false;
//
//
//			if(!eTexture.hasMultipleDifferentValues && eTexture.objectReferenceValue != null) //this wont pick up if one is null and another is not...?
//			{
//				if(spriteMeshLink.sprites != null && spriteMeshLink.sprites.Length != 0)
//				{
//					GUIContent[] contents = new GUIContent[sprites.Length];
//					for(int i = 0; i < contents.Length; i++)
//						contents[i] = new GUIContent(sprites[i].name);
//
//					GUIStyle selectedSpriteStyle = new GUIStyle(EditorStyles.popup);
//					if(eSelectedSprite.prefabOverride || eTexture.prefabOverride)
//						selectedSpriteStyle.fontStyle = FontStyle.Bold;
//
//					EditorGUI.showMixedValue = eSelectedSprite.hasMultipleDifferentValues;
//					EditorGUI.BeginChangeCheck();
//					eSelectedSprite.intValue =  EditorGUILayout.Popup(new GUIContent("Sprite"), eSelectedSprite.intValue, contents, selectedSpriteStyle); //TODO make bold???
//					if(EditorGUI.EndChangeCheck())
//					{
//						for(int i = 0; i < serializedObject.targetObjects.Length; i++)
//						{
//							DeformableSprite link = (DeformableSprite)serializedObject.targetObjects[i];
//
//							if(link.texture == null)
//								continue;
//
//							if(link.selectedSprite < 0 || link.selectedSprite > link.sprites.Length)
//								link.selectedSprite = 0;
//
//							eOffset.vector2Value = link.offset;
//							link.SelectSprite(eSelectedSprite.intValue);
//							eOffset.vector2Value = link.offset;
//						
//							EditorUtility.SetDirty(link.body);
//						}
//					}
//					EditorGUI.showMixedValue = false;
//
//					int index = eSelectedSprite.intValue;
//					if(index < 0 || index >= spriteMeshLink.sprites.Length)
//						index = 0;
//
//					Sprite sp = spriteMeshLink.sprites[index];
//					Rect spriteRect = new Rect(sp.rect.x / sp.texture.width, sp.rect.y / sp.texture.height, sp.rect.width / sp.texture.width, sp.rect.height / sp.texture.height);
//					float maxRectSize = 128f;
//					Vector2 rectSize = Vector2.one *  maxRectSize;
//					if(sp.rect.width > sp.rect.height)
//					{
//						rectSize.y = maxRectSize * sp.rect.height / sp.rect.width;
//					}
//					else if(sp.rect.width < sp.rect.height)
//					{
//						rectSize.x = maxRectSize * sp.rect.width / sp.rect.height;
//					}
//					Rect r = GUILayoutUtility.GetRect(rectSize.x, rectSize.y, GUILayout.ExpandWidth(false));
//
//					GUI.DrawTextureWithTexCoords(r, sp.texture, spriteRect);
//				}
//			}
//			else
//			{
//				EditorGUILayout.HelpBox("Mulit-edit sprite selection not supported when source textures are not the same.", MessageType.Info);
//			}
//
//			if(GUILayout.Button(updateMeshContent, EditorStyles.miniButton))
//			{	 
//				for(int i = 0; i < serializedObject.targetObjects.Length; i++)
//				{
//					DeformableSprite link = (DeformableSprite)serializedObject.targetObjects[i];
//					
//					link.Initialize(true);
//
//					EditorUtility.SetDirty(link.body);
//				}
//			}
//
//			serializedObject.ApplyModifiedProperties();
//		}
	}
}
