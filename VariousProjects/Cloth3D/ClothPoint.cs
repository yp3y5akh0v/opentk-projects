using OpenTK;
using SharedLib;

namespace Cloth3D
{
    public class ClothPoint: Point3D
    {
        protected int Id { get; set; }
        protected bool Locked { get; set; }
        protected float Mass { get; set; }
        protected Vector3 Acceleration { get; set; }
        protected Vector3 prevPosition { get; set; }

        public ClothPoint(int Id): this(Id, Vector3.Zero)
        {
        }

        public ClothPoint(int Id, float x, float y, float z): base(x, y, z)
        {
            this.Id = Id;
            Mass = 1f;
            Acceleration = Vector3.Zero;
            Locked = false;
            prevPosition = new Vector3(x, y, z);
        }

        public ClothPoint(int Id, Vector3 v): this(Id, v.X, v.Y, v.Z)
        {
        }

        public void SetId(int Id)
        {
            this.Id = Id;
        }

        public int GetId()
        {
            return Id;
        }

        public void Lock()
        {
            Locked = true;
        }

        public void UnLock()
        {
            Locked = false;
        }

        public bool IsLocked()
        {
            return Locked;
        }

        public void SetMass(float Mass)
        {
            this.Mass = Mass;
        }

        public float GetMass()
        {
            return Mass;
        }

        public void SetAcceleration(Vector3 Acceleration)
        {
            this.Acceleration = Acceleration;
        }

        public Vector3 GetAcceleration()
        {
            return Acceleration;
        }

        public void ApplyForce(Vector3 Force)
        {
            SetAcceleration(Force / Mass);
        }

        public void SetPrevPosition(Vector3 v)
        {
            prevPosition = new Vector3(v);
        }

        public Vector3 GetPrevPosition()
        {
            return prevPosition;
        }

        public void ApplyPositionStep(float delta)
        {
            if (Locked) return;

            var temp = new Vector3(position);
            position = position + (position - prevPosition) * (1 - Constants.DAMPING) + Acceleration * delta * delta / 2f;
            prevPosition = temp;
            Acceleration = Vector3.Zero;
        }
    }
}
