

using UnityEngine;


public class Main : MonoBehaviour
{
    private int paintUVPropertyID;
    private int brushTexturePropertyID;
    private int brushScalePropertyID;
    private int brushRotatePropertyID;
    private int brushColorPropertyID;

    const float brushSize = 0.15f;  


    public Camera myCamera;
    public SpriteRenderer mySpriteRender;   //Sprite Show
    public Texture2D sourceTexture;     //原始图像
    public Material MatComputDefault;   //用来涂色计算的材质，Shaer
    public Material MatBrush;   //刷子材质，为了获取贴图
    public Color nowColor = Color.red;  //刷的颜色


    RenderTexture myRenderTextureMain;  //当前的一直在变动的myRenderTextureMain
    Texture2D myTexture;    //当前一直在变动的Texture2D
    Material computMainMat;     //创建的计算表现的
    Texture brushMainTexture;    //刷子形状贴图

    Vector3 screenPos;
    Vector3 worldPos;
    int paperWidth;
    int paperHeight;
    private void Awake()
    {
        paintUVPropertyID = Shader.PropertyToID("_PaintUV");
        brushTexturePropertyID = Shader.PropertyToID("_Brush");
        brushScalePropertyID = Shader.PropertyToID("_BrushScale");
        brushRotatePropertyID = Shader.PropertyToID("_BrushRotate");
        brushColorPropertyID = Shader.PropertyToID("_ControlColor");

        paperWidth = sourceTexture.width;
        paperHeight = sourceTexture.height;

        myTexture = new Texture2D(paperWidth, paperHeight, TextureFormat.ARGB32, false);
        myRenderTextureMain = new RenderTexture(paperWidth, paperHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

        //把底图复制到RenderTexture和Texture，显示用。
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = myRenderTextureMain;
        // Copy your texture ref to the render texture
        Graphics.Blit(sourceTexture, myRenderTextureMain);
        myTexture.ReadPixels(new Rect(0, 0, paperWidth, paperHeight), 0, 0);
        myTexture.Apply();
        RenderTexture.active = currentRT;

        mySpriteRender.sprite = Sprite.Create(myTexture, new Rect(0f, 0f, paperWidth, paperHeight), new Vector2(0.5f, 0.5f));


        //创建运行时的计算材质
        computMainMat = new Material(MatComputDefault.shader);
        computMainMat.CopyPropertiesFromMaterial(MatComputDefault);
        //刷子贴图
        brushMainTexture = MatBrush.GetTexture("_MainTex");


    }
    public void Brushing(float x, float y)
    {
        screenPos.x = x;
        screenPos.y = y;
        screenPos.z = Mathf.Abs(myCamera.transform.position.z);
        DrawPaper();
    }

    float drawTime;
    static Vector3 uvAdd = new Vector3(0.5f, 0.5f, 0f);
    RenderTexture tmpBuffer;
    private void DrawPaper()
    {
        if (Time.time < drawTime)
            return;
        drawTime = Time.time + 0.05f;    //1秒画30次
        worldPos = myCamera.ScreenToWorldPoint(screenPos);

        Vector3 paperAt = mySpriteRender.transform.InverseTransformPoint(worldPos);   //模型局部坐标
        paperAt.x /= mySpriteRender.bounds.size.x;    //根据面片模型计算uv位置
        paperAt.y /= mySpriteRender.bounds.size.y;
        paperAt += uvAdd;  //
        //Debug.Log("worldPos:" + worldPos.ToString() + ",uv:" + paperAt.ToString("F5") + ",sz:" + drawRenderUV.bounds.size.ToString("F5"));
        //Debug.Log("lastColor:" + lastColor.ToString() + ",mixColor:" + mixColor + ",nowColor:" + nowColor);
        if (paperAt.x > 1f || paperAt.x < 0f)
            return;
        if (paperAt.y > 1f || paperAt.y < 0f)
            return;

        float rotate = UnityEngine.Random.value * 360f;
        tmpBuffer = RenderTexture.GetTemporary(paperWidth, paperHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

        SetBrushData(paperAt, brushMainTexture, brushSize, rotate, nowColor);
        Graphics.Blit(myRenderTextureMain, tmpBuffer, computMainMat);
        Graphics.Blit(tmpBuffer, myRenderTextureMain);
        

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = myRenderTextureMain;
        myTexture.ReadPixels(new Rect(0, 0, paperWidth, paperHeight), 0, 0);
        myTexture.Apply();
        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(tmpBuffer);

    }

    void SetBrushData(Vector2 _uv, Texture _txture, float _scale, float _rot, Color _c)
    {
        computMainMat.SetVector(paintUVPropertyID, _uv);
        computMainMat.SetTexture(brushTexturePropertyID, _txture);
        computMainMat.SetFloat(brushScalePropertyID, _scale);
        computMainMat.SetFloat(brushRotatePropertyID, _rot);
        computMainMat.SetVector(brushColorPropertyID, _c);
    }


    bool mousedown;
    private void Update()
    {
        
        if (Input.GetMouseButton(0))
        {
            Vector3 at = Input.mousePosition;
            Debug.Log(at);
            Brushing(at.x, at.y);
        }
    }
}
