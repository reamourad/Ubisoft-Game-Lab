using System;
using UnityEngine;
using UnityEngine.UI;

public class Scanline : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 10;
    
    private static readonly int DetailTex = Shader.PropertyToID("_DetailTex");
    private Image targetImage;
    private Material material;

    private float yOffset=0;
    
    private void Start()
    {
        targetImage = GetComponent<Image>();
        material = targetImage.material;
    }

    // Update is called once per frame
    void Update()
    {
        yOffset += 0.1f * scrollSpeed * Time.deltaTime;
        yOffset = yOffset % 100f;
        var vec2 = new Vector2(0, yOffset);
        material.SetTextureOffset(DetailTex, vec2);
    }
}
