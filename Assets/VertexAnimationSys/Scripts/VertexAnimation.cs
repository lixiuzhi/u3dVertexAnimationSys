using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class VertexAnimation : MonoBehaviour
{
    public string animationResPrefix = "";
    public TextAsset[] clipDatas;

    public string curAinResName = string.Empty;
    public VertexAnimationResManager.VertexAnimationClipInfo currClipInfo = null;
    public MeshFilter meshFilter;
    public MeshRenderer meshRender;
    public float speed = 1;
    //当前播放动作时间
    public float curPlayPos = 0;
    //当前片段开始时间
    public float curClipBeginPos = 0;
    //下一个片段开始时间
    public float nextClipPos = 0;
    public int curClipOffsetIndex = 0;
    public string defaultAnimName = "idle";
    float frame2Time;
    float frame3Time;
    float curTime = 0;
    int frame2TimeShaderId;
    int frame3TimeShaderId;
    int curTimeShaderId;
    MaterialPropertyBlock prop;
    //test
    public bool randStartPos = false;
    public bool loop = false;

    void Awake()
    {
        prop = new MaterialPropertyBlock();

        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRender = gameObject.GetComponent<MeshRenderer>();
        //material = meshRender.material;
        var shader = meshRender.sharedMaterial.shader;
        curTimeShaderId = shader.GetPropertyNameId(shader.FindPropertyIndex("_CurTime"));
        frame2TimeShaderId = shader.GetPropertyNameId(shader.FindPropertyIndex("_Frame2Time"));
        frame3TimeShaderId = shader.GetPropertyNameId(shader.FindPropertyIndex("_Frame3Time"));

        if (randStartPos)
        {
            curPlayPos = UnityEngine.Random.Range(0, 0.2f);
        }
        Play(defaultAnimName, speed, loop);
    }
    public void Play(string name, float speed = 1, bool loop = true)
    {
        if (clipDatas == null)
            return;
        this.speed = speed;
        this.loop = loop;
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
        curPlayPos = 0;
        curClipBeginPos = 0;
        nextClipPos = currClipInfo.clipLenghts[0];
        curClipOffsetIndex = 0;
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
        curPlayPos += Time.deltaTime * speed;
        if (curPlayPos > currClipInfo.clipTotalTimeLen)
        {
            if (loop)
            {
                curPlayPos = 0.001f * (((int)(curPlayPos * 1000)) % ((int)(1000 * currClipInfo.clipTotalTimeLen)));
                float ft = curPlayPos;
                for (int i = 0; i < currClipInfo.clipLenghts.Count; i++)
                {
                    if (ft < currClipInfo.clipLenghts[i])
                    {
                        if (i > 0)
                        {
                            curClipBeginPos = nextClipPos;
                            nextClipPos += currClipInfo.clipLenghts[i];
                        }
                        else
                        {
                            curClipBeginPos = 0;
                            nextClipPos = currClipInfo.clipLenghts[i];
                        }
                        curClipOffsetIndex = i;
                        break;
                    }
                    ft -= currClipInfo.clipLenghts[i];
                }
            }
            else
            {
                curPlayPos = currClipInfo.clipTotalTimeLen;
                curClipBeginPos = nextClipPos = curPlayPos;
            }

            if (currClipInfo.clipMeshs.Count > 1)
                isChangeMesh = true;
        }

        ///判断是否切换mesh
        if (currClipInfo.clipMeshs.Count > 1)
        {
            if (curPlayPos > nextClipPos)
            {
                isChangeMesh = true;
                curClipOffsetIndex++;
                if (curClipOffsetIndex >= currClipInfo.clipLenghts.Count)
                {
                    curClipOffsetIndex = 0;
                    curClipBeginPos = 0;
                    nextClipPos = currClipInfo.clipLenghts[0];
                }
                else
                {
                    curClipBeginPos = nextClipPos;
                    nextClipPos += currClipInfo.clipLenghts[curClipOffsetIndex];
                }
            }
        }

        if (isChangeMesh)
        {
            var mesh = currClipInfo.clipMeshs[curClipOffsetIndex];
            meshFilter.mesh = mesh;
            Vector3 v3 = currClipInfo.everyClipFrameTimePoints[curClipOffsetIndex];
            frame2Time = v3.x;
            frame3Time = v3.y;
        }

        if (nextClipPos - curClipBeginPos > 0)
            curTime = (curPlayPos - curClipBeginPos) / (nextClipPos - curClipBeginPos);
        else
            curTime = 1;

        prop.SetFloat(curTimeShaderId, curTime);
        prop.SetFloat(frame2TimeShaderId, frame2Time);
        prop.SetFloat(frame3TimeShaderId, frame3Time);
        meshRender.SetPropertyBlock(prop);
    }

}
