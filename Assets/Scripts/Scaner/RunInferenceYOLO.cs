using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.Services.CloudSave;

public class RunInferenceYOLO : MonoBehaviour
{
    int score = 0;
    int count = 0;
    public Button butCool;
    public Button butShot;
    public Button butRestart;
    public GameObject Controler;
    private WebCamTexture webCamTexture;
    public TextMeshProUGUI displayCount;
    public GameObject Main;

    public RawImage displayImage;
    public NNModel srcModel;
    public TextAsset labelsAsset;
    public Dropdown backendDropdown;
    public Transform displayLocation;
    public Font font;
    public float confidenceThreshold = 0.25f;
    public float iouThreshold = 0.45f;

    private Texture2D inputImage;
    private Model model;
    private IWorker engine;
    private Dictionary<string, Tensor> inputs = new Dictionary<string, Tensor>();
    private string[] labels;
    private RenderTexture targetRT;
    private const int amountOfClasses = 80;
    private const int box20Sections = 20;
    private const int box40Sections = 40;
    private const int anchorBatchSize = 85;
    private const int inputResolutionX = 640;
    private const int inputResolutionY = 640;
    //model output returns box scales relative to the anchor boxes, 3 are used for 40x40 outputs and other 3 for 20x20 outputs,
    //each cell has 3 boxes 3x85=255
    private readonly float[] anchors = { 10, 14, 23, 27, 37, 58, 81, 82, 135, 169, 344, 319 };

