using System;
using System.Collections.Generic;
using MineAPI.Common;
using MineAPI.Protocol.IO;

namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x38, PacketDirection.Clientbound)]
    public struct PlayerListItemPacket : IManualPacket
    {
        public PlayerListAction Action;
        public IList<IPlayerListItem> PlayerList; 

        public void ReadPacket(IMinecraftStreamReader reader)
        {
            Action = (PlayerListAction)reader.ReadVarInt();
            var count = reader.ReadVarInt();
            PlayerList = new List<IPlayerListItem>(count);

            for (int i = 0; i < count; i++)
            {
                var listItem = CreatePlayerListItem(Action);
                listItem.Uuid = new PlayerUuid(reader.ReadByteArray(16));
                listItem.FromReader(reader);

                PlayerList.Add(listItem);
            }
        }

        public void WritePacket(IMinecraftStreamWriter writer)
        {
            writer.WriteVarInt((int) Action);
            writer.WriteVarInt(PlayerList.Count);

            foreach (var listItem in PlayerList)
            {
                writer.WriteByteArray(listItem.Uuid.ToByteArray());
                listItem.ToStream(writer);
            }
        }

        private static IPlayerListItem CreatePlayerListItem(PlayerListAction action)
        {
            switch (action)
            {
                case PlayerListAction.AddPlayer:
                    return new PlayerListItemActionAddPlayer();
                case PlayerListAction.UpdateGamemode:
                    return new PlayerListItemActionUpdateGamemode();
                case PlayerListAction.UpdateLatency:
                    return new PlayerListItemActionUpdateLatency();
                case PlayerListAction.UpdateDisplayName:
                    return new PlayerListItemActionUpdateDisplayName();
                case PlayerListAction.RemovePlayer:
                    return new PlayerListItemActionRemovePlayer();
                default:
                    throw new ArgumentException("Invalid Player List Action passed");
            }
        }
    }

    public enum PlayerListAction
    {
        AddPlayer = 0,
        UpdateGamemode = 1,
        UpdateLatency = 2,
        UpdateDisplayName = 3,
        RemovePlayer = 4
    }

    public interface IPlayerListItem
    {
        PlayerUuid Uuid { get; set; }
        IPlayerListItem FromReader(IMinecraftStreamReader reader);
        void ToStream(IMinecraftStreamWriter writer);
    }

    public struct Properties
    {
        public string Name;
        public string Value;
        public bool IsSigned;
        public string Signature;
    }

    public class PlayerListActionProperties
    {
        private readonly List<Properties> _entries;

        public PlayerListActionProperties()
        {
            _entries = new List<Properties>();
        }

        public int Count
        {
            get { return _entries.Count; }
        }

        public Properties this[int index]
        {
            get { return _entries[index]; }
            set { _entries.Insert(index, value); }
        }

        public static PlayerListActionProperties FromReader(IMinecraftStreamReader reader)
        {
            var count = reader.ReadVarInt();

            var value = new PlayerListActionProperties();
            for (var i = 0; i < count; i++)
            {
                var property = new Properties();

                property.Name = reader.ReadString();
                property.Value = reader.ReadString();
                property.IsSigned = reader.ReadBool();

                if (property.IsSigned)
                    property.Signature = reader.ReadString();

                value[i] = property;
            }

            return value;
        }

        public void ToStream(IMinecraftStreamWriter stream)
        {
            stream.WriteVarInt(Count);

            foreach (var entry in _entries)
            {
                stream.WriteString(entry.Name);
                stream.WriteString(entry.Value);
                stream.WriteBool(entry.IsSigned);
                if (entry.IsSigned)
                    stream.WriteString(entry.Signature);
            }
        }
    }

    public struct PlayerListItemActionAddPlayer : IPlayerListItem
    {
        public string Name;
        public PlayerListActionProperties Properties;
        public int Gamemode;
        public int Ping;
        public bool HasDisplayName;
        public string DisplayName;

        public PlayerUuid Uuid { get; set; }

        public IPlayerListItem FromReader(IMinecraftStreamReader reader)
        {
            Name = reader.ReadString();
            Properties = PlayerListActionProperties.FromReader(reader);

            Gamemode = reader.ReadVarInt();
            Ping = reader.ReadVarInt();
            HasDisplayName = reader.ReadBool();
            if (HasDisplayName)
                DisplayName = reader.ReadString();

            return this;
        }

        public void ToStream(IMinecraftStreamWriter stream)
        {
            stream.WriteString(Name);
            Properties.ToStream(stream);
            stream.WriteVarInt(Gamemode);
            stream.WriteVarInt(Ping);
            stream.WriteBool(HasDisplayName);
            if (HasDisplayName)
                stream.WriteString(DisplayName);

        }
    }

    public struct PlayerListItemActionUpdateGamemode : IPlayerListItem
    {
        public int Gamemode;

        public PlayerUuid Uuid { get; set; }

        public IPlayerListItem FromReader(IMinecraftStreamReader reader)
        {
            Gamemode = reader.ReadVarInt();

            return this;
        }

        public void ToStream(IMinecraftStreamWriter stream)
        {
            stream.WriteVarInt(Gamemode);
        }
    }

    public struct PlayerListItemActionUpdateLatency : IPlayerListItem
    {
        public int Ping;

        public PlayerUuid Uuid { get; set; }

        public IPlayerListItem FromReader(IMinecraftStreamReader reader)
        {
            Ping = reader.ReadVarInt();

            return this;
        }

        public void ToStream(IMinecraftStreamWriter stream)
        {
            stream.WriteVarInt(Ping);
        }
    }

    public struct PlayerListItemActionUpdateDisplayName : IPlayerListItem
    {
        public bool HasDisplayName;
        public string DisplayName;

        public PlayerUuid Uuid { get; set; }

        public IPlayerListItem FromReader(IMinecraftStreamReader reader)
        {
            HasDisplayName = reader.ReadBool();
            DisplayName = reader.ReadString();

            return this;
        }

        public void ToStream(IMinecraftStreamWriter stream)
        {
            stream.WriteBool(HasDisplayName);
            stream.WriteString(DisplayName);
        }
    }

    public struct PlayerListItemActionRemovePlayer : IPlayerListItem
    {
        public PlayerUuid Uuid { get; set; }

        public IPlayerListItem FromReader(IMinecraftStreamReader reader)
        {
            return this;
        }

        public void ToStream(IMinecraftStreamWriter stream)
        {
        }
    }
}