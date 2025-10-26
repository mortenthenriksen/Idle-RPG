using Godot;

public partial class DraggableContent : Control
{
    private bool _dragging = false;
    private Vector2 _dragOffset;
    private Panel _panel;

    public override void _Ready()
    {
        _panel = GetParent<Panel>();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
        {
            _dragging = mouseButton.Pressed;
            if (_dragging)
                _dragOffset = GetGlobalMousePosition() - GlobalPosition;
        }

        if (@event is InputEventMouseMotion mouseMotion && _dragging)
        {
            Vector2 newPosition = GetGlobalMousePosition() - _dragOffset;

            Rect2 panelRect = _panel.GetGlobalRect();
            Rect2 contentRect = GetGlobalRect();

            // Compute min/max positions for both axes
            float minX = panelRect.Position.X;
            float maxX = panelRect.Position.X + panelRect.Size.X - contentRect.Size.X;
            float minY = panelRect.Position.Y;
            float maxY = panelRect.Position.Y + panelRect.Size.Y - contentRect.Size.Y;

            // Protect against inverted ranges
            if (minX > maxX) (minX, maxX) = (maxX, minX);
            if (minY > maxY) (minY, maxY) = (maxY, minY);

            // Clamp both X and Y
            newPosition.X = Mathf.Clamp(newPosition.X, minX, maxX);
            newPosition.Y = Mathf.Clamp(newPosition.Y, minY, maxY);

            GlobalPosition = newPosition;
        }
    }
}
