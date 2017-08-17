using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;

public class VertexAnimationResManager : SingletonTemplate<VertexAnimationResManager>
{
    public class ByteBufferReader
    {
        MemoryStream stream = null;
        BinaryWriter writer = null;
        BinaryReader reader = null;

        public ByteBufferReader()
        {
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
        }

        public ByteBufferReader(byte[] data)
        {
            if (data != null)
            {
                stream = new MemoryStream(data);
                reader = new BinaryReader(stream);
            }
            else
            {
                stream = new MemoryStream();
                writer = new BinaryWriter(stream);
            }
        }

        public void Close()
        {
            if (writer != null) writer.Close();
            if (reader != null) reader.Close();

            stream.Close();
            writer = null;
            reader = null;
            stream = null;
        } 

        public int ReadInt()
        {
            return (int)reader.ReadInt32();
        } 
        public float ReadFloat()
        {
            return reader.ReadSingle();
        } 
    }
    public class ClipMeshData
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
        //public float[] colorBuffer;

        public Mesh GenMesh()
        { 
            Mesh mesh = new Mesh();

            int vertexCount = vertexBuffer.Length / 3;

            mesh.subMeshCount = subMeshCount;
            //顶点
            Vector3[] vertexs = new Vector3[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                vertexs[i] = new Vector3(vertexBuffer[i * 3], vertexBuffer[i * 3 + 1], vertexBuffer[i * 3 + 2]);
            }
            mesh.vertices = vertexs; 
            //uv
            Vector2[] uv = new Vector2[vertexCount];
            for (int i = 0; i < uv.Length; i++)
            {
                uv[i] = new Vector2(uvBuffer[i * 2], uvBuffer[i * 2 + 1]);
            }
            mesh.uv = uv;
            //uv2
            Vector2[] uv2 = new Vector2[vertexCount];
            for (int i = 0; i < uv.Length; i++)
            {
                uv2[i] = new Vector2(uv2Buffer[i * 2], uv2Buffer[i * 2 + 1]);
            }
            mesh.uv2 = uv2;

            //法线 
            Vector3[] normals = new Vector3[vertexCount];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = new Vector3(normalBuffer[i * 3], normalBuffer[i * 3 + 1], normalBuffer[i * 3 + 2]);
            } 
            mesh.normals = normals;

            //切线
            var tangents = new Vector4[vertexCount]; 
            for (int i = 0; i < tangents.Length; i++)
            {
                tangents[i] = new Vector4(tangentBuffer[i * 4], tangentBuffer[i * 4 + 1], tangentBuffer[i * 4 + 2], tangentBuffer[i * 4 + 3]);
            } 
            mesh.tangents = tangents;
            ////颜色
            //Color[] colors = new Color[colorBuffer.Length / 4];
            //for (int i = 0; i < colors.Length; i++)
            //{
            //    colors[i] = new Vector4(colorBuffer[i * 4], colorBuffer[i * 4 + 1], colorBuffer[i * 4 + 2], 1);
            //}
            //mesh.colors = colors;

            //三角形 
            int startIndex = 0;
            int bufferLen = 0; 

            for (int i = 0; i < subMeshCount; i++)
            {
                bufferLen = subMeshTriangleLens[i];
                if (bufferLen <= 0) continue;
                var triIndexBuffer = new int[bufferLen];
                Array.Copy(triangleBuffer, startIndex, triIndexBuffer, 0, bufferLen);
                mesh.SetTriangles(triIndexBuffer, i);
                startIndex += bufferLen;
            }

            subMeshTriangleLens =null;
            triangleBuffer = null;
            vertexBuffer = null;
            normalBuffer = null;
            tangentBuffer = null;
            uvBuffer = null;
            uv2Buffer = null;

