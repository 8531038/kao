using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class Acceleration : MonoBehaviour
{
    private static int Width = 1080; //1080(750)
    private static int Height = 2520; //1920(1334)
    private static int fps = 30;

    private Vector3 acceleration; //加速度変数

    private string savepath; // 保存先のパス
    private int directory_count; // ディレクトリの数を数える
    private string string_directory_count; // 新たに作るディレクトリの名前

    WebCamTexture webCamTexture; //カメラ
    public Color32[] color32;

    private int count = 0;//カウント

    Touch touch; //画面座標

    private string datetimeStr;
    private float time = 0.0f;

    // Use this for initialization
    void Start()
    {
        //ディレクトリ作成
        savepath = Application.persistentDataPath; // Androidで動かすとき
        directory_count = Directory.GetDirectories(savepath, "*", SearchOption.AllDirectories).Length;
        string_directory_count = (directory_count + 1).ToString();
        savepath = savepath + "/" + string_directory_count + "/";
        Directory.CreateDirectory(savepath);

        // カメラ起動
        WebCamDevice[] webCamDevice = WebCamTexture.devices;

        //webCamDevice[0] = PCカメラ, iPhone外カメラ webCamdevice[1] = iPhone内カメラ
        this.webCamTexture = new WebCamTexture(webCamDevice[1].name, Width, Height, fps);
        
        GetComponent<Renderer>().material.mainTexture = webCamTexture; // quad
        this.webCamTexture.Play();
    }

    // Update is called once per frame
    void Update()
    {
        datetimeStr = System.DateTime.Now.ToString();
        time += Time.deltaTime;

        //加速度取得
        this.acceleration = Input.acceleration;
        touch = Input.GetTouch(0);

        StartCoroutine("Score_Save"); // コルーチン
    }

        private void OnGUI()
    {
        var rect = new Rect(20, 150, 700, 100);
        GUI.skin.label.fontSize = 45;
        GUI.Label(rect, string.Format("Accel = {0:f2}", acceleration));
        rect.y = 250;
        GUI.Label(rect, string.Format("TPosition = {0:f2}", touch.position));
        rect.y = 350;
        GUI.Label(rect, string.Format("TRadius = {0:f2}", touch.radius));
        rect.y = 450;
        GUI.Label(rect, string.Format("Bright = {0:f2}", Screen.brightness.ToString()));
        rect.y = 550;
        GUI.Label(rect, string.Format("dateTime = {0:f2}", datetimeStr));
        rect.y = 650;
        GUI.Label(rect, string.Format("daltaTime = {0:f2}", time));
    }

    
    public IEnumerator Score_Save()
    {
        yield return new WaitForSeconds(1.0f);

        //画像取得
        color32 = webCamTexture.GetPixels32();
        Texture2D texture = new Texture2D(webCamTexture.width, webCamTexture.height);

        GameObject.Find("Quad").GetComponent<Renderer>().material.mainTexture = texture; //quad
        texture.SetPixels32(color32);
        texture.Apply();

        // 画像生成
        var bytes = texture.EncodeToJPG();

        //加速度
        count++;
        string namecount = count.ToString();
        string str_getdata = String.Format("Accel:{0:f6}, Position:{1:f}, Radius:{2:f6}, Bright:{3:f2}, Time:{4:f2}",
            acceleration, touch.position, touch.radius, Screen.brightness.ToString(), time);

        File.WriteAllBytes(savepath + namecount + ".jpg", bytes); // 画像書き出し
        File.WriteAllText(savepath + namecount + ".txt", str_getdata); // 加速度書き出し
    }
}