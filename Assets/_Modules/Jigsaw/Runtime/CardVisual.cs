using UnityEngine;

public class CardVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private int defaultOrderLayer;

    private void Start()
    {
        this.spriteRenderer.sortingOrder = this.defaultOrderLayer;
    }

    public void SetSpriteVisual(Sprite sprite)
    {
        this.spriteRenderer.sprite = sprite;
    }

    public void SetOrderLayer(int orderLayer)
    {
        this.spriteRenderer.sortingOrder = this.defaultOrderLayer;
    }

    public Bounds GetBounds()
    {
        return this.spriteRenderer.bounds;
    }
}