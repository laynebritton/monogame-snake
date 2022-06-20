using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Snake
{
    class GridTile : GameEntity
    {
        public GridTile(int x, int y, Texture2D texture, ENTITY_TYPE entityType) : base(x, y, texture, entityType) { }
    }
}
