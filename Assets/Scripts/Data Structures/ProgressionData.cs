using System;
using UnityEngine;

namespace GM.Data
{
    [Serializable]
    public struct ProgressionLevel
    {
        [HideInInspector] public string ArrayName;
        public int Level;
        public bool EnablePieceGhost;
        public ProgressionTimers ProgressionTimers;
        public ProgressionAssets ProgressionAssets;
    }

    [Serializable]
    public struct ProgressionTimers
    {
        [Min(1)] public float DropFrames;
        [Range(1, 20)] public int Gravity;
        [Space]
        [Min(1)] public int SpawnFrames;
        [Min(1)] public int LineClearSpawnFrames;
        [Min(1)] public int LockFrames;
        [Min(1)] public int LineFrame;
        [Min(1)] public int AutoShiftFrames;
    }

    [Serializable]
    public struct ProgressionAssets
    {
        public Texture Background;
        public BGM Music;
    }

    public struct ProgressionState
    {
        public float SpawnDuration;
        public float LineClearSpawnDuration;
        public float DropDuration;
        public int Gravity;
        public float LockDuration;
        public float LineDuration;
        public float AutoShiftDuration;
        public bool GhostPiece;
    }

    [CreateAssetMenu(fileName = "ProgressionData", menuName = "GM/Progression Data")]
    public class ProgressionData : ScriptableObject
    {
        public static float FRAME = 0.0166666667f;
        public int EndLevel;
        [SerializeField] private ProgressionLevel[] _progressionLevel;

        private int? _level;
        private int _index;

        private ProgressionState _state;

        public ProgressionState GetState(int level)
        {
            if (_level.HasValue && _level == level)
            {
                return _state;
            }

            if (level < _level)
            {
                _level = level;
                _index = 0;
            }

            if (_index + 1 < _progressionLevel.Length)
            {
                while (level >= _progressionLevel[_index + 1].Level)
                {
                    _index++;

                    if (_index + 1 >= _progressionLevel.Length)
                    {
                        break;
                    }
                }
            }

            var progression = _progressionLevel[_index].ProgressionTimers;
            _level = level;

            _state = new ProgressionState
            {
                DropDuration = progression.DropFrames * FRAME,
                Gravity = progression.Gravity,

                SpawnDuration = progression.SpawnFrames * FRAME,
                LineClearSpawnDuration = progression.LineClearSpawnFrames * FRAME,
                LockDuration = progression.LockFrames * FRAME,
                LineDuration = progression.LineFrame * FRAME,
                AutoShiftDuration = progression.AutoShiftFrames * FRAME,
                GhostPiece = _progressionLevel[_index].EnablePieceGhost
            };

            return _state;
        }

        public ProgressionAssets GetAssets(int level)
        {
            for (var index = 0; index < _progressionLevel.Length; index++)
            {
                var progressionLevel = _progressionLevel[index];
                if (level >= progressionLevel.Level)
                {
                    continue;
                }

                return _progressionLevel[index - 1].ProgressionAssets;
            }

            return _progressionLevel[_progressionLevel.Length - 1].ProgressionAssets;
        }

        private void OnValidate()
        {
            if (_progressionLevel.Length > 0)
            {
                _progressionLevel[0].ArrayName = $"Level: {_progressionLevel[0].Level}";
            }

            for (var i = 1; i < _progressionLevel.Length; i++)
            {
                if (_progressionLevel[i].Level <= _progressionLevel[i - 1].Level)
                {
                    _progressionLevel[i].Level = _progressionLevel[i - 1].Level + 1;
                }

                _progressionLevel[i].ArrayName = $"Level: {_progressionLevel[i].Level}";

                var currentTimers = _progressionLevel[i].ProgressionTimers;
                var prevTimers = _progressionLevel[i - 1].ProgressionTimers;

                if (currentTimers.Gravity < 1)
                {
                    currentTimers.Gravity = 1;
                }
                if (currentTimers.SpawnFrames >= prevTimers.SpawnFrames)
                {
                    currentTimers.SpawnFrames = prevTimers.SpawnFrames;
                }
                if (currentTimers.LineClearSpawnFrames >= prevTimers.LineClearSpawnFrames)
                {
                    currentTimers.LineClearSpawnFrames = prevTimers.LineClearSpawnFrames;
                }
                if (currentTimers.LockFrames >= prevTimers.LockFrames)
                {
                    currentTimers.LockFrames = prevTimers.LockFrames;
                }
                if (currentTimers.LineFrame >= prevTimers.LineFrame)
                {
                    currentTimers.LineFrame = prevTimers.LineFrame;
                }
                if (currentTimers.AutoShiftFrames >= prevTimers.AutoShiftFrames)
                {
                    currentTimers.AutoShiftFrames = prevTimers.AutoShiftFrames;
                }

                _progressionLevel[i].ProgressionTimers = currentTimers;

                if (i + 1 < _progressionLevel.Length)
                {
                    if (_progressionLevel[i + 1].EnablePieceGhost)
                    {
                        _progressionLevel[i].EnablePieceGhost = true;
                    }
                }

                if (i > 0)
                {
                    if (_progressionLevel[i].ProgressionAssets.Music < _progressionLevel[i - 1].ProgressionAssets.Music)
                    {
                        _progressionLevel[i].ProgressionAssets.Music = _progressionLevel[i - 1].ProgressionAssets.Music;
                    }
                }
            }
        }

        public void Reset()
        {
            _level = -1;
            _index = 0;
        }
    }
}
