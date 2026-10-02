namespace ReGenesis
{
    // Anything a tower or enemy can damage without knowing its concrete type.
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(float amount);
    }
}
