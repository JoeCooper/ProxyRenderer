using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class ProxyRenderer : MonoBehaviour
{
	public Material[] materials;

	private readonly ProxyRendererObjectPool<CanvasRenderer> canvasRendererPool;
	private readonly ChangeMonitor monitor;

	private readonly IDictionary<Material,CanvasRenderer> canvasRenderersByMaterial;

	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	public ProxyRenderer()
	{
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

	void Update() {
		if(monitor.Evaluate())
		{
			if(IsChildOfCanvas)
			{
				if(meshRenderer != null)
				{
					GameObject.DestroyImmediate(meshRenderer);
					meshRenderer = null;
				}
			}
			else {
				foreach(var canvasRenderer in canvasRenderersByMaterial.Values)
				{
					DestroyCanvasRenderer(canvasRenderer);
				}
				canvasRenderersByMaterial.Clear();

				meshRenderer = meshRenderer ?? gameObject.AddComponent<MeshRenderer>();
				meshRenderer.sharedMaterials = materials;
			}
		}
	}

	private void PushState()
	{

	}
}
