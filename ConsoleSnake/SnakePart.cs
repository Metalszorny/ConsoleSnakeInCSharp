using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSnake
{
    /// <summary>
    /// Base class for SnakePart.
    /// </summary>
    class SnakePart
    {
        #region Fields

        // The position of the SnakePart.
        private Position position;

        // The body part type of the SnakePart.
        private BodyParts bodyPart;

        // The body part types of the SnakePart.
        public enum BodyParts
        {
            Head,
            Body
        }

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public Position Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Gets or sets the bodyPart.
        /// </summary>
        /// <value>
        /// The bodyPart.
        /// </value>
        public BodyParts BodyPart
        {
            get { return bodyPart; }
            set { bodyPart = value; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SnakePart"/> class.
        /// </summary>
        public SnakePart()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnakePart"/> class.
        /// </summary>
        /// <param name="position">The position of the SnakePart.</param>
        /// <param name="bodypart">The body part type of the SnakePart.</param>
        public SnakePart(Position position, BodyParts bodypart)
        {
            this.Position = position;
            this.BodyPart = bodypart;
        }

		/// <summary>
        /// Destroys the instance of the <see cref="SnakePart"/> class.
        /// </summary>
        ~SnakePart()
        { }
		
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Draws the SnakePart.
        /// </summary>
        /// <returns>The image of the SnakePart object.</returns>
        public char Draw()
        {
            try
            {
                // The snake's part is head.
                if (BodyPart == BodyParts.Head)
                {
                    return '@';
                }
                // The snake's part is body.
                else
                {
                    return 'O';
                }
            }
            catch
            {
                return ' ';
            }
        }

        /// <summary>
        /// Repositions the SnakePart.
        /// </summary>
        /// <param name="newposition">The new position of the SnakePart.</param>
        public void Reposition(Position newposition)
        {
            try
            {
                // Sets the position's value to the new position.
                this.position.X = newposition.X;
                this.position.Y = newposition.Y;
            }
            catch
            { }
        }

        #endregion Methods
    }
}
