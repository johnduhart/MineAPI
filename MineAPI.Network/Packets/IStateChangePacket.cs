namespace MineAPI.Network.Packets
{
    public interface IStateChangePacket
    {
        NetworkState NewState { get; }
    }
}