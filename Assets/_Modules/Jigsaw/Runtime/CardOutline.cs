using UnityEngine;

public class CardOutline : MonoBehaviour
{
    [Header("Outline Settings")] [SerializeField]
    private Color outlineColor = Color.white;

    [SerializeField] private float outlineWidth = 0.05f;
    [SerializeField] private int sortingOrder = 10;

    [Header("References")] [SerializeField]
    private SpriteRenderer cardSpriteRenderer;

    private LineRenderer topLine;
    private LineRenderer bottomLine;
    private LineRenderer leftLine;
    private LineRenderer rightLine;

    private void Awake()
    {
        CreateOutlineLines();
    }

    private void CreateOutlineLines()
    {
        this.topLine = CreateLine("TopLine");
        this.bottomLine = CreateLine("BottomLine");
        this.leftLine = CreateLine("LeftLine");
        this.rightLine = CreateLine("RightLine");

        UpdateOutlinePositions();
        SetAllSidesActive(true);
    }

    private LineRenderer CreateLine(string lineName)
    {
        var lineObj = new GameObject(lineName);
        lineObj.transform.SetParent(this.transform);
        lineObj.transform.localPosition = Vector3.zero;

        var line = lineObj.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = this.outlineColor;
        line.endColor = this.outlineColor;
        line.startWidth = this.outlineWidth;
        line.endWidth = this.outlineWidth;
        line.positionCount = 2;
        line.sortingOrder = this.sortingOrder;
        line.useWorldSpace = false;

        return line;
    }

    public void UpdateOutlinePositions()
    {
        if (this.cardSpriteRenderer == null)
        {
            return;
        }

        Bounds bounds = this.cardSpriteRenderer.bounds;
        Vector3 center = this.transform.InverseTransformPoint(bounds.center);
        Vector3 size = bounds.size;

        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;

        this.topLine.SetPosition(0, new Vector3(center.x - halfWidth, center.y + halfHeight, 0));
        this.topLine.SetPosition(1, new Vector3(center.x + halfWidth, center.y + halfHeight, 0));

        this.bottomLine.SetPosition(0, new Vector3(center.x - halfWidth, center.y - halfHeight, 0));
        this.bottomLine.SetPosition(1, new Vector3(center.x + halfWidth, center.y - halfHeight, 0));

        this.leftLine.SetPosition(0, new Vector3(center.x - halfWidth, center.y - halfHeight, 0));
        this.leftLine.SetPosition(1, new Vector3(center.x - halfWidth, center.y + halfHeight, 0));

        this.rightLine.SetPosition(0, new Vector3(center.x + halfWidth, center.y - halfHeight, 0));
        this.rightLine.SetPosition(1, new Vector3(center.x + halfWidth, center.y + halfHeight, 0));
    }

    public void SetSideActive(Direction direction, bool active)
    {
        switch (direction)
        {
            case Direction.Up:
                this.topLine.enabled = active;
                break;
            case Direction.Down:
                this.bottomLine.enabled = active;
                break;
            case Direction.Left:
                this.leftLine.enabled = active;
                break;
            case Direction.Right:
                this.rightLine.enabled = active;
                break;
        }
    }

    public void SetAllSidesActive(bool active)
    {
        this.topLine.enabled = active;
        this.bottomLine.enabled = active;
        this.leftLine.enabled = active;
        this.rightLine.enabled = active;
    }
}