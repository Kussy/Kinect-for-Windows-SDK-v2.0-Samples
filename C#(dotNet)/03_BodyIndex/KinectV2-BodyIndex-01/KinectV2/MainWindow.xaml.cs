﻿using System;
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
using Microsoft.Kinect;

namespace KinectV2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinect;

        BodyIndexFrameReader bodyIndexFrameReader;
        FrameDescription bodyIndexFrameDesc;
        byte[] bodyIndexBuffer;

        WriteableBitmap bodyIndexColorImage;
        Int32Rect bodyIndexColorRect;
        int bodyIndexColorStride;
        int bodyIndexColorBytesPerPixel = 4;
        byte[] bodyIndexColorBuffer;

        Color[] bodyIndexColors;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded( object sender, RoutedEventArgs e )
        {
            try {
                kinect = KinectSensor.GetDefault();
                if ( kinect == null ) {
                    throw new Exception("Kinectを開けません");
                }

                kinect.Open();

                // 表示のためのデータを作成
                bodyIndexFrameDesc = kinect.DepthFrameSource.FrameDescription;
                bodyIndexBuffer = new byte[bodyIndexFrameDesc.LengthInPixels];

                // 表示のためのビットマップに必要なものを作成
                bodyIndexColorImage = new WriteableBitmap( bodyIndexFrameDesc.Width, bodyIndexFrameDesc.Height,
                    96, 96, PixelFormats.Bgra32, null );
                bodyIndexColorRect = new Int32Rect( 0, 0, bodyIndexFrameDesc.Width, bodyIndexFrameDesc.Height );
                bodyIndexColorStride = (int)(bodyIndexFrameDesc.Width * bodyIndexColorBytesPerPixel);

                bodyIndexColorBuffer = new byte[bodyIndexFrameDesc.LengthInPixels * bodyIndexColorBytesPerPixel];

                ImageBodyIndex.Source = bodyIndexColorImage;

                bodyIndexColors = new Color[]{
                    Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow, Colors.Pink, Colors.Purple,
                };

                // ボディーリーダーを開く
                bodyIndexFrameReader = kinect.BodyIndexFrameSource.OpenReader();
                bodyIndexFrameReader.FrameArrived += bodyIndexFrameReader_FrameArrived;
            }
            catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
                Close();
            }
        }

        private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            if ( bodyIndexFrameReader != null ) {
                bodyIndexFrameReader.Dispose();
                bodyIndexFrameReader = null;
            }

            if ( kinect != null ) {
                kinect.Close();
                kinect = null;
            }
        }


        void bodyIndexFrameReader_FrameArrived( object sender, BodyIndexFrameArrivedEventArgs e )
        {
            UpdateBodyIndexFrame( e );
            DrawBodyIndexFrame();
        }

        // ボディインデックスフレームの更新
        private void UpdateBodyIndexFrame( BodyIndexFrameArrivedEventArgs e )
        {
            using ( var bodyIndexFrame = e.FrameReference.AcquireFrame() ) {
                if ( bodyIndexFrame == null ) {
                    return;
                }

                // ボディインデックスデータを取得する
                bodyIndexFrame.CopyFrameDataToArray( bodyIndexBuffer );
            }
        }

        // ボディインデックスフレームの表示
        private void DrawBodyIndexFrame()
        {
            // ボディインデックスの値をカラーデータにする
            for ( int i = 0; i < bodyIndexBuffer.Length; i++ ) {
                var index = bodyIndexBuffer[i];
                var colorIndex = i * 4;

                if ( index != 255 ) {
                    var color = bodyIndexColors[index];
                    bodyIndexColorBuffer[colorIndex + 0] = color.B;
                    bodyIndexColorBuffer[colorIndex + 1] = color.G;
                    bodyIndexColorBuffer[colorIndex + 2] = color.R;
                    bodyIndexColorBuffer[colorIndex + 3] = 255;
                }
                else {
                    bodyIndexColorBuffer[colorIndex + 0] = 0;
                    bodyIndexColorBuffer[colorIndex + 1] = 0;
                    bodyIndexColorBuffer[colorIndex + 2] = 0;
                    bodyIndexColorBuffer[colorIndex + 3] = 255;
                }
            }

            bodyIndexColorImage.WritePixels( bodyIndexColorRect, bodyIndexColorBuffer, bodyIndexColorStride, 0 );
        }
    }
}
