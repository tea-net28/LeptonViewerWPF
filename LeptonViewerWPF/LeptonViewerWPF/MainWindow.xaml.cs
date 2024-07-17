using System.Windows;
using System.Windows.Input;

namespace LeptonViewerWPF
{
    public partial class MainWindow : Window
    {
        MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            // マウスカーソルが各 Image コンポーネント上にきたときに実行する処理を追加
            LeptonImage1.MouseMove += OnMouseMove_LeptonImage1;
            LeptonImage2.MouseMove += OnMouseMove_LeptonImage2;
            LeptonImage3.MouseMove += OnMouseMove_LeptonImage3;

            this.Loaded += OnLoaded_MainWindow;
        }

        private void OnLoaded_MainWindow(object sender, RoutedEventArgs e)
        {
            _mainViewModel = (MainViewModel)this.DataContext;
        }

        private void OnMouseMove_LeptonImage1(object sender, MouseEventArgs e)
        {
            _mainViewModel.OnMouseMove_LeptonImage(e, LeptonImage1.ActualWidth, LeptonImage1.ActualHeight, e.GetPosition(LeptonImage1));
        }

        private void OnMouseMove_LeptonImage2(object sender, MouseEventArgs e)
        {
            _mainViewModel.OnMouseMove_LeptonImage(e, LeptonImage2.ActualWidth, LeptonImage2.ActualHeight, e.GetPosition(LeptonImage2));
        }

        private void OnMouseMove_LeptonImage3(object sender, MouseEventArgs e)
        {
            _mainViewModel.OnMouseMove_LeptonImage(e, LeptonImage3.ActualWidth, LeptonImage3.ActualHeight, e.GetPosition(LeptonImage3));
        }
    }
}