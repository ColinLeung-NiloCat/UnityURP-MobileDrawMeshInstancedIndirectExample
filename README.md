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
 
 Note
 -------------
 This is a simplified example project, just to test DrawMeshInstancedIndirect API on mobile.  
 Does not contain any compute shader GPU culling and Acceleration Algorithms. It is as simple as possible.
 
Asset that use DrawMeshInstancedIndirect?
-------------------
https://assetstore.unity.com/packages/tools/utilities/gpu-instancer-117566?aid=1100l3Rmg&utm_source=aff#reviews
 
