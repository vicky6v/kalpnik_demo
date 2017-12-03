using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json.Linq;

public class BackEnd : MonoBehaviour
{

    List<string> contentListOfNames = new List<string>();
    List<string> contentListOfImages = new List<string>();
    public List<string> contentListOfImagesNames = new List<string>();

   
    public string gameDataProjectFilePath = "";
    private void SaveGameData(String tempdata)
    {
        gameDataProjectFilePath = Application.persistentDataPath + "/"+dataFileName;
        StreamWriter file;

        if (!File.Exists(gameDataProjectFilePath))
        {
            file = File.CreateText(gameDataProjectFilePath);
            file.WriteLine(tempdata);
            file.Close();

        }
       

    }
    public List<Texture2D> tex;
    WWW www;
    int count = 0;
    IEnumerator downloadImages(List<string> imgPath)
    {
         www = new WWW(imgPath[count]);
        yield return new WaitUntil(() => www.isDone == true);


        if (www.isDone)
        {
            Debug.Log("is done called");
            if (www.bytes != null)
            {

                if (!File.Exists(Application.persistentDataPath + "/" + Path.GetFileName(imgPath[count])))
                {
                    File.WriteAllBytes(Application.persistentDataPath + "/" + Path.GetFileName(imgPath[count]), www.bytes);
                    tex.Add(www.texture);

                    count++;
                    if ((count < imgPath.Count))
                    {
                        StartCoroutine(downloadImages(contentListOfImages));

                    }

                }
                else
                {
                    tex.Add(LoadPNG(Application.persistentDataPath + "/" + Path.GetFileName(imgPath[count])));
                    count++;
                    if ((count < imgPath.Count))
                    {
                        StartCoroutine(downloadImages(contentListOfImages));

                    }
                }
            }
        }



    }
    public Texture2D LoadPNG(string filePath)
    {

        Texture2D tex1 = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex1 = new Texture2D(2, 2);
            tex1.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex1;
    }
   
    string jsonData = "";
    string tempdebug = "";
    String dataFileName = "";
    IEnumerator DownloadDB()
    {
        tex = new List<Texture2D>();
        tex.Clear();

        gameDataProjectFilePath = Application.persistentDataPath + "/"+ dataFileName;
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("Error. Check internet connection!");
            tempdebug = "Not connected to internet";

        }
        else
        {
            tempdebug = "Connected to internet";

        }

        if (File.Exists(gameDataProjectFilePath) && Application.internetReachability == NetworkReachability.NotReachable)
        {
            jsonData = File.ReadAllText(gameDataProjectFilePath);
            JObject jo = JObject.Parse(jsonData);
            //  Dictionary<string, object> dictObj = jo.ToObject<Dictionary<string, object>>();

            for (int i = 0; i < 4; i++)
            {
                contentListOfNames.Add(jo["contents"][i]["name"].ToString());
                contentListOfImages.Add(jo["contents"][i]["thumbnailId"].ToString());
                contentListOfImagesNames.Add(Path.GetFileName(jo["contents"][i]["thumbnailId"].ToString()));
                //  print("file name = "+Path.GetFileName(jo["contents"][i]["thumbnailId"].ToString()));
            }
            count = 0;
            StartCoroutine(downloadImages(contentListOfImages));

        }
        else
        {

            WWW www = new WWW(url);
            yield return www;
            if (www.error == null && www.isDone)
            {

                jsonData = www.text;
                SaveGameData(jsonData);
                JObject jo = JObject.Parse(www.text);

                for (int i = 0; i < 4; i++)
                {
                    contentListOfNames.Add(jo["contents"][i]["name"].ToString());
                    contentListOfImages.Add(jo["contents"][i]["thumbnailId"].ToString());
                    contentListOfImagesNames.Add(Path.GetFileName(jo["contents"][i]["thumbnailId"].ToString()));
                    ///  print("file name = " + Path.GetFileName(jo["contents"][i]["thumbnailId"].ToString()));
                }


                count = 0;
                StartCoroutine(downloadImages(contentListOfImages));

            }
        }

    }
    public float nativeWidth = 1080.0f;
    public float nativeHeight = 1920.0f;
    float rx, ry;
    bool showButtons = false;
    void OnGUI()
    {
        rx = Screen.width / nativeWidth;
        ry = Screen.height / nativeHeight;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(rx, ry, 1));
        GUI.DrawTexture(new Rect(0, 0, nativeWidth, nativeHeight),Texture2D.whiteTexture, ScaleMode.StretchToFill);
        GUI.skin.label.fontSize = 40;
        GUI.skin.button.fontSize = 40;

        GUI.skin.label.normal.textColor = Color.black;
        GUI.skin.button.normal.textColor = Color.black;

        if (!showButtons)
        {
           
            if (GUI.Button(new Rect(0, 200, nativeWidth, 200), "Category")){
                onClick(0);
             }
            if (GUI.Button(new Rect(0, 500, nativeWidth, 200), "Featured"))
            {
                onClick(1);


            }
            if (GUI.Button(new Rect(0, 800, nativeWidth, 200), "something"))
            {

                onClick(2);

            }


        }
        else if (tex.Count > 0)
        {
     
            DrawObjectItems();
           
        }
        GUI.Label(new Rect(0, Screen.height - 100, Screen.width, 200), "Status:" + tempdebug);
        if (GUI.Button(new Rect(Screen.width - 200, Screen.height - 100, 200, 100), "Exit"))
        {
            Application.Quit();
        }

    }
    string url = "";

    void onClick(int i)
    {
        string url0 = "https://s3.ap-south-1.amazonaws.com/unity-code-sample-content/category.json";
        string url1 = "https://s3.ap-south-1.amazonaws.com/unity-code-sample-content/featured.json";
        string url2 = "https://s3.ap-south-1.amazonaws.com/unity-code-sample-content/top.json";
        switch (i)
        {
            case 0:
                url = url0;
                break;
            case 1:
                url = url1;
                break;
            case 2:
                url = url2;
                break;

        }
        print("jasddhfjhds" + Path.GetFileName(url));
        dataFileName = Path.GetFileName(url);
        StartCoroutine(DownloadDB());
        showButtons = true;
    }
    
    void DrawObjectItems()
    {
       

        for (int i = 0; i < contentListOfImagesNames.Count; i++)
        {
            GUI.DrawTexture(new Rect(0, i* 440, nativeWidth, 500), tex[i]);
            GUI.Label(new Rect(0, i * 440, nativeWidth/2, 200), contentListOfNames[i]);
           
          
        }


    }
}
