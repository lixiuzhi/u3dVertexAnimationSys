<<<<<<< HEAD
# u3dVertexAnimationSys
实现原理在我的博客园里有写，很久没更新，也懒得搬移，链接https://www.cnblogs.com/lxzCode/p/4780360.html    
利用animation clip生成顶点动画数据   
将mesh的vertex normal tangent uv2通道来存取关键帧顶点信息    
运行时cpu来控制动画时间推移，和mesh的切换    
通过shader来取得通道数据进行顶点插值     
## 用法    
参考test场景   
=======
抽取骨骼动画关键帧顶点数据，组合成新的mesh，将mesh的法线，切线，uv等通道用来存关键帧顶点数据
运行时候通过顶点shader进行gpu插值
cpu控制时间轴以及长动画的mesh切换

增加了gpu instance的支持
>>>>>>> 6f0fa0d37ed7aea31173dbdbe088ead862f6f50a
