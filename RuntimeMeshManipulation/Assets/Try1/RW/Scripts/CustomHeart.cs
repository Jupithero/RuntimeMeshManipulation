/*
 * Copyright (c) 2019 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/

using System.Collections.Generic;
using UnityEngine;

public class CustomHeart : MonoBehaviour {
    // For Editor
    public enum EditType {
        AddIndices,
        RemoveIndices,
        None
    }

    [HideInInspector] public int targetIndex;

    [HideInInspector] public Vector3 targetVertex;

    [HideInInspector] public Vector3[] oVertices;

    [HideInInspector] public Vector3[] modifiedVertices;

    [HideInInspector] public Vector3[] normals;

    public EditType editType;

    public bool showTransformHandle = true;
    public List<int> selectedIndices = new List<int>();
    public float pickSize = 0.01f;

    // Deforming settings
    public float radiusofeffect = 0.3f;
    public float pullvalue = 0.3f;

    // Animation settings
    public float duration = 1.2f;
    private Mesh cMesh;
    private int currentIndex;
    private bool isAnimate;
    private MeshFilter oFilter;
    private Mesh oMesh;
    private float runtime;
    private float starttime;


    private void Start() {
        Init();
    }

    private void FixedUpdate() {
        if (!isAnimate) return;

        runtime = Time.time - starttime;

        if (runtime < duration) {
            var relativePoint = oFilter.transform.InverseTransformPoint(targetVertex);
            DisplaceVertices(relativePoint, pullvalue, radiusofeffect);
        }
        else {
            currentIndex++;
            if (currentIndex < selectedIndices.Count) {
                StartDisplacement();
                Debug.Log("next");
            }
            else {
                oMesh = GetComponent<MeshFilter>().sharedMesh;
                isAnimate = false;
                Debug.Log("done");
            }
        }
    }

    public void Init() {
        oFilter = GetComponent<MeshFilter>();
        currentIndex = 0;

        if (editType == EditType.AddIndices || editType == EditType.RemoveIndices) {
            oMesh = oFilter.sharedMesh;
            cMesh = new Mesh();
            cMesh.name = "clone";
            cMesh.vertices = oMesh.vertices;
            cMesh.triangles = oMesh.triangles;
            cMesh.normals = oMesh.normals;
            oFilter.mesh = cMesh;

            // update local variables...
            oVertices = cMesh.vertices;

            normals = cMesh.normals;
            Debug.Log("Init & Cloned");
        }
        else {
            oMesh = oFilter.sharedMesh;
            oVertices = oMesh.vertices;

            normals = oMesh.normals;
            modifiedVertices = new Vector3[oVertices.Length];
            for (var i = 0; i < oVertices.Length; i++) modifiedVertices[i] = oVertices[i];

            StartDisplacement();
        }
    }

    public void StartDisplacement() {
        targetVertex = modifiedVertices[selectedIndices[currentIndex]];
        starttime = Time.time;
        isAnimate = true;
    }

    private void DisplaceVertices(Vector3 pos, float force, float radius) {
        var vert = Vector3.zero;
        var sqrRadius = radius * radius;

        for (var i = 0; i < modifiedVertices.Length; i++) {
            var sqrMagnitude = (modifiedVertices[i] - pos).sqrMagnitude;
            if (sqrMagnitude > sqrRadius) continue;
            vert = modifiedVertices[i];

            var distance = Mathf.Sqrt(sqrMagnitude);
        }

        oMesh.vertices = modifiedVertices;
        oMesh.RecalculateNormals();
    }

    public void ClearAllData() {
        selectedIndices = new List<int>();
        targetIndex = 0;
        targetVertex = Vector3.zero;
    }

    private void CurveType1() { }

    private void CurveType2() { }

    public void ShowNormals() {
        for (var i = 0; i < modifiedVertices.Length; i++)
            Debug.DrawLine(transform.TransformPoint(modifiedVertices[i]), transform.TransformPoint(normals[i]), Color.green, 4.0f, false);
    }
}