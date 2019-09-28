using OpenTK;

namespace Cloth3D
{
    public class ClothSpring
    {
        protected ClothPoint cpA { get; set; }
        protected ClothPoint cpB { get; set; }
        protected float RestLength { get; set; }

        public ClothSpring(ClothPoint cpA, ClothPoint cpB)
        {
            this.cpA = cpA;
            this.cpB = cpB;
            RestLength = (cpB.GetPosition() - cpA.GetPosition()).Length;
        }

        public ClothPoint GetPointA()
        {
            return cpA;
        }

        public ClothPoint GetPointB()
        {
            return cpB;
        }

        public int GetIdOfPointA()
        {
            return cpA.GetId();
        }

        public int GetIdOfPointB()
        {
            return cpB.GetId();
        }

        public void SetRestLength(float length)
        {
            RestLength = length;
        }

        public float GetRestLength()
        {
            return RestLength;
        }

        public void ApplyConstraint()
        {
            var pATopB = cpB.GetPosition() - cpA.GetPosition();
            var percentVector = 0.5f * (1 - RestLength / pATopB.Length) * pATopB;

            if (!cpA.IsLocked())
            {
                cpA.UpdatePosition(percentVector);
            }

            if (!cpB.IsLocked())
            {
                cpB.UpdatePosition(-percentVector);
            }
        }
    }
}