            return mesh;
        }
    }

    /// <summary>
    /// 一个动画 由多个动画片段构成，每个动画片段的长度， 每个片段里的5帧对应的时间点（区间[0,1]）
    /// </summary>
    [Serializable]
    public class VertexAnimationClipInfo
    {
        public float clipTotalTimeLen = 0;
        public List<Mesh> clipMeshs = new List<Mesh>();
        public List<Vector2> everyClipFrameTimePoints = new List<Vector2>();
        public List<float> clipLenghts = new List<float>();
    }

    public Dictionary<string,VertexAnimationClipInfo> AnimationClipInfos  = new Dictionary<string,VertexAnimationClipInfo>();
 

    ClipMeshData GetMeshData(ByteBufferReader bbuffer)
    {
        ClipMeshData meshData = new ClipMeshData(); 

        meshData.timeLenth = bbuffer.ReadFloat();
        meshData.Frame2TimePoint = bbuffer.ReadFloat();
        meshData.Frame3TimePoint = bbuffer.ReadFloat();
       // meshData.Frame4TimePoint = bbuffer.ReadFloat(); 

        meshData.subMeshCount = bbuffer.ReadInt();
        meshData.subMeshTriangleLens = new int[meshData.subMeshCount];
        for (int m = 0; m < meshData.subMeshCount; m++)
        {
            meshData.subMeshTriangleLens[m] = bbuffer.ReadInt();
        }

        int triangleBufferCount = bbuffer.ReadInt();
        meshData.triangleBuffer = new int[triangleBufferCount];
        for (int m = 0; m < triangleBufferCount; m++)
        {
            meshData.triangleBuffer[m] = bbuffer.ReadInt();
        } 

        int vertexBufferCount = bbuffer.ReadInt();
        meshData.vertexBuffer = new float[vertexBufferCount];
        for (int m = 0; m < vertexBufferCount; m++)
        {
            meshData.vertexBuffer[m] = bbuffer.ReadFloat();
        }

        int normalBufferCount = bbuffer.ReadInt();
        meshData.normalBuffer = new float[normalBufferCount];
        for (int m = 0; m < normalBufferCount; m++)
        {
            meshData.normalBuffer[m] = bbuffer.ReadFloat();
        } 

        int tangentBufferCount = bbuffer.ReadInt();
        meshData.tangentBuffer = new float[tangentBufferCount];
        for (int m = 0; m < tangentBufferCount; m++)
        {
            meshData.tangentBuffer[m] = bbuffer.ReadFloat();
        }

        int uvBufferCount = bbuffer.ReadInt();
        meshData.uvBuffer = new float[uvBufferCount];
        for (int m = 0; m < uvBufferCount; m++)
        {
            meshData.uvBuffer[m] = bbuffer.ReadFloat();
        }
         
        int uv2BufferCount = bbuffer.ReadInt();
        meshData.uv2Buffer = new float[uv2BufferCount];
        for (int m = 0; m < uv2BufferCount; m++)
        {
            meshData.uv2Buffer[m] = bbuffer.ReadFloat();
        }
         
        //int colorBufferCount = bbuffer.ReadInt();
        //meshData.colorBuffer = new float[colorBufferCount];
        //for (int m = 0; m < colorBufferCount; m++)
        //{
        //    meshData.colorBuffer[m] = bbuffer.ReadFloat();
        //}

        return meshData;
    }

    VertexAnimationClipInfo getOrAddAnimationInfo(string aniResName)
    {
        aniResName = aniResName.ToLower();
        VertexAnimationClipInfo clipInfo = null;

        AnimationClipInfos.TryGetValue(aniResName, out clipInfo);

        if(clipInfo != null)
        { 
            return clipInfo; 
        }

        var asset = Resources.Load<TextAsset>(aniResName);
        byte[] clipData=null; 
        if (asset != null)
        {
            clipData = Resources.Load<TextAsset>(aniResName).bytes;
        }
        if (clipData == null)
        {
            Debug.LogError("animation clip data is null:" + aniResName);
            return null;
        }

        clipInfo = new VertexAnimationClipInfo();

        ByteBufferReader bbuffer = new ByteBufferReader(clipData);
        int Count = bbuffer.ReadInt();

        for (int i = 0; i < Count; i++)
        {
            ClipMeshData meshData = GetMeshData(bbuffer);
            clipInfo.clipTotalTimeLen += meshData.timeLenth;
            clipInfo.clipLenghts.Add(meshData.timeLenth);
            clipInfo.everyClipFrameTimePoints.Add(new Vector2(meshData.Frame2TimePoint, meshData.Frame3TimePoint)); //,meshData.Frame4TimePoint
            clipInfo.clipMeshs.Add(meshData.GenMesh());
        }

        bbuffer.Close();
        clipData = null;

        AnimationClipInfos.Add(aniResName, clipInfo);
        return clipInfo;
    }

    public VertexAnimationClipInfo GetAnimationMeshInfo(string resname)
    { 
        return getOrAddAnimationInfo(resname);
    }
}
