using System;

namespace Yuizumi.Icfpc2018
{
    using static Harmonics;
    using static Voxel;

    public static class StateExtension
    {
        internal static void FillVoxel(this State s, Coord c)
        {
            if (s.Matrix[c] == Void) {
                s.Matrix[c] = Full;
                s.Energy += 12;
            } else {
                s.Energy += 6;
            }
        }

        internal static void VoidVoxel(this State s, Coord c)
        {
            if (s.Matrix[c] == Full) {
                s.Matrix[c] = Void;
                s.Energy -= 12;
            } else {
                s.Energy += 3;
            }
        }

        public static void VerifyWellFormed(this State s)
        {
            if (s.Harmonics == Low) {
                Requires.State(s.Matrix.IsGrounded(), "Model is not grounded.");
            }
            // TODO(yuizumi): Verify other conditions?
        }
    }
}
