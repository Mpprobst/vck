using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    [SerializeField] SpriteRenderer frame, roof, door, windows, lawn, fence, sky;
    [SerializeField] Sprite[] frameSprites, roofSprites, doorSprites, windowSprites, lawnSprites, fenceSprites, skySprites;
    [SerializeField] Color[] frameColors, roofColors, doorColors, windowColors, lawnColors, fenceColors;

    // Start is called before the first frame update
    void Start()
    {
        RandomizeSprite(frame, frameSprites, frameColors);
        RandomizeSprite(roof, roofSprites, frameColors);
        RandomizeSprite(door, doorSprites, doorColors);
        RandomizeSprite(windows, windowSprites, doorColors);
        RandomizeSprite(lawn, lawnSprites, lawnColors);
        RandomizeSprite(fence, fenceSprites, fenceColors);
        RandomizeSprite(sky, skySprites, lawnColors);
    }

    private void RandomizeSprite(SpriteRenderer rend, Sprite[] sprites, Color[] colors)
    {
        if (sprites.Length > 0)
            rend.sprite = sprites[Random.Range(0, sprites.Length)];
        if (colors.Length > 0)
            rend.color = colors[Random.Range(0, colors.Length)];
    }
}
