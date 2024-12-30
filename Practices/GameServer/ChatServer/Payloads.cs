namespace ChatServer
{
    //      TCP          UDP
    // 연결지향        비연결지향
    // 전송보장        전송보장X
    // 전송순서보장    전송순서보장X
    // 느림           빠름
    // 
    // TCP 가 전송을 보장할수있는 이유 : Client 연결 요청 -> Server 의 수락 -> Client 의 전송 하는 3-handshake 형태로 로직을 구성하기때문
    // TCP 가 전송순서를 보장할수있는 이유 : Segment 에 Sequence Number 를 붙여 보내기 때문. 받은측에서 버퍼에 모아뒀다가 Sequence Number 에 따라 재조립을함.
    // 만약에 Sequence Number 가 누락된게 있따면 송신측에서 재전송


    /// <summary>
    /// 데이터를 수신한 쪽에서 어떤 타입의 객체로 데이터를 Deserialize 해야하는지 명시해주기위해서
    /// </summary>
    public enum PayloadType : ushort
    {
        None,

        // Tcp
        ConnectedResponse,
        ChatMessage,

        // Udp
        NetworkObjectTransformUpdate = 10001,
    }

    public static class PayloadFactory
    {
        public static IPayload Create(PayloadType payloadType)
        {
            IPayload payload = default;

            switch (payloadType)
            {
                case PayloadType.ConnectedResponse:
                    payload = new ConnectedResponse();
                    break;
                case PayloadType.ChatMessage:
                    break;
                case PayloadType.NetworkObjectTransformUpdate:
                    break;
                default:
                    throw new Exception();
            }

            return payload;
        }
    }

    public interface IPayload
    {
        PayloadType PayloadType { get; }


        byte[] Serialize();
        void Deserialize(byte[] bytes);

        // 가비지컬렉션 및 생성 오버헤드 최소화하기위한 오버로드
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }

    public class ConnectedResponse : IPayload
    {
        public PayloadType PayloadType => PayloadType.ConnectedResponse;


        public string Message;
        public int ClientId;


        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                Serialize(writer);
                return stream.ToArray();
            }
        }

        public void Deserialize(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Deserialize(reader);
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Message);
            writer.Write(ClientId);
        }

        public void Deserialize(BinaryReader reader)
        {
            Message = reader.ReadString();
            ClientId = reader.ReadInt32();
        }
    }


    public struct Vector3
    {
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        public float x, y, z;
    }

    public struct Quaternion
    {
        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public float x, y, z, w;
    }

    public class NetworkObjectTransformUpdate : IPayload
    {
        public PayloadType PayloadType => PayloadType.NetworkObjectTransformUpdate;


        public int ClientId;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;


        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                Serialize(writer);
                return stream.ToArray();
            }
        }

        public void Deserialize(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Deserialize(reader);
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ClientId);
            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write(Position.z);
            writer.Write(Rotation.x);
            writer.Write(Rotation.y);
            writer.Write(Rotation.z);
            writer.Write(Rotation.w);
            writer.Write(Scale.x);
            writer.Write(Scale.y);
            writer.Write(Scale.z);
        }

        public void Deserialize(BinaryReader reader)
        {
            ClientId = reader.ReadInt32();
            Position.x = reader.ReadSingle();
            Position.y = reader.ReadSingle();
            Position.z = reader.ReadSingle();
            Rotation.x = reader.ReadSingle();
            Rotation.y = reader.ReadSingle();
            Rotation.z = reader.ReadSingle();
            Rotation.w = reader.ReadSingle();
            Scale.x = reader.ReadSingle();
            Scale.y = reader.ReadSingle();
            Scale.z = reader.ReadSingle();
        }
    }
}
