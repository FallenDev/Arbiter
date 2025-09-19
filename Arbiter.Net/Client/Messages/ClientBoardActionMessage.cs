using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.BoardAction)]
public class ClientBoardActionMessage : ClientMessage
{
    public MessageBoardAction Action { get; set; }
    public ushort? BoardId { get; set; }
    public short? StartPostId { get; set; }
    public byte? Unknown { get; set; }
    public short? PostId { get; set; }
    public MessageBoardNavigation? Navigation { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? Recipient { get; set; }
    public string? MailSubject { get; set; }
    public string? MailBody { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Action = (MessageBoardAction)reader.ReadByte();

        if (Action == MessageBoardAction.ViewBoard)
        {
            BoardId = reader.ReadUInt16();
            StartPostId = reader.ReadInt16();
            Unknown = reader.ReadByte();
        }
        else if (Action == MessageBoardAction.ViewPost)
        {
            BoardId = reader.ReadUInt16();
            PostId = reader.ReadInt16();
            Navigation = (MessageBoardNavigation)reader.ReadSByte();
        }
        else if (Action == MessageBoardAction.CreatePost)
        {
            BoardId = reader.ReadUInt16();
            Subject = reader.ReadString8();
            Body = reader.ReadString16();
        }
        else if (Action == MessageBoardAction.DeletePost)
        {
            BoardId = reader.ReadUInt16();
            PostId = reader.ReadInt16();
        }
        else if (Action == MessageBoardAction.SendMail)
        {
            BoardId = reader.ReadUInt16();
            Recipient = reader.ReadString8();
            MailSubject = reader.ReadString8();
            MailBody = reader.ReadString16();
        }
        else if (Action == MessageBoardAction.HighlightPost)
        {
            BoardId = reader.ReadUInt16();
            PostId = reader.ReadInt16();
        }
    }
    
    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}