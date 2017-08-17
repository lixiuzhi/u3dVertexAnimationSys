using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class VertexAnimation : MonoBehaviour
{

    string CurrAinResName = string.Empty;
    // public Texture2D TextureMap;
    public VertexAnimationResManager.VertexAnimationClipInfo currClipInfo = null;

    public MeshFilter meshFilter;
    public MeshRenderer meshRender;
    public Material material;

    public float speed = 1;
    //当前播放动作时间
    public float currPlayPos = 0;
    //当前片段开始时间
    public float currClipBeginPos = 0;
    //下一个片段开始时间
    public float nextClipPos = 0;
    public int currClipOffsetIndex = 0;

    public string defaultAnimName = "Gongji";
    public string animationResPrefix = "";
    public List<string> animationNames = new List<string>();

    void Awake()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRender = gameObject.GetComponent<MeshRenderer>();
        material = meshRender.material;
        Play(defaultAnimName);
    }

    public void Play(string name)
    {
        string newResName = animationResPrefix + name;
        if (CurrAinResName == newResName)
        {
            return;
        }
        CurrAinResName = newResName;
        currClipInfo = VertexAnimationResManager.Singleton.GetAnimationMeshInfo(newResName);
        if (currClipInfo == null)
            return;

        meshFilter.mesh = currClipInfo.clipMeshs[0]; 
        currPlayPos = 0;
        currClipBeginPos = 0;
        nextClipPos = currClipInfo.clipLenghts[0];
        currClipOffsetIndex = 0;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    bool isChangeMesh = false;
    void LateUpdate()
    {
        if (currClipInfo == null || currClipInfo.clipLenghts.Count == 0)
            return;
        isChangeMesh = false;
        if (currPlayPos >= currClipInfo.clipTotalTimeLen)
        {
            currPlayPos = 0.001f * (((int)(currPlayPos * 1000)) % ((int)(1000 * currClipInfo.clipTotalTimeLen)));

            float ft = currPlayPos;
            for(int i=0;i< currClipInfo.clipLenghts.Count;i++)
            {
                if(ft < currClipInfo.clipLenghts[i])
                {
                    if (i > 0)
                    {
                        currClipBeginPos = nextClipPos; 
                        nextClipPos += currClipInfo.clipLenghts[i];
                    }
                    else
                    {
                        currClipBeginPos = 0;
                        nextClipPos = currClipInfo.clipLenghts[i];
                    }
                    currClipOffsetIndex = i;
                    break;
                }
                ft -= currClipInfo.clipLenghts[i];
            }
             
            if (currClipInfo.clipMeshs.Count > 1)
                isChangeMesh = true;
        }

        ///判断是否切换mesh
        if (currClipInfo.clipMeshs.Count > 1)
        {
            if (currPlayPos > nextClipPos)
            {
                isChangeMesh = true;
                currClipOffsetIndex++;
                if (currClipOffsetIndex >= currClipInfo.clipLenghts.Count)
                {
                    currClipOffsetIndex = 0;
                    currClipBeginPos = 0;
                    nextClipPos = currClipInfo.clipLenghts[0];
                }
                else
                {
                    currClipBeginPos = nextClipPos;
                    nextClipPos += currClipInfo.clipLenghts[currClipOffsetIndex];
                }
            }
        }

        if (isChangeMesh)
        {
            Mesh mesh = currClipInfo.clipMeshs[currClipOffsetIndex];
            // if(meshFilter.mesh!=mesh)
            {
                meshFilter.mesh = mesh;
            }
            Vector3 v3 = currClipInfo.everyClipFrameTimePoints[currClipOffsetIndex];
            material.SetFloat("_Frame2Time", v3.x);
            material.SetFloat("_Frame3Time", v3.y);
            //material.SetFloat("Frame4Time", v3.z);
        }
        //material.SetFloat("_CurTime", (currPlayPos - currClipBeginPos) / (nextClipPos - currClipBeginPos));
        //时间转换成当前片段的0-1内
        material.SetFloat("_CurTime", (currPlayPos - currClipBeginPos) / (nextClipPos - currClipBeginPos));

        currPlayPos += Time.deltaTime * speed;
    }
}
