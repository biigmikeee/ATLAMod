using ATLAMod.UI.BendingScroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATLAMod.Systems.Bending;

namespace ATLAMod.Systems.Bending
{
    public static class MoveRegistry
    {
        private static readonly Dictionary<string, BendingMove> _moves = new();

        public static void Register(BendingMove move) => _moves[move.Id] = move;
        public static BendingMove Get(string id) => _moves.TryGetValue(id, out var m) ? m : null;
        public static IEnumerable<BendingMove> All => _moves.Values;

        public static void Bootstrap()
        {
            Register(new Moves.Fire.FireFist());
            Register(new Moves.Fire.FireYoyo());
        }
    }
}
