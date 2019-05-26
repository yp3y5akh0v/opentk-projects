using OpenTK;

namespace SharedLib
{
    public interface IGameInput
    {
        void Input(GameWindow window, float interval);
    }
}
