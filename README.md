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
 To demonstrate the only API that can draw millions of instance -> DrawMeshInstancedIndirect, running on mobile devices.
 
 How fast is DrawMeshInstancedIndirect API?
---------------
- can draw 4 million instances on Samsung Galaxy A70 (GPU = adreno612, not a strong GPU), 30fps, performance mainly affected by visible grass count on screen
- can draw 10 million instances on most of the 2018/2019 flagship mobiles (GPU = adreno630 or better), >30fps, performance mainly affected by visible grass count on screen
 
 Requirement
 -----------------
 if you want to try the pre-built .apk, your android device must support Opengles3.2 / Vulkan
 
 Editor
 ------------
 2019.4.3f1
 
 Note
 -------------
 This is a simplified example project to demonstrate DrawMeshInstancedIndirect API on mobile platform.  
 This project is as simple as possible, only contain a minimum compute GPU frustum culling (no Acceleration Algorithms), then just 1 DrawMeshInstancedIndirect call, nothing else.
 
 Lighting and animation is not the main focus of this project, but >50% of the time was spent on writing grass shader's lighting & animation, you can have a look at  InstancedIndirectGrass.shader if you are interested.  
 
 This project also contains a RendererFeature(GrassBendingRTPrePass.cs) to render an offscreen RT(R8), which renders top down view grass bending area (by trail renderer following moving objects), it is a very simple method but the result is good enough for this demo.
 
reference
-------------------
- https://github.com/ellioman/Indirect-Rendering-With-Compute-Shaders
- https://github.com/tiiago11/Unity-InstancedIndirectExamples
- https://assetstore.unity.com/packages/tools/utilities/gpu-instancer-117566
- https://assetstore.unity.com/packages/tools/terrain/advanced-terrain-grass-100014

