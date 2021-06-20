using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Threading;
using UnityEditor.PackageManager;

public class DepthRosGeometryView : MonoBehaviour {

    private WebsocketClient wsc;
    string depthTopic;
    string colorTopic;
    int framerate = 100;
    public string compression = "none"; //"png" is the other option, haven't tried it yet though
    string depthMessage;
    string colorMessage;

    public Material Material;
    Texture2D depthTexture;
    Texture2D colorTexture;

    int width = 512;
    int height = 424;

    Matrix4x4 m;

    // Use this for initialization
    void Start() {
        // Create a texture for the depth image and color image
        depthTexture = new Texture2D(width, height, TextureFormat.R16, false);
        colorTexture = new Texture2D(2, 2);

        wsc = GameObject.Find("WebsocketClient").GetComponent<WebsocketClient>();
        depthTopic = "h9camera/sd/image_depth_rect";
        colorTopic = "h9camera/sd/image_color_rect/compressed";
        SubscribeDepth();
        SubscribeColor();
        InvokeRepeating("UpdateTexture", 0.1f, 0.1f);
    }

    void SubscribeDepth()
    {
        wsc.Subscribe(depthTopic, "sensor_msgs/Image", compression, framerate);
    }

    void SubscribeColor()
    {
        wsc.Subscribe(colorTopic, "sensor_msgs/CompressedImage", compression, framerate);
    }

    // Update is called once per frame
    void UpdateTexture() {
        try {
            depthMessage = wsc.messages[depthTopic];
            byte[] depthImage = System.Convert.FromBase64String(depthMessage);

            depthTexture.LoadRawTextureData(depthImage);
            //depthTexture.LoadImage(depthImage);
            depthTexture.Apply();
            //Debug.Log(depthTexture.GetType());

        }
        catch (Exception e) {
            Debug.LogError(e.ToString());
            if (!wsc.messages.Keys.Contains(depthTopic))
            {
                SubscribeDepth();
                return;
            }
        }

        try {
            colorMessage = wsc.messages[colorTopic];
            byte[] colorImage = System.Convert.FromBase64String(colorMessage);
            colorTexture.LoadImage(colorImage);
            colorTexture.Apply();
        }
        catch (Exception e) {
            Debug.LogError(e.ToString());
            if (!wsc.messages.Keys.Contains(colorTopic))
            {
                SubscribeColor();
                return;
            }
        }
    }

    void OnRenderObject() {

        Material.SetTexture("_MainTex", depthTexture);
        Material.SetTexture("_ColorTex", colorTexture);
        Material.SetPass(0);

        m = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
        Material.SetMatrix("transformationMatrix", m);

        Graphics.DrawProceduralNow(MeshTopology.Points, 512 * 424, 1);
    }
}