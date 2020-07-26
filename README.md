# UnityURP-MobileDrawMeshInstancedIndirectExample

youtube: https://youtu.be/Y7wAwMn4i2M  
download .apk: https://drive.google.com/file/d/185JWZXYPnVyDnA451cEZkS2H2wOYSce_/view

 DrawMeshInstancedIndirect ON
 ![screenshot](https://i.imgur.com/DDPbFhQ.png)
 ![screenshot](https://i.imgur.com/rBvlLeG.png)
 DrawMeshInstancedIndirect ON (grass bending)
 ![screenshot](https://i.imgur.com/QDXbEZw.png)
 ![screenshot](https://i.imgur.com/E7wEEPR.png)
 DrawMeshInstancedIndirect OFF
 ![screenshot](https://i.imgur.com/xOhTW6d.png)
 
 Why create this project?
 -------------
 To demonstrate DrawMeshInstancedIndirect API on mobile devices.
- can draw 100,000 instance on almost any mobile GPU(e.g. adreno506) within 5ms, performance mainly affected by visible grass count on screen
- can draw 1,000,000 instances on 2018/2019 flagship mobile GPU (adreno630) within 7ms, performance mainly affected by visible grass count on screen
 
 Requirement
 -----------------
 if you try the pre-built .apk, your android device must support Opengles3.2 / Vulkan
 
 Editor
 ------------
 2019.4.3f1
 
 Note
 -------------
 This is a simplified example project to demonstrate DrawMeshInstancedIndirect API on mobile platform.  
 Does not contain any compute GPU culling and Acceleration Algorithms. It is as simple as possible, just 1 DrawMeshInstancedIndirect call, nothing else.
 
 Lighting and animation is not the main focus of this project, but >50% of the time was spent on writing grass shader's lighting & animation, you can have a look at  InstancedIndirectGrass.shader if you are interested.  
 
 This project also contains a RendererFeature(GrassBendingRTPrePass.cs) to render an offscreen RT(R8), which renders top down view grass bending area (by trail renderer following moving objects), it is a very simple method but the result is good enough for this demo.
 
some assets that use DrawMeshInstancedIndirect also in the asset store
-------------------
- https://assetstore.unity.com/packages/tools/utilities/gpu-instancer-117566
- https://assetstore.unity.com/packages/tools/terrain/advanced-terrain-grass-100014
