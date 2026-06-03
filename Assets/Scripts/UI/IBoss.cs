public interface IBoss
{
    float MaxHealth { get; }
    float CurrentHealth { get; }
    bool IsDead { get; }
}