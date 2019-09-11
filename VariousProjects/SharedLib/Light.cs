using OpenTK;

namespace SharedLib
{
    public class Light: IcoSphere
    {
        private Vector3 color;
        private float ambientStrength;
        
        public Light(): base(200f, 5)
        {
            color = Vector3.One;
            ambientStrength = 0.8f;
        }

        public void SetColor(float r, float g, float b)
        {
            color = new Vector3(r, g, b);
        }

        public void SetAmbientStrength(float ambientStrength)
        {
            this.ambientStrength = ambientStrength;
        }

        public Vector3 GetColor()
        {
            return color;
        }

        public float GetAmbientStrength()
        {
            return ambientStrength;
        }
    }
}
