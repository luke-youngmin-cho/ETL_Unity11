using UnityEngine;

namespace ARP.MinimapSystems
{
    public class MockGPS : MonoBehaviour, IGPS
    {
        public float latitude { get; } = 37.5138649f;

        public float longitude { get; } = 127.0295296f;

        public float altitude { get; } = 0f;
    }
}