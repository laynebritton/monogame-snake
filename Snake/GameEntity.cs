using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Snake
{
    abstract class GameEntity
    {
        protected GameEntity(int x, int y, Texture2D texture, ENTITY_TYPE entityType)
        {
            X = x;
            Y = y;
            this.Texture = texture;
            this.EntityType = entityType;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public Texture2D Texture { get; set; }
        public ENTITY_TYPE EntityType { get; set; }

        public virtual Vector2 GetVector()
        {
            return new Vector2(X, Y);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Texture, GetVector(), Color.White);
            spriteBatch.End();
        }

        public enum ENTITY_TYPE
        {
            SNAKE,
            SNAKE_BODY,
            FOOD,
            GRID_TILE,
        }

        public int GetXCoordinate(int tileSize)
        {
            return X / tileSize;
        }

        public int GetYCoordinate(int tileSize)
        {
            return X / tileSize;
        }

        override
        public string ToString()
        {
            return "Entity: " + EntityType + " - at X: " + X + ", and Y:" + Y;
        }

    }
}
