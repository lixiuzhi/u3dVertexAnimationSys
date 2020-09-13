using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class VertexAnimation : MonoBehaviour
{

    string curAinResName = string.Empty; 
    public VertexAnimationResManager.VertexAnimationClipInfo currClipInfo = null;

    public MeshFilter meshFilter;
    public MeshRenderer meshRender; 

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

    
    float _Frame2Time;

    float _Frame3Time;
    float _CurTime=0;
    int _Frame2TimeShaderId;

    int _Frame3TimeShaderId;
    int _CurTimeShaderId;
    MaterialPropertyBlock prop;

    //test
    public bool randStartPos = true;
 
    void Awake()
    {
        prop = new MaterialPropertyBlock();

        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRender = gameObject.GetComponent<MeshRenderer>();
        //material = meshRender.material;
        Play(defaultAnimName); 
        var shader = meshRender.sharedMaterial.shader;
        _CurTimeShaderId = shader.GetPropertyNameId(shader.FindPropertyIndex("_CurTime"));
        _Frame2TimeShaderId =  shader.GetPropertyNameId(shader.FindPropertyIndex("_Frame2Time"));
        _Frame3TimeShaderId =  shader.GetPropertyNameId(shader.FindPropertyIndex("_Frame3Time"));

        if(randStartPos){
            currPlayPos = UnityEngine.Random.Range(0,0.6f);
        }
    }
    public void Play(string name)
    {
        string newResName = animationResPrefix + name;
        if (curAinResName == newResName)
        {
            return;
        }
        curAinResName = newResName;
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
            meshFilter.mesh = mesh; 
            Vector3 v3 = currClipInfo.everyClipFrameTimePoints[currClipOffsetIndex]; 
            _Frame2Time = v3.x;
            _Frame3Time = v3.y;
        } 
        _CurTime = (currPlayPos - currClipBeginPos) / (nextClipPos - currClipBeginPos);

        currPlayPos += Time.deltaTime * speed; 
        prop.SetFloat(_CurTimeShaderId,_CurTime);
        prop.SetFloat(_Frame2TimeShaderId,_Frame2Time);
        prop.SetFloat(_Frame3TimeShaderId,_Frame3Time);
        meshRender.SetPropertyBlock(prop);     
    }

}
