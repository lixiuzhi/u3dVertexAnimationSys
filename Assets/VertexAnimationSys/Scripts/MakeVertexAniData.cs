using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class MakeVertexAniData : MonoBehaviour {

    //float clipTimeLenghts = 1;

    //float frame2Pos = 0.333f;
    //float frame3Pos = 0.6666f;

    [Serializable]
    public class MeshTimePair {
        public Mesh mesh;
        public float timePoint;
    }

    [Header ("   ")]
    [Header ("拖动CapAniPointScale100进度来控制播放时间点")]
    [Header ("点击c键截取")]
    [Header ("整个动画截取完后，点击m键生成数据")] 

    public Animation CaptureAnimation;
    public SkinnedMeshRenderer CaptureSkinMesh;

    public float AniTotalLen = 1;

    public float CapAniPointScale100 = 0;
    public float CapAniPoint = 0;

    public string animationResPrefix = "";
    public string AniName;
    public List<MeshTimePair> meshList;

    // Update is called once per frame
    void Update () {

        AniTotalLen = CaptureAnimation.clip.length;
        CapAniPointScale100 = Mathf.Clamp (CapAniPointScale100, 0, AniTotalLen * 100f);
        CapAniPoint = CapAniPointScale100 * 0.01f;

        CaptureAnimation[CaptureAnimation.clip.name].wrapMode = WrapMode.Loop;
        CaptureAnimation[CaptureAnimation.clip.name].time = CapAniPoint;
        CaptureAnimation[CaptureAnimation.clip.name].speed = 0;

        if (Input.GetKeyDown (KeyCode.C)) {
            Mesh mesh0 = new Mesh ();
            CaptureSkinMesh.BakeMesh (mesh0);
            meshList.Add (new MeshTimePair { mesh = mesh0, timePoint = CapAniPoint });
        }

        if (Input.GetKeyDown (KeyCode.M)) {

            if (string.IsNullOrEmpty (animationResPrefix)) {
                Debug.LogError ("没有设置资源前缀");
                return;
            }

            if (meshList.Count < 2)
                return;

            int addCount = 0;

            int N = (meshList.Count - 1) / 3;

            if ((meshList.Count - 1) % 3 != 0) {
                N++;
            }
            addCount = 3 * N + 1 - meshList.Count;

            for (int i = 0; i < addCount; i++) {
                meshList.Add (meshList[meshList.Count - 1]);
            }

            List<byte[]> genDataList = new List<byte[]> ();
            for (int j = 0, i = 0; j < N; j++, i += 3) {
                //当前合成mesh时间长度
                float timeLen = meshList[i + 3].timePoint - meshList[i].timePoint;
                //第二个关键帧时间
                float f2 = (meshList[i + 1].timePoint - meshList[i].timePoint) / timeLen;
                //第三个关键帧时间
                float f3 = (meshList[i + 2].timePoint - meshList[i].timePoint) / timeLen;
                genDataList.Add (Make (meshList[i].mesh, meshList[i + 1].mesh, meshList[i + 2].mesh, meshList[i + 3].mesh, timeLen, f2, f3));
            }

            ByteBuffer bbuffer = new ByteBuffer ();
            bbuffer.WriteInt (genDataList.Count);
            for (int i = 0; i < genDataList.Count; i++) {
                bbuffer.WriteBytes (genDataList[i]);
            }

            System.IO.File.WriteAllBytes (Application.dataPath + "/Resources/" + animationResPrefix + AniName + ".bytes", bbuffer.ToBytes ());

            for (int i = 0; i < addCount; i++) {
                meshList.RemoveAt (meshList.Count - 1);
            }
        }
    }

    /*
    class ClipMeshData
    {
        public float timeLenth;

        ///Frame1TimePoint =0  Frame4TimePoint = 1
        public float Frame2TimePoint = 0.333f;
        public float Frame3TimePoint = 0.666f;
        //public float Frame4TimePoint = 0.75f;

        public int subMeshCount;
        public int[] subMeshTriangleLens;
        public int[] triangleBuffer;
        public float[] vertexBuffer;
        public float[] normalBuffer;
        public float[] tangentBuffer;
        public float[] uvBuffer;
        public float[] uv2Buffer; 
    }
     * 
     * */
    byte[] Make (Mesh mesh1, Mesh mesh2, Mesh mesh3, Mesh mesh4, float clipTimeLenghts, float frame2Pos, float frame3Pos) {
        Mesh[] meshs = new Mesh[] { mesh1, mesh2, mesh3, mesh4 };
        VertexAnimationResManager.ClipMeshData meshData = new VertexAnimationResManager.ClipMeshData ();
        meshData.subMeshCount = meshs[0].subMeshCount;

        int count = meshs[0].vertices.Length;
        //顶点
        if (meshs[0].vertices != null && meshs[0].vertices.Length > 0) {
            meshData.vertexBuffer = new float[count * 3];

            for (int i = 0; i < meshs[0].vertices.Length; i++) {
                meshData.vertexBuffer[i * 3] = meshs[0].vertices[i].x;
                meshData.vertexBuffer[i * 3 + 1] = meshs[0].vertices[i].y;
                meshData.vertexBuffer[i * 3 + 2] = meshs[0].vertices[i].z;
            }

            //GCHandle verSrcHand = GCHandle.Alloc(meshs[0].vertices, GCHandleType.Pinned); 
            //Marshal.Copy(verSrcHand.AddrOfPinnedObject(),meshData.vertexBuffer,  0, meshData.vertexBuffer.Length); 
            //verSrcHand.Free();  
        }
        //uv
        if (meshs[0].uv != null && meshs[0].uv.Length > 0) {
            meshData.uvBuffer = new float[count * 2];

            for (int i = 0; i < meshs[0].vertices.Length; i++) {
                meshData.uvBuffer[i * 2] = meshs[0].uv[i].x;
                meshData.uvBuffer[i * 2 + 1] = meshs[0].uv[i].y;
            }

            //GCHandle verSrcHand = GCHandle.Alloc(meshs[0].uv, GCHandleType.Pinned);
            //Marshal.Copy(verSrcHand.AddrOfPinnedObject(), meshData.uvBuffer, 0, meshData.uvBuffer.Length);
            //verSrcHand.Free();   
        }

        //法线  这里用来存动画第二帧的顶点信息
        if (meshs[1].vertices != null && meshs[1].vertices.Length > 0) {
            meshData.normalBuffer = new float[count * 3];

            for (int i = 0; i < meshs[0].vertices.Length; i++) {
                meshData.normalBuffer[i * 3] = meshs[1].vertices[i].x;
                meshData.normalBuffer[i * 3 + 1] = meshs[1].vertices[i].y;
                meshData.normalBuffer[i * 3 + 2] = meshs[1].vertices[i].z;
            }
        }

        //切线 这里用来存动画第三帧的顶点信息
        if (meshs[2].vertices != null && meshs[2].vertices.Length > 0) {
            meshData.tangentBuffer = new float[count * 4];

            for (int i = 0; i < meshs[0].vertices.Length; i++) {
                meshData.tangentBuffer[i * 4] = meshs[2].vertices[i].x;
                meshData.tangentBuffer[i * 4 + 1] = meshs[2].vertices[i].y;
                meshData.tangentBuffer[i * 4 + 2] = meshs[2].vertices[i].z;
                meshData.tangentBuffer[i * 4 + 3] = meshs[3].vertices[i].x;
            }
        }

        //UV2 用来存第四个关键帧率的 顶点YZ 坐标  X坐标由切线的W通道来存
        if (meshs[3].vertices != null && meshs[3].vertices.Length > 0) {
            meshData.uv2Buffer = new float[count * 2];

            for (int i = 0; i < meshs[0].vertices.Length; i++) {
                meshData.uv2Buffer[i * 2] = meshs[3].vertices[i].y;
                meshData.uv2Buffer[i * 2 + 1] = meshs[3].vertices[i].z;
            }

            //GCHandle verSrcHand = GCHandle.Alloc(meshs[0].uv, GCHandleType.Pinned);
            //Marshal.Copy(verSrcHand.AddrOfPinnedObject(), meshData.uvBuffer, 0, meshData.uvBuffer.Length);
            //verSrcHand.Free();   
        }

        //颜色 用来存第5个顶点信息
        //if (meshs[Indexs[4]].vertices != null && meshs[Indexs[4]].vertices.Length > 0)
        //{
        //    //颜色通道貌似没有负数,且范围为0到1  所有这里需要将模型顶点映射到[0,1]之间，映射范围为[-1,1]之间

        //    meshData.colorBuffer = new float[count * 4];
        //    for (int i = 0; i < meshs[Indexs[4]].vertices.Length; i++)
        //    {
        //        meshData.colorBuffer[i * 4] = (meshs[Indexs[4]].vertices[i].x * 0.5f) + 0.5f;
        //        meshData.colorBuffer[i * 4 + 1] = (meshs[Indexs[4]].vertices[i].y * 0.5f) + 0.5f;
        //        meshData.colorBuffer[i * 4 + 2] = (meshs[Indexs[4]].vertices[i].z * 0.5f) + 0.5f;
        //    }
        //}

        count = 0;
        int len = 0;
        meshData.subMeshTriangleLens = new int[meshData.subMeshCount];
        for (int i = 0; i < meshData.subMeshCount; i++) {
            len = meshs[0].GetTriangles (i).Length;
            count += len;
            meshData.subMeshTriangleLens[i] = len;
        }

        meshData.triangleBuffer = new int[count];

        len = 0;
        for (int i = 0; i < meshData.subMeshCount; i++) {
            meshs[0].GetTriangles (i).CopyTo (meshData.triangleBuffer, len);
            len += meshData.subMeshTriangleLens[i];
        }

        ByteBuffer bbuffer = new ByteBuffer ();
        bbuffer.WriteFloat (clipTimeLenghts);
        bbuffer.WriteFloat (frame2Pos);
        bbuffer.WriteFloat (frame3Pos);
        bbuffer.WriteInt (meshs[0].subMeshCount);

        for (int i = 0; i < meshData.subMeshTriangleLens.Length; i++) {
            bbuffer.WriteInt (meshData.subMeshTriangleLens[i]);
        }

        bbuffer.WriteInt (meshData.triangleBuffer.Length);
        for (int i = 0; i < meshData.triangleBuffer.Length; i++) {
            bbuffer.WriteInt (meshData.triangleBuffer[i]);
        }

        bbuffer.WriteInt (meshData.vertexBuffer.Length);
        for (int i = 0; i < meshData.vertexBuffer.Length; i++) {
            bbuffer.WriteFloat (meshData.vertexBuffer[i]);
        }

        bbuffer.WriteInt (meshData.normalBuffer.Length);
        for (int i = 0; i < meshData.normalBuffer.Length; i++) {
            bbuffer.WriteFloat (meshData.normalBuffer[i]);
        }

        bbuffer.WriteInt (meshData.tangentBuffer.Length);
        for (int i = 0; i < meshData.tangentBuffer.Length; i++) {
            bbuffer.WriteFloat (meshData.tangentBuffer[i]);
        }

        bbuffer.WriteInt (meshData.uvBuffer.Length);
        for (int i = 0; i < meshData.uvBuffer.Length; i++) {
            bbuffer.WriteFloat (meshData.uvBuffer[i]);
        }

        bbuffer.WriteInt (meshData.uv2Buffer.Length);
        for (int i = 0; i < meshData.uv2Buffer.Length; i++) {
            bbuffer.WriteFloat (meshData.uv2Buffer[i]);
        }
        return bbuffer.ToBytes ();
    }

    void OnGUI () {

    }
}
#endif