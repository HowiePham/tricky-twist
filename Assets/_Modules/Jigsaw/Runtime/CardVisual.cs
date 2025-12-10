using UnityEngine;

public class CardVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private int defaultOrderLayer;
    [SerializeField] private int priorityOrderLayer;

    private void Start()
    {
        this.spriteRenderer.sortingOrder = this.defaultOrderLayer;
    }

    public void SetSpriteVisual(Sprite sprite)
    {
        this.spriteRenderer.sprite = sprite;
    }

    public void PrioritizeOrderLayer()
    {
        this.spriteRenderer.sortingOrder = this.priorityOrderLayer;
    }

    public void ResetOrderLayer()
    {
        this.spriteRenderer.sortingOrder = this.defaultOrderLayer;
    }

    public Bounds GetBounds()
    {
        return this.spriteRenderer.bounds;
    }
}