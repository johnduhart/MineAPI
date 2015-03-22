namespace MineAPI.Protocol.Packets
{
    public interface IStateChangePacket
    {
        NetworkState NewState { get; }
    }
}