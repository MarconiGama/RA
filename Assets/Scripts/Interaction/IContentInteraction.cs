using System;

public interface IContentInteraction
{
    event Action<InteractionResult> Completed;
    bool IsActive { get; }
    void Activate();
    void Deactivate();
    void Demonstrate();
}
