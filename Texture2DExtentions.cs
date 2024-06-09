using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Texture2DExtensions
{
    public static Texture2D GetResized(this Texture2D texture, int width, int height)
    {
        // ���T�C�Y��̃T�C�Y������RenderTexture���쐬���ď�������
        var rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(texture, rt);

        // ���T�C�Y��̃T�C�Y������Texture2D���쐬����RenderTexture���珑������
        var preRT = RenderTexture.active;
        RenderTexture.active = rt;
        var ret = new Texture2D(width, height);
        ret.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        ret.Apply();
        RenderTexture.active = preRT;
        RenderTexture.ReleaseTemporary(rt);
        UnityEngine.Object.Destroy(texture);//@@@ �������[���[�N���ӁI
        return ret;
    }
}

