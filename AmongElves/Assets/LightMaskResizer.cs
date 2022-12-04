using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMaskResizer : MonoBehaviour
{

    private Material material;
    private int screenRatioPropId;
    private Vector2 initialScreenSize;

    // Start is called before the first frame update
    void Start()
    {
        var meshRenderer = this.GetComponent<MeshRenderer>();
        material = meshRenderer.material;
        screenRatioPropId = Shader.PropertyToID("_ScreenRatio");
        initialScreenSize = new Vector2(Screen.width, Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        
        material.SetVector(screenRatioPropId, new Vector4(Screen.width / initialScreenSize.x / Screen.height * initialScreenSize.y, 1.0f));
    }
}
