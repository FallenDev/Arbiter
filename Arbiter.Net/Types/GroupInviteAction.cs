namespace Arbiter.Net.Types;

public enum GroupInviteAction : byte
{
    Invite = 1,
    QuickInvite = 2,
    AcceptInvite = 3,
    CreateGroupBox = 4,
    ViewGroupBox = 5,
    RemoveGroupBox = 6,
    RequestToJoin = 7,
}