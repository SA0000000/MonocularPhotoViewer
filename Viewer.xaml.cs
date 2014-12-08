using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace MonocularPhotoViewer
{
    
    public partial class Viewer : Window
    {
        DispatcherTimer _timer = new DispatcherTimer();     //to poll for Xbox controller events
        TransformGroup xformGroup;
        ScaleTransform xform;
        GamePadState lastGamePadState;

        
        //enum to indicate direction
        enum Direction : int
        {
            up = 1,
            down,
            left,
            right
        };

        //varaible to hold which direction moved 
        int move = 0;
        int moveValue = 10; //how much to move by

        //For zooming
        enum Zoom : int { zoomIn = 1, zoomOut = -1 };
        float zoomValue = 0.05f;
        int x_zoom = 0, y_zoom = 0;

        //create variables that will store where the user set the first image and 
        //then will be used to display the other images at the same spatial window coordinates and state
        bool setImagePosition = false;
        double leftPos, topPos;
        double[] img_zoomVal = new double[2];

        //create an instance of Class Images to store and deal with all images
        Images images;

        public Viewer(String studynum, String[] filelist, int training, int task1)
        {
            InitializeComponent();
            LeftCanvas.Visibility = Visibility.Visible;

            //set transforms for left canvas
            xformGroup = new TransformGroup();
            xform = new ScaleTransform();
            xformGroup.Children.Add(xform);
            imageRegion.RenderTransform = xformGroup;

            //set initial zoom values for images
            //image
            img_zoomVal[0] = xform.ScaleX + 0.15f * (-1);
            img_zoomVal[1] = xform.ScaleY + 0.15f * (-1);

            //init Images class object
            images = new Images(studynum, filelist, training, task1);

            //set first image on the canvas
            imageRegion.Source = new BitmapImage(new Uri(@"FirstImage.png", UriKind.RelativeOrAbsolute));
            imageRegion.Focus();

            //set timer object
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        //poll for Xbox Controller events
        void _timer_Tick(object sender, EventArgs e)
        {
            //check for Xbox Controller inputs  and set flags
            UpdateInput();
            UpdatePosition();
        }

        //check for Xbox Controller State and take actions accordingly
        void UpdateInput()
        {
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);

            //check for GamePadState and various buttons that have been pressed
            //based on button presses take specific actions
            if (currentState.IsConnected && lastGamePadState != currentState)
            {
                #region Dpad Direction
                //if DPad has been pressed...check which direction and move the image accordingly
                if (currentState.DPad.Up == ButtonState.Pressed)
                {
                    y_zoom = (int)Zoom.zoomOut;
                }
                else if (currentState.DPad.Down == ButtonState.Pressed)
                {
                    y_zoom = (int)Zoom.zoomIn;
                }
                else if (currentState.DPad.Left == ButtonState.Pressed)
                {
                    x_zoom = (int)Zoom.zoomOut;
                }
                else if (currentState.DPad.Right == ButtonState.Pressed)
                {
                    x_zoom = (int)Zoom.zoomIn;
                }
                #endregion

                #region LeftThumbstick Direction
                //ifLeftThumbstick has been moved move image accordingly
                if (lastGamePadState.IsButtonUp(Buttons.LeftThumbstickUp) && currentState.IsButtonDown(Buttons.LeftThumbstickUp))
                    move = (int)Direction.up;
                else if (lastGamePadState.IsButtonUp(Buttons.LeftThumbstickDown) && currentState.IsButtonDown(Buttons.LeftThumbstickDown))
                    move = (int)Direction.down;
                else if (lastGamePadState.IsButtonUp(Buttons.LeftThumbstickRight) && currentState.IsButtonDown(Buttons.LeftThumbstickRight))
                    move = (int)Direction.up;
                else if (lastGamePadState.IsButtonUp(Buttons.LeftThumbstickLeft) && currentState.IsButtonDown(Buttons.LeftThumbstickLeft))
                    move = (int)Direction.left;
                else
                    move = 0;
                #endregion

                //if Button A has been pressed move to the next image and save state accordingly
                if (currentState.Buttons.A == ButtonState.Pressed)
                {
                    //save state and move to the next image
                    NextImage();
                }
            }

            lastGamePadState = currentState;
        }

        //update position of the image on the canvas based on Xbox controller state
        void UpdatePosition()
        {
            //update position of image
            //get focus on image
                LeftCanvas.Focus();
                Double LeftPos = Convert.ToDouble(imageRegion.GetValue(Canvas.LeftProperty));
                Double TopPos = Convert.ToDouble(imageRegion.GetValue(Canvas.TopProperty));
                if (move != 0)
                    moveImage(LeftPos, TopPos, imageRegion);
                if (x_zoom != 0 || y_zoom !=0)
                {
                    zoomImage(LeftPos, TopPos, imageRegion);
                }

                //store the values where the left image should be drawn and what their zoom value should be
                //so that all other subsequent images with the same values
                leftPos = LeftPos;
                topPos = TopPos; 
        }

        //move the image in 2D
        void moveImage(double LeftPos, double TopPos, Image image)
        {
            if (move == (int)Direction.left)
            {
                //if (LeftPos < (LeftCanvas.ActualWidth - leftImage.Width))
                image.SetValue(Canvas.LeftProperty, LeftPos - moveValue);
            }

            else if (move == (int)Direction.right)
            {
                image.SetValue(Canvas.LeftProperty, LeftPos + moveValue);
            }

            else if (move == (int)Direction.up)
            {
                //if (topPos < (LeftCanvas.ActualHeight - leftImage.Height))
                image.SetValue(Canvas.TopProperty, TopPos - moveValue);
            }
            else if (move == (int)Direction.down)
            {
                image.SetValue(Canvas.TopProperty, TopPos + moveValue);
            }
            else if (move == -1)
            {
                //image needs to be redrawn at topPos, leftPos because it was scaled 
                image.SetValue(Canvas.TopProperty, TopPos);
                image.SetValue(Canvas.LeftProperty, LeftPos);
            }

            //reset move to 0
            move = 0;
        }

        //make image larger or smaller
        void zoomImage(double LeftPos, double TopPos, Image image)
        {
            TransformGroup transformGroup = (TransformGroup)image.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];

            //scale image and then redraw it at the previous position
            //only scale if scaling factor is greater than zero
            if (transform.ScaleX + zoomValue * x_zoom > 0 || transform.ScaleY + zoomValue * y_zoom > 0)
            {
                transform.ScaleX += zoomValue * x_zoom;
                transform.ScaleY += zoomValue * y_zoom;
                img_zoomVal[0] = transform.ScaleX;
                img_zoomVal[1] = transform.ScaleY;
            }

            //after zooming make sure the top left corner of the image doesn't change
            move = -1;
            moveImage(LeftPos, TopPos, image);

            //reset zoom to 0
            x_zoom = 0;
            y_zoom = 0;
        }

        //move to the next image
        void NextImage()
        {
            String uri = images.nextImage();
            imageRegion.Source = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

            //draw the image at the place where the user last set the location to
            moveImage(leftPos, topPos, imageRegion);
            
            //set default zoom values for images
            //left image
            xform.ScaleX = img_zoomVal[0];
            xform.ScaleY = img_zoomVal[1];

            //check if the user has reached the last image 
            //if yes then inform them they are done
            if (uri.Equals(@"LastImage.png"))
            {
                if (MessageBox.Show("Congratulations!! You have successfully finished the study!! :) :)", "Viewer", MessageBoxButton.OK) == MessageBoxResult.OK)
                    this.Close();
            }
        }
    }
}
