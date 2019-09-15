using OpenTK;

namespace SharedLib
{
    public class Light: IcoSphere
    {
        private Vector3 color;
        private Vector3 direction;
        private float ambientStrength;
        private Segment beam;

        public Light(): base(200f, 5)
        {
            color = Vector3.One;
            direction = Vector3.Zero;
            ambientStrength = 0.6f;
        }

        public void SetColor(float r, float g, float b)
        {
            color = new Vector3(r, g, b);
        }

        public void SetAmbientStrength(float ambientStrength)
        {
            this.ambientStrength = ambientStrength;
        }

        public void SetDirection(Vector3 direction)
        {
            this.direction = direction;
        }

        public Vector3 GetDirection()
        {
            return direction.Length > 0 ? direction.Normalized() : direction;
        }

        public Vector3 GetColor()
        {
            return color;
        }

        public float GetAmbientStrength()
        {
            return ambientStrength;
        }

        public override void CleanUp()
        {
            base.CleanUp();
            beam?.CleanUp();
        }

        public override void Render()
        {
            base.Render();
            beam?.Render();
        }

        public void SetBeamWidth(float w)
        {
            beam?.SetWidth(w);
        }

        public void InitiateBeam(float w)
        {
            var startPos = GetRadius() * GetDirection();
            beam = new Segment(startPos, startPos + w * GetDirection(), Vector3.UnitY);
        }
    }
}
