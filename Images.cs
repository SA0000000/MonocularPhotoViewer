using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Permissions;
using System.Security;

namespace MonocularPhotoViewer
{
    //the struct will be used to store per image information
    public struct ImageInfo
    {
        public String Filename;
        public DateTime startTime;
        public DateTime endTime;
        public String timeTaken;       //end -start
        public String ImageCategory;

        public ImageInfo(String fname)
        {
            Filename = fname;
            startTime = DateTime.Now;
            endTime = DateTime.Now;
            timeTaken = (endTime - startTime).ToString();
            ImageCategory = "";
        }
    } ;
    class Images
    {
        public String studyNumber;

        //housekeeping flags
        private static bool start = true;
        private static bool end = false;
        bool flag = start;

        protected ImageInfo[] myImageInfo;  //represents all images in one study
        private int MAX;                    //total number of images
        private int imgCount = 0;           //to keep track of current image user is viewing
        int imgNum;
        private int TrainingImageCount;     //number of Training images
        private int TaskOneImageCount;      //number of task1 images
        private String ImageCategory;       //whether training image or task1 image

        //initialize components
        public Images(String s_num, String[] filelist, int training, int task1)
        {
            //set the study number
            studyNumber = s_num;

            //set number of images per task
            TrainingImageCount = training;
            TaskOneImageCount = task1;
            imgNum = training + task1;
            MAX = imgNum + 2;    //to include the other task separation images

            //initialize ImageInfo list
            myImageInfo = new ImageInfo[MAX];
            init(filelist);
        }

        void init(String[] filelist)
        {
            for (int i = 0, k = 0; i < MAX && k < imgNum; i++)
            {
                if (i == 0)
                {
                    ImageCategory = "Training";
                    myImageInfo[i] = new ImageInfo(@"StartofTraining.png");
                }
                else if (i == TrainingImageCount + 1)
                {
                    ImageCategory = "Task 1";
                    myImageInfo[i] = new ImageInfo(@"EndofTraining.png");
                }
                //else if (i == TrainingImageCount + TaskOneImageCount+2) //+2 to account for the images that are added to indicate start and end of tasks
                //{
                //    ImageCategory = "End";
                //    myImageInfo[i] = new ImageInfo(@"EndofTask1.png");
                //}
                else
                    myImageInfo[i] = new ImageInfo(filelist[k++]);

                myImageInfo[i].ImageCategory = ImageCategory;
            }
        }

        //Navigate to next image--returns the next image in the list 
        //and stores timing information for the 
        //previous image and initializes next image start time
        public String nextImage()
        {
            String file = "";

            if (imgCount == (MAX + 1))         //when the user is done in the end and we need to store stuff
            {
                if (flag == end)  //to get the end time for the last image
                {
                    myImageInfo[imgCount - 2].endTime = DateTime.Now;
                    myImageInfo[imgCount - 2].timeTaken = (myImageInfo[imgCount - 2].endTime - myImageInfo[imgCount - 2].startTime).ToString();
                    flag = start;
                    SaveToFile();
                    file = @"LastImage.png";
                    //if (MessageBox.Show("Congratulations!! You have successfully finished the study!! :) :)") == DialogResult.OK)
                    //    this.Close();
                }
            }


            if (imgCount < MAX && imgCount >= 0)
            {
                if (myImageInfo[imgCount].Filename != null)
                {
                    file = (myImageInfo[imgCount].Filename);
                    if (flag == start)
                    {
                        myImageInfo[imgCount].startTime = DateTime.Now;
                        if (imgCount > 0)
                        {
                            StoreTimeInfo(imgCount);
                        }
                    }

                    imgCount++;
                }
                else
                {
                    // imgCount = MAX + 2;
                    flag = end;
                }
            }

            if (imgCount == MAX)
            {
                //btnProceed.Text = "DONE";
                imgCount = MAX + 1;
                flag = end;
            }

            //for debuging
            if (imgCount < MAX)
                System.Diagnostics.Debug.WriteLine("ImageNum:%{0}  Start: {1}  End:{2}   Total:{3}   Image:{4}", imgCount - 1, myImageInfo[imgCount - 1].startTime.ToString(), myImageInfo[imgCount - 1].endTime.ToString(), myImageInfo[imgCount - 1].timeTaken.ToString(), Path.GetFileName(myImageInfo[imgCount - 1].Filename));

            return file;

        } //end of function

        //Store time---set timing information for (i-1)th image and init timing for ith image
        private void StoreTimeInfo(int i)
        {
            myImageInfo[i - 1].endTime = DateTime.Now;
            myImageInfo[i - 1].timeTaken = (myImageInfo[i - 1].endTime - myImageInfo[i - 1].startTime).ToString();
            myImageInfo[i].startTime = DateTime.Now;
        }

        //reset timing when user wants to go back to an image
        private void RestartTimeInfo(int i)
        {
            //clear the start and end time of the next image
            myImageInfo[i + 1].startTime = DateTime.Parse("01/01/0001 00:00:00 AM",
                                      System.Globalization.CultureInfo.InvariantCulture);
            myImageInfo[i + 1].endTime = DateTime.Parse("01/01/0001 00:00:00 AM",
                                     System.Globalization.CultureInfo.InvariantCulture);

            //set the start time of the current image; the end time and total 
            //time will be updated when the user clicks the next button
            myImageInfo[i].startTime = DateTime.Now;

        }

        //Save to File---save all the information stored in the ImageInfo for all the 
        //images that were part of one study
        private void SaveToFile()
        {
            //create directory if it doesnot exist and then save files in that directory
            String dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            dir = System.IO.Path.Combine(dir, "VC_shares");
            if ((!System.IO.Directory.Exists(dir)))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            StreamWriter sw = new StreamWriter(dir + "\\VC_Study_" + studyNumber + ".txt");
            String entry;
            sw.WriteLine("Image filename\t\t\t\t\t\t\tImage Category\tStart Time\t\tEnd Time\t\tTotal Time\n");

            //write all the entries for this study set
            for (int i = 0; i < MAX; i++)
            {
                entry = myImageInfo[i].Filename + "\t" + myImageInfo[i].ImageCategory + "\t" + myImageInfo[i].startTime.ToString() +
                        "\t" + myImageInfo[i].endTime.ToString() +
                        "\t" + myImageInfo[i].timeTaken;
                sw.WriteLine(entry);
            }
            sw.Close();
        }//end of function
    } //end of class
}
