using MakeMeHero.Core;
using UnityEngine;
using System;

namespace MakeMeHero.Game
{
    /// <summary>Attach to a tile prefab. It maps one visual tile to one core grid coordinate.</summary>
    public sealed class TileView : MonoBehaviour
    {
        public static event Action<TileView> Clicked;
        [SerializeField, Range(0, 2)] private int column;
        [SerializeField, Range(0, 2)] private int row;

        public GridPosition Position { get { return new GridPosition(column, row); } }
        public int Column { get { return column; } }
        public int Row { get { return row; } }

        public void Configure(int newColumn, int newRow)
        {
            column = Mathf.Clamp(newColumn, 0, 2);
            row = Mathf.Clamp(newRow, 0, 2);
        }

        private void OnMouseUpAsButton()
        {
            if (Clicked != null) Clicked(this);
        }
    }
}
