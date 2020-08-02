# UnityURP-MobileDrawMeshInstancedIndirectExample

youtube runtime demo video: https://youtu.be/Y7wAwMn4i2M  
download .apk, try it on your android phone: https://drive.google.com/file/d/185JWZXYPnVyDnA451cEZkS2H2wOYSce_/view

 DrawMeshInstancedIndirect ON
 ![screenshot](https://i.imgur.com/DDPbFhQ.png)
 ![screenshot](https://i.imgur.com/rBvlLeG.png)
 DrawMeshInstancedIndirect ON (grass bending)
 ![screenshot](https://i.imgur.com/QDXbEZw.png)
 ![screenshot](https://i.imgur.com/E7wEEPR.png)
 DrawMeshInstancedIndirect OFF
 ![screenshot](https://i.imgur.com/xOhTW6d.png)
 
 Why create this repository?
 -------------
 To demonstrate an API that can draw millions of instance -> DrawMeshInstancedIndirect(), running on mobile devices.
 
 Can this demo runs on midrange mobile?
---------------
- can handle 10 million instances on Samsung Galaxy A70 (GPU = adreno612, not a strong GPU), 50~60fps, performance mainly affected by visible grass count on screen(draw distance = 125)
- can handle 10 million instances on Lenovo S5 (GPU = adreno506, a weak GPU), 30fps, performance mainly affected by visible grass count on screen(draw distance = 75)
 
 Requirement
 -----------------
 if you want to try the pre-built .apk, your android device must support Opengles3.2 / Vulkan  
 download .apk: https://drive.google.com/file/d/185JWZXYPnVyDnA451cEZkS2H2wOYSce_/view
 
 Where are the important files
 ----------------
 https://github.com/ColinLeung-NiloCat/UnityURP-MobileDrawMeshInstancedIndirectExample/tree/master/Assets/URPMobileGrassInstancedIndirectDemo/InstancedIndirectGrass/Core
 
 Editor
 ------------
 2019.4.3f1
 
 Note
 -------------
 This is a simplified example repository to demonstrate DrawMeshInstancedIndirect API on mobile platform.  
 This repository is as simple as possible, only contains a simple CPU cell frustum culling(not even a quadtree) -> minimum compute GPU frustum culling (no Acceleration Algorithms), then just 1 DrawMeshInstancedIndirect call, nothing else, code is very short.
 
 Lighting and animation is not the main focus of this project, but ~40% of the time was spent on writing grass shader's lighting & animation, you can have a look at  InstancedIndirectGrass.shader if you are interested.  
 
 This repository also contains a RendererFeature(GrassBendingRTPrePass.cs) to render an offscreen RT(R8), which renders top down view grass bending area (by trail renderer following moving objects), it is a very simple method but the result is good enough for this demo.
 
reference
-------------------
- https://github.com/ellioman/Indirect-Rendering-With-Compute-Shaders
- https://github.com/tiiago11/Unity-InstancedIndirectExamples
- https://assetstore.unity.com/packages/tools/utilities/gpu-instancer-117566
- https://assetstore.unity.com/packages/tools/terrain/advanced-terrain-grass-100014

