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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace MonocularPhotoViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int ImageNum = 0;
        private const int MAX = 18;
        private static String[] filenames;   //= new String[12];
        OpenFileDialog openFileDialog = new OpenFileDialog();
        private int NumTrainingImg = 0;
        private int NumTask1Img = 0;

        public MainWindow()
        {
            InitializeComponent();
            filenames = new String[MAX];
            //set properties for the file dialog
            initFileDialog();
        }

        //set properties for the file dialog to enable selecting multiple files from disk
        void initFileDialog()
        {
            this.openFileDialog.Filter = "Images (*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|" +
                                           "All files (*.*)|*.*";
            //allow user to select multiple images
            this.openFileDialog.Multiselect = true;
            this.openFileDialog.Title = "Binocular AR Image Select...";
        }


        //select images from the file system and add them to the viewer
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //add images only if the max limit hasn't been reached
            if (ImageNum < MAX)
            {
                //Open the dialog box and if user selected image add it to my list of images
                if (openFileDialog.ShowDialog() == true)
                {
                    //Read the files
                    foreach (String file in openFileDialog.FileNames)
                    {
                        //add the images and an associated image number to a structure for later use
                        //also show the images that have been added on the form--in a list
                        try
                        {
                            filenames[ImageNum] = file;
                            imgList.Items.Add(filenames[ImageNum]);
                            ImageNum++;

                            //make sure less than MAX images are selected
                            if (ImageNum == MAX)
                            {
                                MessageBox.Show("Too many images added!! No more room!");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Oops! " + ex.Message);
                        }
                    }   //end of foreach loop
                }
            }
            else
            {
                MessageBox.Show("Too many images added!! No more room!");
            }

        }

        //to clear all 
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            //reset number of images to zero and clear everything on the form
            ImageNum = 0;
            txtNumTrainingImages.Text = "";
            txtNumTask1Images.Text = "";
            imgList.Items.Clear();
            int i = 0;
            while (i < MAX)
            {
                filenames[i] = "";
                i++;
            }

        }

        //when user wants to start the picture viewer
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //check number of training images and task 1 images and make sure they are within correct limits

            if (ImageNum > 0)
            {
                if (checkImageCount())
                {
                    //if user has added images....then launch picture viewer
                    Viewer myViewer = new Viewer(txtStudyNum.Text.Trim(), filenames, NumTrainingImg, NumTask1Img);
                    myViewer.Show();
                }
            }
            else
            {
                MessageBox.Show("No Images!!!");
            }
        }

        //make sure the user entered the number of images for traning and task modules correctly
        bool checkImageCount()
        {
            bool flag = false;
            if (String.IsNullOrEmpty(txtNumTrainingImages.Text.Trim()))
                MessageBox.Show("You forgot to Add the Number of Training Images!!!You forgot to Add the Study Number!!!");
            else if (String.IsNullOrEmpty(txtNumTask1Images.Text.Trim()))
                MessageBox.Show("You forgot to Add the Study Number!!!");
            else if (Int32.Parse(txtNumTrainingImages.Text) < 1 || Int32.Parse(txtNumTrainingImages.Text) > 7)
                MessageBox.Show("Max of 7 training images allowed!!!");
            else if (Int32.Parse(txtNumTask1Images.Text) < 1 || Int32.Parse(txtNumTask1Images.Text) > 18)
                MessageBox.Show("Enter a number between 1 and 11 for number of images in Task 1!!!");
            else
            {
                NumTask1Img = Int32.Parse(txtNumTask1Images.Text);
                NumTrainingImg = Int32.Parse(txtNumTrainingImages.Text);
                flag = true;
            }
            //check to make sure user entered the study number
            if (String.IsNullOrEmpty(txtStudyNum.Text.Trim()))
            {
                flag = false;
                MessageBox.Show("Enter Study Number!!");
            }
            return flag;
        }
    }
}
