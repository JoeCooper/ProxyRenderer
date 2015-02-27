using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
public class ProxyRenderer : MonoBehaviour
{
	public Material[] materials;

	private readonly ProxyRendererObjectPool<CanvasRenderer> canvasRendererPool;
	private readonly ChangeMonitor monitor;

	private readonly IDictionary<int,CanvasRenderer> canvasRenderersBySubmeshIndex;

	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	public ProxyRenderer()
	{
		canvasRenderersBySubmeshIndex = new Dictionary<int, CanvasRenderer>();
		canvasRendererPool = new ProxyRendererObjectPool<CanvasRenderer>(CreateCanvasRenderer, ProxyRenderer.DestroyCanvasRenderer, 8);
		monitor = new ChangeMonitor(true);
	}
	
	void Start() {
		meshFilter = GetComponent<MeshFilter>();

		monitor.Add(() => IsChildOfCanvas);
		monitor.Add(() => meshFilter.sharedMesh);
		monitor.Add<Material[]>(() => materials, ChangeMonitor.ArrayEvaluator);
	}
	
	private bool IsChildOfCanvas {
		get {
			return GetComponentInParent<Canvas>() != null;
		}
	}

	private CanvasRenderer CreateCanvasRenderer()
	{
		var go = new GameObject();
		var goTransform = go.transform;
		goTransform.parent = this.transform;
		goTransform.localScale = Vector3.one;
		goTransform.localPosition = Vector3.zero;
		goTransform.localRotation = Quaternion.identity;
		var canvasRenderer = go.AddComponent<CanvasRenderer>();
		return canvasRenderer;
	}

	private static void DestroyCanvasRenderer(CanvasRenderer r)
	{
		GameObject.Destroy(r.gameObject);
	}

	void LateUpdate() {
		if(monitor.Evaluate())
		{
			if(IsChildOfCanvas)
			{
				if(meshRenderer != null)
				{
					GameObject.DestroyImmediate(meshRenderer);
					meshRenderer = null;
				}

				var mesh = meshFilter.sharedMesh;

				if(mesh != null && materials.Length > 0)
				{
					var positions = mesh.vertices;
					var colors = mesh.colors32;
					var coords = mesh.uv;
					var coords2 = mesh.uv2;
					var normals = mesh.normals;
					var tangents = mesh.tangents;

					var defaultColor = new Color32(255,255,255,255);
					var defaultNormal = Vector3.forward;
					var defaultCoord = Vector2.zero;
					var defaultTangent = Vector4.zero;

					for(int i = 0; i < mesh.subMeshCount; i++)
					{
						var material = materials[i];

						CanvasRenderer canvasRenderer;

						if(canvasRenderersBySubmeshIndex.ContainsKey(i))
						{
							canvasRenderer = canvasRenderersBySubmeshIndex[i];
						}
						else {
							canvasRenderersBySubmeshIndex[i] = canvasRenderer = canvasRendererPool.GetObject();
						}

						var triangleIndices = mesh.GetTriangles(i);

						var triangleCount = triangleIndices.Length / 3;
						var sourceIndices = new int[triangleCount * 4];

						for(int j = 0; j < triangleCount; j++)
						{
							sourceIndices[j * 4] = triangleIndices[j * 3];
							sourceIndices[j * 4 + 1] = triangleIndices[j * 3];
							sourceIndices[j * 4 + 2] = triangleIndices[j * 3 + 1];
							sourceIndices[j * 4 + 3] = triangleIndices[j * 3 + 2];
						}

						var uiVertices = new UIVertex[sourceIndices.Length];

						for(int j = 0; j < sourceIndices.Length; j++)
						{
							var sourceIndex = sourceIndices[j];
							uiVertices[j] = new UIVertex {
								position = positions[sourceIndex],
								color = sourceIndex < colors.Length ? colors[sourceIndex] : defaultColor,
								normal = sourceIndex < normals.Length ? normals[sourceIndex] : defaultNormal,
								tangent = sourceIndex < tangents.Length ? tangents[sourceIndex] : defaultTangent,
								uv0 = sourceIndex < coords.Length ? coords[sourceIndex] : defaultCoord,
								uv1 = sourceIndex < coords2.Length ? coords2[sourceIndex] : defaultCoord
							};
						}

						canvasRenderer.SetMaterial(material, null);
						canvasRenderer.SetVertices(uiVertices, uiVertices.Length);
						canvasRenderer.SetColor(Color.white);
						canvasRenderer.SetAlpha(1f);
						canvasRenderer.gameObject.SetActive(true);
					}
				}

				var subMeshCount = mesh != null ? mesh.subMeshCount : 0;

				if(canvasRenderersBySubmeshIndex.Count > mesh.subMeshCount)
				{
					var oldKeys = from key in canvasRenderersBySubmeshIndex.Keys where key >= subMeshCount select key;
					foreach(var key in oldKeys)
					{
						var canvasRenderer = canvasRenderersBySubmeshIndex[key];
						canvasRenderersBySubmeshIndex.Remove(key);
						canvasRenderer.gameObject.SetActive(false);
						canvasRendererPool.PutObject(canvasRenderer);
					}
				}
			}
			else {
				foreach(var canvasRenderer in canvasRenderersBySubmeshIndex.Values)
				{
					DestroyCanvasRenderer(canvasRenderer);
				}
				canvasRenderersBySubmeshIndex.Clear();

				meshRenderer = meshRenderer ?? gameObject.AddComponent<MeshRenderer>();
				meshRenderer.sharedMaterials = materials;
			}
		}
	}
}
