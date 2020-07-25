# UnityURP-MobileDrawMeshInstancedIndirectExample

youtube: ______
download .apk: __________

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
 To test DrawMeshInstancedIndirect API on mobile devices. And found that everything works very well even on weak GPU like adreno506.  
- can draw 10,000 instance on almost any mobile GPU(e.g. adreno506) within 4ms, performance affected mainly by grass's vertex shader
- can draw 100,000 instances on 2018/2019 flagship mobile GPU (adreno630) within 4ms, performance affected mainly by grass's vertex shader
 
 Requirement
 -----------------
 your android device must support Opengles3.2 / Vulkan
 
 Editor
 ------------
 2019.4.3f1
 
 Note
 -------------
 This is a simplified example project, just to test DrawMeshInstancedIndirect API on mobile.  
 Does not contain any compute GPU culling and Acceleration Algorithms. It is as simple as possible, just 1 DrawMeshInstancedIndirect call.
 
 Lighting and animation is not the main focus of this project, but >50% of the time was spent on writing grass shader lighting & animation, you can have a look at the InstancedIndirectGrass.shader if you are interested.  
 
 This project also contains an offscreen RT(R8) to render a top down grass bending area using trail renderer, it is very simple but the result is good enough for this demo.
 
Asset that use DrawMeshInstancedIndirect?
-------------------
- https://assetstore.unity.com/packages/tools/utilities/gpu-instancer-117566
- https://assetstore.unity.com/packages/tools/terrain/advanced-terrain-grass-100014
