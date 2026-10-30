using Android.App;
using Android.Content.PM;
using Android.Net.Wifi;
using Android.OS;
using Android.Views;
using System;

using Microsoft.Xna.Framework;

namespace VectorRumble
{
    /// <summary>
    /// The main activity for the Android application. It initializes the game instance,
    /// sets up the rendering view, and starts the game loop.
    /// </summary>
    /// <remarks>
    /// This class is responsible for managing the Android activity lifecycle and integrating
    /// with the MonoGame framework.
    /// </remarks>
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        Theme = "@style/Theme.Splash",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden
    )]
    public class MainActivity : AndroidGameActivity
    {
        private VectorRumbleGame _game;
        private View _view;
        private WifiManager.MulticastLock _multicastLock;

        /// <summary>
        /// Called when the activity is first created. Initializes the game instance,
        /// retrieves its rendering view, and sets it as the content view of the activity.
        /// Finally, starts the game loop.
        /// </summary>
        /// <param name="bundle">A Bundle containing the activity's previously saved state, if any.</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Android drops broadcast packets unless a multicast lock is held; finding LAN games needs them.
            var wifi = (WifiManager)GetSystemService(WifiService);
            _multicastLock = wifi?.CreateMulticastLock("VectorRumble.LanDiscovery");
            _multicastLock?.SetReferenceCounted(false);
            _multicastLock?.Acquire();

            _game = new VectorRumbleGame();
            _view = _game.Services.GetService(typeof(View)) as View;

            SetContentView(_view);
            HideSystemBars();
            _game.Run();
        }

        /// <summary>
        /// The game uses the whole screen. With the system's navigation bar showing, the view is shorter than the
        /// screen the game believes it has, and the bottom of the picture is lost behind the bar.
        /// </summary>
        private void HideSystemBars()
        {
            // draw into the notch area too: the game's picture keeps its shape, and the bars it leaves
            // at the sides take up the notch
            if (Window?.Attributes != null && OperatingSystem.IsAndroidVersionAtLeast(28))
            {
                Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
            }

            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                Window.SetDecorFitsSystemWindows(false);
                var controller = Window.InsetsController;
                if (controller != null)
                {
                    controller.Hide(WindowInsets.Type.SystemBars());
                    controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                }
            }
            else
            {
#pragma warning disable CS0618 // the older way, for devices before Android 11
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                    SystemUiFlags.ImmersiveSticky | SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen |
                    SystemUiFlags.LayoutStable | SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutFullscreen);
#pragma warning restore CS0618
            }
        }

        /// <summary>
        /// The system brings its bars back when the app loses focus (a swipe from the edge, a dialog); hide them
        /// again when it returns.
        /// </summary>
        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus)
            {
                HideSystemBars();
            }
        }

        protected override void OnDestroy()
        {
            if (_multicastLock != null && _multicastLock.IsHeld)
                _multicastLock.Release();
            base.OnDestroy();
        }
    }
}
