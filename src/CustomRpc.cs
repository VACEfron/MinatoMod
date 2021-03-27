using Hazel;
using Reactor;

namespace MinatoMod
{
    [RegisterCustomRpc]
    public class CustomRpc : PlayerCustomRpc<MinatoMod, CustomRpc.Data>
    {
        public CustomRpc(MinatoMod plugin) : base(plugin)
        {
        }

        public readonly struct Data
        {
            public readonly byte PlayerId;
            public readonly CustomRpcType RpcType;

            public Data(byte? playerId, CustomRpcType rpcType)
            {
                PlayerId = playerId ?? byte.MaxValue;
                RpcType = rpcType;
            }
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.PlayerId);
            writer.Write((int)data.RpcType);
        }

        public override Data Read(MessageReader reader)
        {
            return new Data(reader.ReadByte(), (CustomRpcType)reader.ReadInt32());
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            if (innerNetObject == null)
                return;

            switch (data.RpcType)
            {
                case CustomRpcType.SetMinato:
                    Utils.MinatoPlayer = Utils.GetPlayerById(data.PlayerId);
                    Minato.SetMinatoButtons();
                    break;
                case CustomRpcType.SetMinatoTarget:
                    Utils.MinatoTarget = data.PlayerId != byte.MaxValue ? Utils.GetPlayerById(data.PlayerId) : null;
                    break;
            }
        }

        public static void HandleCustomRpc(byte? playerId, CustomRpcType rpcType)
        {
            Rpc<CustomRpc>.Instance.Send(new Data(playerId, rpcType));
        }

        public enum CustomRpcType
        {
            SetMinato,
            SetMinatoTarget,
            Reset
        }
    }
}
