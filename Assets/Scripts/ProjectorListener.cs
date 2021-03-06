using UnityEngine;
using System.Collections;

public class ProjectorListener : MonoBehaviour {

	public string arm;

	private WebsocketClient wsc;
	string topic;
	public int framerate = 15;
	public string compression = "none"; //"png" is the other option, haven't tried it yet though

	Projector projector;
	Texture2D texture;
	Color black = new Color (0f, 0f, 0f, 0f);

	// Use this for initialization
	void Start () {
		
		projector = GetComponent<Projector> ();

		texture = new Texture2D(2, 2, TextureFormat.ASTC_4x4, false);
		texture.wrapMode = TextureWrapMode.Clamp;
		projector.material.SetTexture("_ShadowTex", texture);


		GameObject wso = GameObject.FindWithTag ("WebsocketTag");
		wsc = wso.GetComponent<WebsocketClient> ();
		topic = "cameras/" + arm + "_hand_camera/image_compressed/compressed";
		wsc.Subscribe (topic, "sensor_msgs/CompressedImage", compression, framerate);

		Invoke ("attachToGripper", 0.2f); // wait a bit to make sure the gripper object has been made

		InvokeRepeating ("renderTexture", .5f, 1.0f/framerate);
	}

	void attachToGripper() {
		GameObject gripper = GameObject.Find (arm + "_gripper");
		this.transform.SetParent (gripper.transform);
		this.transform.localPosition = new Vector3 (0f, 0f, 0f);
		this.transform.localEulerAngles = new Vector3 (-90f, 0f, -90f);
	}
	
	// Update is called once per frame
	void Update () {
		//testTexture ();
	}

	void renderTexture() {
		string message = wsc.messages[topic];
		byte[] image = System.Convert.FromBase64String(message);

		texture.LoadImage (image);

		//give the texture a black border or the shader gives weird artifacts
		for (int i = 0; i < texture.width; i++) {
			for (int j = 0; j < 1; j++) {
				texture.SetPixel (i, j, black);
				texture.SetPixel (i, texture.height - j, black);
			}
		}
		for (int i = 0; i < texture.height; i++) {
			texture.SetPixel (0, i, black);
			texture.SetPixel (texture.width, i, black);
		}

		texture.Apply();

	}
}
