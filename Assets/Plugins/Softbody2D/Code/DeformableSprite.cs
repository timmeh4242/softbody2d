namespace FinerGames {

	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public class DeformableSprite : DeformableMesh {

		public Sprite[] Sprites;
		public Sprite Sprite;
		public Texture2D Texture;
		public int SelectedSprite;
		public List<int> Triangles = new List<int> ();


		public void SetTextureAndSprites (Texture2D sourceTexture, Sprite[] textureSprites) {
			
			Texture = sourceTexture;
			Sprites = textureSprites;
			if(SelectedSprite < 0 || (Sprites != null && SelectedSprite >= Sprites.Length))
				SelectedSprite = 0;

			SelectSprite(SelectedSprite);
		}

		public void SelectSprite (int index) {
			
			if(Sprites == null || Sprites.Length == 0 || Texture == null)
				return;

			if(index < 0 || index > Sprites.Length)
				index = 0;

			SelectedSprite = index;

			Sprite = Sprites[index];

			Vector2 length = new Vector2(Texture.width, Texture.height);

			ApplyNewOffset(-new Vector2((Sprite.rect.x + Sprite.rect.width * 0.5f - Sprite.texture.width * 0.5f) / length.x, (Sprite.rect.y + Sprite.rect.height * 0.5f - Sprite.texture.height * 0.5f) / length.y));
		}

		public override void Initialize(bool forceUpdate = false) {

			base.Initialize(forceUpdate);

			if(Sprites == null || Sprites.Length == 0 || Texture == null)
			{
				Debug.LogWarning("No sprites and/or texture found. exiting operation.");
				return;
			}

			if(MeshRenderer.sharedMaterial == null)
				MeshRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

			if(MeshRenderer.sharedMaterial.mainTexture == null)
				MeshRenderer.sharedMaterial.mainTexture = Texture;


			if(SelectedSprite < 0 || SelectedSprite >= Sprites.Length)
				SelectedSprite = 0;

			Sprite = Sprites[SelectedSprite];

			if(Sprite == null)
				return;

			Vertices.Clear ();
			Vertices.Add (this.transform);
			var pointMasses = this.GetComponentsInChildren<PointMass> ();
			for (int i = 0; i < pointMasses.Length; i++) {
				Vertices.Add (pointMasses [i].transform);
			}
				
			if(forceUpdate || MeshFilter.sharedMesh.vertexCount != Vertices.Count) {

				if(MeshFilter.sharedMesh.vertexCount != Vertices.Count)
					MeshFilter.sharedMesh.Clear();

				Vector2[] uvPts = new Vector2[Vertices.Count];

				var minX = Vertices.Min (_ => _.position.x);
				var maxX = Vertices.Max (_ => _.position.x);
				var minY = Vertices.Min (_ => _.position.y);
				var maxY = Vertices.Max (_ => _.position.y);

				for(int i = 0; i < uvPts.Length; i++) {
					var x = (Vertices [i].position.x - minX) / (maxX - minX);
					var y = (Vertices [i].position.y - minY) / (maxY - minY);

					uvPts [i] = new Vector2 (x, y);
					uvPts[i] -= offset;
				}

				Triangles.Clear ();
				// the first vertice is the center, so doesn't need to be done -> start from 1
				for (int i = 1; i < Vertices.Count; i++) {
					Triangles.Add (i);

					// if on the last vertice, wrap around
					if(i == Vertices.Count - 1)
						Triangles.Add (1);
					else
						Triangles.Add (i + 1);

					Triangles.Add (0);
				}

				var positions = new Vector3[Vertices.Count];
				for (int i = 0; i < Vertices.Count; i++) {
					if(i == 0)
						positions [i] = Vector3.zero;
					else
						positions [i] = Vertices [i].localPosition;
				}
				MeshFilter.sharedMesh.vertices = positions;
				MeshFilter.sharedMesh.uv = uvPts;
				MeshFilter.sharedMesh.triangles = Triangles.ToArray ();
				MeshFilter.sharedMesh.colors = null;
				if(CalculateNormals)
					MeshFilter.sharedMesh.RecalculateNormals();
				if(CalculateTangents)
					CalculateMeshTangents();

				MeshFilter.sharedMesh.RecalculateBounds();
				MeshFilter.sharedMesh.Optimize();
				MeshFilter.sharedMesh.MarkDynamic();
			}
		}

		public void UpdateMesh () {

			var vertices = new Vector3[Vertices.Count];
			for (int i = 0; i < Vertices.Count; i++) {
				if (i == 0) {
					vertices [i] = Vector3.zero;
				} else {
					vertices [i] = Vertices [i].localPosition;
				}
			}

			MeshFilter.sharedMesh.vertices = vertices;
			MeshFilter.sharedMesh.RecalculateBounds ();
		}

		public override bool UpdatePivotPoint (Vector2 change, out MonoBehaviour monoBehavior) {
			
			if(MeshFilter.sharedMesh == null)
				Initialize (true);

			pivotOffset -= change; //TODO update this in all mesh links!!!!

			var positions = new Vector3[Vertices.Count];
			for (int i = 0; i < Vertices.Count; i++) {
				if(i == 0)
					positions [i] = Vector3.zero;
				else
					positions [i] = Vertices [i].localPosition;
			}

			for(int i = 0; i < MeshFilter.sharedMesh.vertices.Length; i++)
				positions [i] -= (Vector3)change;

			MeshFilter.sharedMesh.vertices = positions;

			return base.UpdatePivotPoint (change, out monoBehavior);
		}
	}
}