    //box struct with the original output data
    public struct Box
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public string label;
        public int anchorIndex;
        public int cellIndexX;
        public int cellIndexY;
    }

    //restructured data with pixel units
    public struct PixelBox
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public string label;
    }

    public void Start()
    {
        if (PlayerPrefs.HasKey("BottleCount")) score = PlayerPrefs.GetInt("BottleCount");
        count = 0;
        webCamTexture = new WebCamTexture(1920, 1080);
        webCamTexture.Play();
        displayImage.GetComponent<RectTransform>().sizeDelta = new Vector2(webCamTexture.width, webCamTexture.height);
        displayImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(250 + (webCamTexture.width - 640) / 2, (webCamTexture.height - 640) / 2);
        displayImage.texture = webCamTexture;
    }

    public void Shot()
    {
        Texture2D photo = new Texture2D(640, 640, TextureFormat.ARGB32, false);
        photo.SetPixels(0, 0, 640, 640, webCamTexture.GetPixels(0, 0, 640, 640));
        photo.Apply();
        displayImage.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 640);
        displayImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(250, 0);
        inputImage = photo;
        displayImage.texture = photo;
        webCamTexture.Stop();

        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        labels = labelsAsset.text.Split('\n'); //parse neural net labels
        //load model
        model = ModelLoader.Load(srcModel);
        ExecuteML();

        butShot.gameObject.SetActive(false);
        butRestart.gameObject.SetActive(true);
        butCool.gameObject.SetActive(true);
    }

    public async void Cool()
    {
        displayCount.text = "0";
        score += count;
        PlayerPrefs.SetInt("BottleCount", score);
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> { { "BottleCount", score.ToString() } });
        Main.GetComponent<LoadScore>().Load();
    }

    public void ExecuteML()
    {
        count = 0;
        ClearAnnotations();

        displayImage.texture = inputImage;

        engine = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, model);

        //preprocess image for input
        var input = new Tensor((inputImage), 3);
        engine.Execute(input);
        //read output tensors
        var output20 = engine.PeekOutput("016_convolutional"); //016_convolutional = original output tensor name for 20x20 boundingBoxes
        var output40 = engine.PeekOutput("023_convolutional"); //023_convolutional = original output tensor name for 40x40 boundingBoxes

        List<Box> outputBoxList = new List<Box>(); //this list is used to store the original model output data
        List<PixelBox> pixelBoxList = new List<PixelBox>(); //this list is used to store the values converted to intuitive pixel data
        outputBoxList = DecodeOutput(output20, output40); //decode the output
        pixelBoxList = ConvertBoxToPixelData(outputBoxList); //convert output to intuitive pixel data (x,y coords from the center of the image; height and width in pixels)
        pixelBoxList = NonMaxSuppression(pixelBoxList); //non max suppression (remove overlapping objects)

        //draw bounding boxes
        for (int i = 0; i < pixelBoxList.Count; i++)
        {
            if (pixelBoxList[i].label == "bottle")
            {
                count++;
                DrawBox(pixelBoxList[i]);
            }
        }
        displayCount.text = count.ToString();

        //clean memory
        input.Dispose();
        engine.Dispose();
        Resources.UnloadUnusedAssets();
    }

    public List<Box> DecodeOutput(Tensor output20, Tensor output40)
    {
        List<Box> outputBoxList = new List<Box>();

        //decode results into a list for each output(20x20 and 40x40), anchor mask selects the output box presets (first 3 or the last 3 presets) 
        outputBoxList = DecodeYolo(outputBoxList, output40, box40Sections, 0);
        outputBoxList = DecodeYolo(outputBoxList, output20, box20Sections, 3);

        return outputBoxList;
    }

    public List<Box> DecodeYolo(List<Box> outputBoxList, Tensor output, int boxSections, int anchorMask)
    {
        for (int boundingBoxX = 0; boundingBoxX < boxSections; boundingBoxX++)
        {
            for (int boundingBoxY = 0; boundingBoxY < boxSections; boundingBoxY++)
            {
                for (int anchor = 0; anchor < 3; anchor++)
                {
                    if (output[0, boundingBoxX, boundingBoxY, anchor * anchorBatchSize + 4] > confidenceThreshold)
                    {
                        //identify the best class
                        float bestValue = 0;
                        int bestIndex = 0;
                        for (int i = 0; i < amountOfClasses; i++)
                        {
                            float value = output[0, boundingBoxX, boundingBoxY, anchor * anchorBatchSize + 5 + i];
                            if (value > bestValue)
                            {
                                bestValue = value;
                                bestIndex = i;
                            }
                        }
                        //Debug.Log(labels[bestIndex]);
                        Box tempBox;
                        tempBox.x = output[0, boundingBoxX, boundingBoxY, anchor * anchorBatchSize];
                        tempBox.y = output[0, boundingBoxX, boundingBoxY, anchor * anchorBatchSize + 1];
                        tempBox.width = output[0, boundingBoxX, boundingBoxY, anchor * anchorBatchSize + 2];
                        tempBox.height = output[0, boundingBoxX, boundingBoxY, anchor * anchorBatchSize + 3];
                        tempBox.label = labels[bestIndex];
                        tempBox.anchorIndex = anchor + anchorMask;
                        tempBox.cellIndexY = boundingBoxX;
                        tempBox.cellIndexX = boundingBoxY;
                        outputBoxList.Add(tempBox);

                    }
                }
            }
        }
        return outputBoxList;
    }

    public List<PixelBox> ConvertBoxToPixelData(List<Box> boxList)
    {
        List<PixelBox> pixelBoxList = new List<PixelBox>();
        for (int i = 0; i < boxList.Count; i++)
        {
            PixelBox tempBox;

            //apply anchor mask, each output uses a different preset box
            var boxSections = boxList[i].anchorIndex > 2 ? box20Sections : box40Sections;

            //move marker to the edge of the picture -> move to the center of the cell -> add cell offset (cell size * amount of cells) -> add scale
            tempBox.x = (float)(-inputResolutionX * 0.5) + inputResolutionX / boxSections * 0.5f +
                        inputResolutionX / boxSections * boxList[i].cellIndexX + Sigmoid(boxList[i].x);
            tempBox.y = (float)(-inputResolutionY * 0.5) + inputResolutionX / boxSections * 0.5f +
                          inputResolutionX / boxSections * boxList[i].cellIndexY + Sigmoid(boxList[i].y);

            //select the anchor box and multiply it by scale
            tempBox.width = anchors[boxList[i].anchorIndex * 2] * (float)Math.Pow(Math.E, boxList[i].width);
            tempBox.height = anchors[boxList[i].anchorIndex * 2 + 1] * (float)Math.Pow(Math.E, boxList[i].height);
            tempBox.label = boxList[i].label;
            pixelBoxList.Add(tempBox);
        }

        return pixelBoxList;
    }

    public List<PixelBox> NonMaxSuppression(List<PixelBox> boxList)
    {
        for (int i = 0; i < boxList.Count - 1; i++)
        {
            for (int j = i + 1; j < boxList.Count; j++)
            {
                if (IntersectionOverUnion(boxList[i], boxList[j]) > iouThreshold && boxList[i].label == boxList[j].label)
                {
                    boxList.RemoveAt(i);
                }
            }
        }
        return boxList;
    }

    public float IntersectionOverUnion(PixelBox box1, PixelBox box2)
    {
        //top left and bottom right corners of two rectangles
        float b1x1 = box1.x - 0.5f * box1.width;
        float b1x2 = box1.x + 0.5f * box1.width;
        float b1y1 = box1.y - 0.5f * box1.height;
        float b1y2 = box1.y + 0.5f * box1.height;
        float b2x1 = box2.x - 0.5f * box2.width;
        float b2x2 = box2.x + 0.5f * box2.width;
        float b2y1 = box2.y - 0.5f * box2.height;
        float b2y2 = box2.y + 0.5f * box2.height;

        //intersection rectangle
        float xLeft = Math.Max(b1x1, b2x1);
        float yTop = Math.Max(b1y1, b2y1);
        float xRight = Math.Max(b1x2, b2x2);
        float yBottom = Math.Max(b1y2, b2y2);

        //check if intersection rectangle exist
        if (xRight < xLeft || yBottom < yTop)
        {
            return 0.0f;
        }

        float intersectionArea = (xRight - xLeft) * (yBottom - yTop);
        float b1area = (b1x2 - b1x1) * (b1y2 - b1y1);
        float b2area = (b2x2 - b2x1) * (b2y2 - b2y1);
        return intersectionArea / (b1area + b2area - intersectionArea);
    }

    public float Sigmoid(float value)
    {
        return 1.0f / (1.0f + (float)Math.Exp(-value));
    }

    public void DrawBox(PixelBox box)
    {
        //add bounding box
        GameObject panel = new GameObject("ObjectBox");
        panel.AddComponent<CanvasRenderer>();
        Image img = panel.AddComponent<Image>();
        img.color = new Color(254, 0, 0, 0.3f);
        panel.transform.SetParent(displayLocation, false);
        panel.transform.localPosition = new Vector3(box.x, -box.y);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(box.width, box.height);

        //add class label
        /*GameObject text = new GameObject("ObjectLabel");
        text.AddComponent<CanvasRenderer>();
        Text txt = text.AddComponent<Text>();
        text.transform.SetParent(panel.transform, false);
        txt.text = box.label;
        txt.color = new Color(1,0,0,1);
        txt.fontSize = 40;
        txt.font = font;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        RectTransform rt2 = text.GetComponent<RectTransform>();
        rt2.offsetMin = new Vector2(20, rt2.offsetMin.y);
        rt2.offsetMax = new Vector2(0, rt2.offsetMax.y);
        rt2.offsetMax = new Vector2(rt2.offsetMax.x, 0);
        rt2.offsetMin = new Vector2(rt2.offsetMin.x, 0);
        rt2.anchorMin = new Vector2(0,0);
        rt2.anchorMax = new Vector2(1, 1);*/
    }

    public void ClearAnnotations()
    {
        foreach (Transform child in displayLocation)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnDestroy()
    {
        engine?.Dispose();

        foreach (var key in inputs.Keys)
        {
            inputs[key].Dispose();
        }

        inputs.Clear();
    }
}
