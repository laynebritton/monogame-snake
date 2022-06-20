using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Snake
{
    class Snake : GameEntity
    {
        public Snake(int x, int y, Texture2D texture, ENTITY_TYPE entityType) : base(x, y, texture, entityType) { }

    }
}
