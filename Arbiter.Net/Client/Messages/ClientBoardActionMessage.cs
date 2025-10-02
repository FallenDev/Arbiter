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
        base.Serialize(builder);
        builder.AppendByte((byte)Action);
        if (Action == MessageBoardAction.ViewBoard)
        {
            builder.AppendUInt16(BoardId ?? 0);
            builder.AppendInt16(StartPostId ?? 0);
            builder.AppendByte(Unknown ?? 0);
        }
        else if (Action == MessageBoardAction.ViewPost)
        {
            builder.AppendUInt16(BoardId ?? 0);
            builder.AppendInt16(PostId ?? 0);
            builder.AppendSByte((sbyte)(Navigation ?? 0));
        }
        else if (Action == MessageBoardAction.CreatePost)
        {
            builder.AppendUInt16(BoardId ?? 0);
            builder.AppendString8(Subject ?? string.Empty);
            builder.AppendString16(Body ?? string.Empty);
        }
        else if (Action == MessageBoardAction.DeletePost)
        {
            builder.AppendUInt16(BoardId ?? 0);
            builder.AppendInt16(PostId ?? 0);
        }
        else if (Action == MessageBoardAction.SendMail)
        {
            builder.AppendUInt16(BoardId ?? 0);
            builder.AppendString8(Recipient ?? string.Empty);
            builder.AppendString8(MailSubject ?? string.Empty);
            builder.AppendString16(MailBody ?? string.Empty);
        }
        else if (Action == MessageBoardAction.HighlightPost)
        {
            builder.AppendUInt16(BoardId ?? 0);
            builder.AppendInt16(PostId ?? 0);
        }
    }
}