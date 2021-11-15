using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class Acc : MonoBehaviour
{
    private static int Width = 1080; //1080(750)
    private static int Height = 2520; //1920(1334)
    private static int fps = 30;

    private Vector3 acceleration; //加速度変数
    private Gyroscope gyro; // ジャイロセンサーで使用

    private float screenbrightness; // 画面の明るさの値

    private string savepath; // 保存先のパス
    private int directory_count; // ディレクトリの数を数える
    private string string_directory_count; // 新たに作るディレクトリの名前

    WebCamTexture webCamTexture; //カメラ
    public Color32[] color32;

    private int count = 0;//カウント

    private Touch touch; // 画面タッチを使用
    private float tpx, tpy, tpre, trad, delta_mag, delta_velocity; //　タッチのデータの値
    private Vector2 delta_position; // タッチの移動距離
    private string tphase;

    private string str_touchdata; // タッチデータを保存する際に使用
    private string str_touchnewdata; // 新たなタッチデータの格納

    void Awake()
    {
        if (Application.isEditor)
        {
            Application.runInBackground = true;
        }
    }


    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("<color=red>バックグラウンド行った</color>");
        }
        else
        {
            Debug.Log("<color=red>復帰した</color>");
        }
    }

    // Use this for initialization
    void Start()
    {
        //ディレクトリ作成
        savepath = Application.persistentDataPath; // Androidで動かすとき
        directory_count = Directory.GetDirectories(savepath, "*", SearchOption.AllDirectories).Length;
        string_directory_count = (directory_count + 1).ToString();
        savepath = savepath + "/" + string_directory_count + "/";
        Directory.CreateDirectory(savepath);

        // ジャイロセンサ起動
        Input.gyro.enabled = true;

        // カメラ起動
        WebCamDevice[] webCamDevice = WebCamTexture.devices;

        //webCamDevice[0] = PCカメラ, iPhone外カメラ webCamdevice[1] = iPhone内カメラ
        this.webCamTexture = new WebCamTexture(webCamDevice[1].name, Width, Height, fps);
        GetComponent<Renderer>().material.mainTexture = webCamTexture; // quad
        this.webCamTexture.Play();

        StartCoroutine("Score_Save"); // コルーチンを使用して撮影
    }

    // Update is called once per frame
    void Update()
    {
        gyro = Input.gyro; // ジャイロセンサー
        acceleration = gyro.userAcceleration; // ユーザの加速度

        screenbrightness = Screen.brightness; // デバイスの画面の明るさ

        Get_TouchData(); // タッチデータを取得
    }

    public IEnumerator Score_Save()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);

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
            string str_getdata = String.Format("Accel:{0:f6}\n Bright:{1:f2}\n",
                acceleration, screenbrightness);

            File.WriteAllBytes(savepath + "face-" + namecount + ".jpg", bytes); // 画像書き出し
            File.WriteAllText(savepath + "sensor-" + namecount + ".txt", str_getdata); // 加速度書き出し
            File.WriteAllText(savepath + "touchdata-" + namecount + ".txt", str_touchdata); // タッチ情報書き出し
            str_touchdata = "";
        }
    }

    private void Get_TouchData() // 画面のタッチ情報を取得
    {
        if (Input.touchCount > 0) // 画面に指が触れている時
        {
            touch = Input.GetTouch(0);

            /*  tpre: タッチ圧力, tpx: タッチしたx座標, tpy: タッチしたy座標, trad: タッチ半径
             *  delta_position: 1フレーム前と現在でのタッチした座標の差
             *  delta_mag: 1フレーム前と現在でのタッチした座標の距離(delta_positionの2乗の平方根と等価)
             *  delta_velocity: 1フレーム前と現在の距離から測定した速度
             */
            switch (touch.phase)
            {
                case TouchPhase.Began: // 画面が指に触れた時
                    tphase = "began";
                    if (Input.touchPressureSupported == true) tpre = touch.pressure;
                    tpx = touch.position.x;
                    tpy = touch.position.y;
                    trad = touch.radius;
                    break;
                case TouchPhase.Moved: // 画面上で指が動いた時
                    tphase = "Moved";
                    if (Input.touchPressureSupported == true) tpre = touch.pressure;
                    tpx = touch.position.x;
                    tpy = touch.position.y;
                    trad = touch.radius;
                    delta_position = new Vector2(touch.deltaPosition.x, touch.deltaPosition.y);
                    delta_mag = touch.deltaPosition.magnitude;
                    delta_velocity = delta_mag / touch.deltaTime;
                    break;
                case TouchPhase.Stationary: // 画面に指が触れているが，静止している時
                    tphase = "Stationary";
                    if (Input.touchPressureSupported == true) tpre = touch.pressure;
                    tpx = touch.position.x;
                    tpy = touch.position.y;
                    trad = touch.radius;
                    break;

                case TouchPhase.Ended: // 画面から指が離れた時
                    tphase = "Ended";
                    tpx = tpy = trad = tpre = 0f;
                    delta_position = new Vector2(0f, 0f);
                    delta_mag = 0f;
                    delta_velocity = 0f;
                    break;
            }
        }
        else // 画面に指が触れていない時
        {
            tphase = "Not touch";
            tpx = tpy = trad = tpre = 0f;
            delta_position = new Vector2(0f, 0f);
            delta_mag = delta_velocity = 0f;
        }

        // タッチ情報を書き加えていく
        str_touchnewdata = string.Format("{0}, {1:f0}, {2:f0}, {3:f5}, {4:f5}, {5:f0}, {6:f5}, {7:f5}\n"
            , tphase, tpx, tpy, trad, tpre, delta_position, delta_mag, delta_velocity);
        str_touchdata = str_touchdata + str_touchnewdata;
    }

    private void OnGUI()
    {
        var rect = new Rect(20, 150, 700, 100);
        GUI.skin.label.fontSize = 45;
        GUI.Label(rect, string.Format("Accel = {0:f2}", acceleration));
        rect.y = 250;
        GUI.Label(rect, string.Format("touchX = {0:f2}", tpx));
        rect.y = 350;
        GUI.Label(rect, string.Format("touchY = {0:f2}", tpy));
        rect.y = 450;
        GUI.Label(rect, string.Format("Bright = {0:f2}", screenbrightness));
    }
}