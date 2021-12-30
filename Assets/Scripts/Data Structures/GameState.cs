using System.Collections.Generic;
using GM.UI.Playfield;
using UnityEngine;

namespace GM.Data
{
    public struct LevelState
    {
        public int Level;
        public float Gravity;
    }

    //Used as reference point for UI.
    public class GameState
    {
        public List<int> LinesCleared;
        public LevelState LevelState;
        public Vector2? GameOverCenter;
        public AlertType? Alert;

        public GameState()
        {
            LinesCleared = new List<int>();
        }


        public void Reset()
        {
            LinesCleared.Clear();
            Alert = null;
        }
    }
}
