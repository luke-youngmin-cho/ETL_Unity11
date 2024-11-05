namespace ARP.MinimapSystems
{
    /// <summary>
    /// 기기의 위도, 경도, 고도 정보
    /// </summary>
    public interface IGPS
    {
        /// <summary>
        /// 위도
        /// </summary>
        float latitude { get; }

        /// <summary>
        /// 경도
        /// </summary>
        float longitude { get; }

        /// <summary>
        /// 고도
        /// </summary>
        float altitude { get; }
    }
}
