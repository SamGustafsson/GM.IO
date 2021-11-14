using GM.Game;
using GM.UI;
using UnityEngine;

namespace GM.Data
{
    [RequireComponent(typeof(GameLogic))]
    public class GameData : MonoBehaviour
    {
        private static GameData INSTANCE;
        public static bool HasInstance => INSTANCE;

        public static GameData GetInstance()
        {
            return INSTANCE;
        }

        private void OnValidate()
        {
            GameLogic = GetComponent<GameLogic>();
        }

        private void Awake()
        {
            if (INSTANCE)
            {
                Debug.Log("Multiple GameData objects detected, you don't want this, deleting.");
                Destroy(gameObject);
            }

            INSTANCE = this;
        }

        //Game Controllers
        public GameLogic GameLogic;

        //Game Settings
        public DriverState InitialState;
        //Progression Data
        //TetraBlock Data

        [Header("Playfield Data")]
        public Vector2Int GridSize;
        public Material BorderMaterial;
        public Material BlockMaterial;
    }
}