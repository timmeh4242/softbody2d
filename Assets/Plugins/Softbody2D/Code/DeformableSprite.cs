﻿/* /*
Copyright (c) 2014 David Stier

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.


******Jello Physics was born out of Walabers JellyPhysics. As such, the JellyPhysics license has been include.
******The original JellyPhysics library may be downloaded at http://walaber.com/wordpress/?p=85.


Copyright (c) 2007 Walaber

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace FinerGames {
	
	using UnityEngine;
	using System.Collections;

	[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
	public class DeformableSprite : DeformableMesh {

		public Sprite m_Sprite;
		public int XDensity = 1;
		public int YDensity = 1;

		Vector3 [] Vertices = new Vector3[4];
		Vector2 [] UVs = new Vector2[4];
		Vector3 [] Normals = new Vector3[4];
		int [] Triangles = new int[6];


		Mesh m_Mesh;
		MeshFilter m_MeshFilter;
		MeshRenderer m_MeshRenderer;

		void OnValidate () {
			m_MeshFilter = this.GetComponent<MeshFilter> ();
			m_MeshRenderer = this.GetComponent<MeshRenderer> ();
		}

		void Start () {

		}

//		public void UpdateMesh (PointMass[] points) {
		public void UpdateMesh () {

			m_Mesh = new Mesh ();
			m_MeshFilter = this.GetComponent<MeshFilter> ();
			m_MeshRenderer = this.GetComponent<MeshRenderer> ();

			m_MeshFilter.mesh = m_Mesh;

			Vertices [0] = new Vector3 (0, 0, 0);
			UVs [0] = new Vector2 (0, 0);
			Normals [0] = Vector3.up;

			Vertices [1] = new Vector3 (0, 1, 0);
			UVs [1] = new Vector2 (0, 1);
			Normals [1] = Vector3.up;

			Vertices [2] = new Vector3 (1, 1, 0);
			UVs [2] = new Vector2 (1, 1);
			Normals [2] = Vector3.up;

			Vertices [3] = new Vector3 (1, 0, 0);
			UVs [3] = new Vector2 (1, 0);
			Normals [3] = Vector3.up;

			Triangles [0] = 0;
			Triangles [1] = 1;
			Triangles [2] = 2;

			Triangles [3] = 0;
			Triangles [4] = 2;
			Triangles [5] = 3;

			m_Mesh.vertices = Vertices;
			m_Mesh.uv = UVs;
			m_Mesh.normals = Normals;
			m_Mesh.triangles = Triangles;

			m_Mesh.RecalculateBounds ();
			m_Mesh.RecalculateNormals ();
//			if(vertices.Length != points.Length)
//				vertices = new Vector3[points.Length];
//
//			for(int i = 0; i < vertices.Length; i++)
//				vertices[i] = (Vector3)points[i].transform.localPosition;
//
//			LinkedMeshFilter.sharedMesh.vertices = vertices;
//			LinkedMeshFilter.sharedMesh.RecalculateBounds();
		}
	}
}
