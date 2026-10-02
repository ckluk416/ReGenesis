using UnityEngine;

namespace ReGenesis
{
    // Lets world input (placement node clicks) ignore clicks that land on the HUD.
    public static class UIPointer
    {
        public static bool IsOverUI { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            IsOverUI = false;
        }
    }
}
