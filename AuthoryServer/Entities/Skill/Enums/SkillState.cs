namespace AuthoryServer.Entities
{
    public enum SkillState : byte
    {
        None = 0,
        OnCasting = 1,
        OnCasted = 2,
        OnMoving = 3,
        OnHit = 4,
        Interrupted = 5,
        Handled = 6,
        OnCooldown = 7,
    }
}
