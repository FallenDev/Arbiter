namespace Arbiter.Net.Types;

public static class SpriteFlags
{
    public const ushort Creature = 0x4000;
    public const ushort Item = 0x8000;

    public static bool IsCreature(ushort sprite) => (sprite & Creature) != 0;
    public static bool IsItem(ushort sprite) => (sprite & Item) != 0;

    public static ushort SetCreature(ushort sprite) => (ushort)(sprite | Creature);
    public static ushort SetItem(ushort sprite) => (ushort)(sprite | Item);

    public static ushort ClearFlags(ushort sprite) => (ushort)(sprite & ~Creature & ~Item);
}