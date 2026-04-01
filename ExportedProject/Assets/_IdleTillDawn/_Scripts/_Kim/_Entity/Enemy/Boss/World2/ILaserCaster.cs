public interface ILaserCaster
{
    bool IsFiringLaser { get; }
    void FireSpinLaser(float duration);
    bool CheckLaserCooldown();
}
