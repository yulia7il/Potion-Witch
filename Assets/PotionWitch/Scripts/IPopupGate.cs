// Optional gating interface for PopupOpener. Gameplay scripts (e.g. PlantPot)
// implement this to control when a popup is allowed to open. If no gate is
// set on a PopupOpener, the popup opens unconditionally on click.
public interface IPopupGate
{
    bool CanOpen();
}
