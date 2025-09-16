using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.BoardResult)]
public class ServerBoardResultMessage : ServerMessage
{
    public MessageBoardResult ResultType { get; set; }
    public List<ServerMessageBoardInfo> Boards { get; set; } = [];
    public MessageBoardSource? Source { get; set; }
    public ushort? BoardId { get; set; }
    public string? BoardName { get; set; }
    public List<ServerMessageBoardPost> Posts { get; set; } = [];
    public bool? ResultSuccess { get; set; }
    public string? ResultMessage { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        ResultType = (MessageBoardResult)reader.ReadByte();

        if (ResultType == MessageBoardResult.BoardList)
        {
            var boardCount = reader.ReadUInt16();
            for (var i = 0; i < boardCount; i++)
            {
                Boards.Add(new ServerMessageBoardInfo
                {
                    Id = reader.ReadUInt16(),
                    Name = reader.ReadString8()
                });
            }
        }
        else if (ResultType is MessageBoardResult.Board or MessageBoardResult.Mailbox)
        {
            Source = (MessageBoardSource)reader.ReadByte();
            BoardId = reader.ReadUInt16();
            BoardName = reader.ReadString8();

            var postCount = reader.ReadByte();
            for (var i = 0; i < postCount; i++)
            {
                Posts.Add(new ServerMessageBoardPost
                {
                    IsHighlighted = reader.ReadBoolean(),
                    Id = reader.ReadInt16(),
                    Author = reader.ReadString8(),
                    Month = reader.ReadByte(),
                    Day = reader.ReadByte(),
                    Subject = reader.ReadString8()
                });
            }
        }
        else if (ResultType == MessageBoardResult.MailLetter)
        {
            
        }
        else if (ResultType is MessageBoardResult.PostSubmitted or MessageBoardResult.PostDeleted
                 or MessageBoardResult.PostHighlighted)
        {
            ResultSuccess = reader.ReadBoolean();
            ResultMessage = reader.ReadString8();
        }
    }
    
    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}