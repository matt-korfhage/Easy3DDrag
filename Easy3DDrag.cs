// compile guard here to prevent version sillyness
#if !NET7_0_OR_GREATER
#error "Note: Remember that you should be using .NET SDK 7"
#endif

using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Easy3DDrag
{
    public class Easy3DDrag
    {

        private bool currently_dragging = false;
        private Point last_point_captured = new(0, 0);
        private static readonly double SQUARE = 2;
        private static readonly bool ENABLE_CONCURRENCY = true;
        private Viewport3D to_register;
        private MouseButton button;

        #region Trig_Lambda_Expressions

        private readonly Func<Point, Point, double> Get_Vector_Magnitude = (A, B) =>
        {
            double diffX = A.X - B.X;
            double diffY = A.Y - B.Y;
            return Math.Sqrt(Math.Pow(diffX, SQUARE) + Math.Pow(diffY, SQUARE));
        };

        private readonly Func<Point, Point, Vector3D> Get_Component_Ratios = (A, B) =>
        {
            double diffX = A.X - B.X;
            double diffY = A.Y - B.Y;
            return new Vector3D(diffY, diffX, 0);
        };

        #endregion // trig lambda expressions


        /// <summary>
        /// Class constructor to initialize the register of a Viewport3D to '
        /// a mousebutton event
        /// </summary>
        /// <param name="to_register">the viewport to register</param>
        /// <param name="button"></param>
        public Easy3DDrag(Viewport3D to_register, MouseButton button)
        {
            this.to_register = to_register;
            this.button = button;
        }

        /// <summary>
        /// Addings a 3D drag event to a 3D viewport in a WPF window.
        /// After calling, the 3D Viewport will automatically respond to mouse drags of the
        /// specific mouse button type.
        /// </summary>
        public void Register_3D_Viewer()
        {
            Modify_Event_Handlers();
        }

        #region Handler_Logic


        private void Viewport_MouseClick(object sender, MouseButtonEventArgs e)
        {
            currently_dragging = e.ButtonState == MouseButtonState.Pressed;
            last_point_captured = e.GetPosition(sender as IInputElement);
        }

        private void ViewPort_MouseMove(object sender, MouseEventArgs e)
        {
            if (!currently_dragging)
                return;
            Point b = e.GetPosition((Viewport3D)sender);
            RotateTransform3D camera_transform = new(new AxisAngleRotation3D(
                Get_Component_Ratios(last_point_captured, b),
                Get_Vector_Magnitude(last_point_captured, b)));
            ((Viewport3D)sender).Camera.Transform = camera_transform;
        }

        #endregion // handler logic

        #region Helper_Functions

            private void Modify_Event_Handlers()
            {
                switch (button)
                {
                    //MouseButtonEventHandler click = new MouseButtonEventHandler(Viewport_MouseClick);
                    /* For some reason WPF doesn't have a dedicated middle 
                        * mouse button up / down event so it's locked into
                        * these two for now */
                    case MouseButton.Left:
                        to_register.AddHandler(UIElement.MouseLeftButtonDownEvent,
                            (MouseButtonEventHandler)Viewport_MouseClick,
                            ENABLE_CONCURRENCY);
                        to_register.AddHandler(UIElement.MouseLeftButtonUpEvent,
                        (MouseButtonEventHandler)Viewport_MouseClick,
                        ENABLE_CONCURRENCY);
                    break;
                    case MouseButton.Right:
                        to_register.AddHandler(Viewport3D.MouseRightButtonDownEvent,
                            (MouseButtonEventHandler)Viewport_MouseClick,
                            ENABLE_CONCURRENCY);
                        to_register.AddHandler(Viewport3D.MouseRightButtonUpEvent,
                            (MouseButtonEventHandler)Viewport_MouseClick,
                            ENABLE_CONCURRENCY);
                        break;
                    default:
                        throw new ArgumentException($"Illegal type {button} - " +
                            $"that mouse control is not supported for this library");
                }
            to_register.AddHandler(UIElement.MouseMoveEvent,
                (MouseEventHandler)ViewPort_MouseMove,
                ENABLE_CONCURRENCY);
            }

        #endregion // helper functions
    }
}
