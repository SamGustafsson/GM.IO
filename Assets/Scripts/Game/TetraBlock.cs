using System.Collections.Generic;
using GM.Data;
using UnityEngine;

namespace GM.Game
{
    public struct Block
    {
        public Vector4 TextureST;
        public Color TopColor;
        public Color BotColor;
    }

    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    public class TetraBlock
    {
        private static readonly Dictionary<Direction, Vector2Int> DIRECTIONS = new Dictionary<Direction, Vector2Int>
        {
            {Direction.Left, new Vector2Int(-1, 0)},
            {Direction.Right, new Vector2Int(1, 0)},
            {Direction.Up, new Vector2Int(0, 1)},
            {Direction.Down, new Vector2Int(0, -1)}
        };

        private Vector2Int _position;
        private Vector2Int _startPosition;

        private Vector4 _textureST;
        private Color _topColor;
        private Color _botColor;
        private bool _canFloorKick;

        private int _blockGridSize;
        private int _rotationIndex;
        private int _lowestPoint;
        private List<BlockSublist> _rotationStates;

        public bool Landed { private set; get; }
        public bool IsAtLowestPoint { private set; get; }
        public List<Vector2Int> DefaultState => _rotationStates[0].Blocks;

        public TetraBlock(BlockData tetraBlock, Vector4 textureST, BlockGrid grid)
        {
            _rotationStates = tetraBlock.RotationStates;
            _textureST = textureST;
            _topColor = tetraBlock.TopBlockColor;
            _botColor = tetraBlock.BotBlockColor;
            _canFloorKick = tetraBlock.CanFloorKick;
            _blockGridSize = tetraBlock.GridSize;

            var highestInitial = int.MinValue;
            var lowestInitial = int.MaxValue;
            foreach (var position in _rotationStates[0].Blocks)
            {
                if (position.y > highestInitial)
                {
                    highestInitial = position.y;
                }

                if (position.y < lowestInitial)
                {
                    lowestInitial = position.y;
                }
            }

            IsAtLowestPoint = true;
            var yPos = grid.Size.y - highestInitial - 1;
            var xPos = (grid.Size.x >> 1) - Mathf.CeilToInt(tetraBlock.GridSize / 2f);

            _lowestPoint = grid.Size.y - _blockGridSize - lowestInitial;
            _startPosition = _position = new Vector2Int(xPos, yPos);

            PerformChecks(grid);
        }

        public void Reset()
        {
            _position = _startPosition;
            _rotationIndex = 0;
        }

        /// <summary>
        /// Move the piece.
        /// </summary>
        /// <returns>True if piece collides.</returns>
        public bool Move(Direction direction, BlockGrid grid, int distance = 1)
        {
            _position += DIRECTIONS[direction] * distance;

            if (CheckCollisions(grid))
            {
                _position -= DIRECTIONS[direction] * distance;
                return true;
            }

            PerformChecks(grid);

            return false;
        }

        public void Rotate(int direction, BlockGrid grid, bool noKick = false)
        {
            var previousIndex = _rotationIndex;
            _rotationIndex += direction;

            _rotationIndex = _rotationIndex < 0 
                ? _rotationStates.Count - 1
                : _rotationIndex % _rotationStates.Count;

            var collision = CheckCollisions(grid);

            if (noKick && collision || collision && CheckKicks(grid))
            {
                _rotationIndex = previousIndex;
            }

            PerformChecks(grid);
        }

        public Block GetBlock()
        {
            return new Block
            {
                TopColor = _topColor,
                BotColor = _botColor,
                TextureST = _textureST
            };
        }

        public Vector2Int[] GetPositions()
        {
            var positions = _rotationStates[_rotationIndex].Blocks.ToArray();

            for (var index = 0; index < positions.Length; index++)
            {
                positions[index] += _position;
            }

            return positions;
        }

        private bool CheckCollisions(BlockGrid grid)
        {
            var positions = GetPositions();

            foreach (var position in positions)
            {
                if (grid.CheckCollision(position))
                {
                    return true;
                }
            }

            return false;
        }

        public void PerformChecks(BlockGrid grid)
        {
            var positions = GetPositions();
            var landed = false;
            var lowestPoint = false;

            foreach (var position in positions)
            {
                if (position.y < _lowestPoint)
                {
                    _lowestPoint = position.y;
                    lowestPoint = true;
                }

                if (grid.CheckCollision(position + Vector2Int.down))
                {
                    landed = true;
                }
            }

            if (Landed == false && landed)
            {
                GlobalResources.GetInstance().SoundController.PlaySFX(SFX.Land);
            }

            Landed = landed;
            IsAtLowestPoint = lowestPoint;
        }

        private bool CheckKicks(BlockGrid grid)
        {
            if (!Move(Direction.Right, grid))
            {
                return false;
            }

            if (_blockGridSize / 2 > 1 && _rotationIndex == 0)
            {
                if (!Move(Direction.Right, grid, _blockGridSize / 2))
                {
                    return false;
                }
            }

            if (!Move(Direction.Left, grid))
            {
                return false;
            }

            if (_canFloorKick)
            {
                if (!Move(Direction.Up, grid))
                {
                    return false;
                }

                if (_blockGridSize / 2 > 1)
                {
                    if (!Move(Direction.Up, grid, _blockGridSize / 2))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
