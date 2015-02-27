# ProxyRenderer
A Unity3D component that provides a way to present geometry *either* through a CanvasRenderer *or* a MeshRenderer, as appropriate.

ProxyRenderer is attached a component alongside a MeshFilter. It observes the MeshFilter and manages the existence of *either* a MeshRenderer *or* a set of CanvasRenderers.

The transfer of geometry from the Mesh – provided by the filter – to the CanvasRenderers can be computationally expensive as the data must be extracted from the Mesh through its high level interfaces, then generate the interleaved vertex data which the CanvasRenderers expect.

Changes to the set of materials, or submesh count, also involve labor when presenting through CanvasRenderers. Because CanvasRenderers will not coexist attached to the same object, and will not present multiple materials, a set of child objects must be managed.

## Quads

Due to the fact that the CanvasRenderer interface takes only quads, and the Mesh class provides triangles, ProxyRenderer must push triangles *as* quads by doubling up one of the vertices.

This presents two problems.

First, it relies on undefined behavior in Unity3D; triangles are presented as quads by doubling the *first* vertex, which makes an assumption about how Unity will convert the quads *back* into triangles. This means that an internal change in a subsequent Unity release will break this kit and warrant amendment.

Second, this will induce unwarranted computational work at runtime.

## Why?

ProxyRenderer is created as bridge code for Noble Reader (found in the Unity Asset Store), so that the Noble Reader can present in a canvas while remaining compatible with existing configurations.

It is an esoteric thing, but perhaps this isn't the only use for it.