using System.IO;
using System.Linq;
using SpotifyRecorderWPF.ViewModels;

namespace SpotifyRecorderWPF.Helper
{
    public class Settings
    {
        public static void Load ( MainViewModel viewModel )
        {
            viewModel.SelectedBitrate = viewModel.Bitrates.Contains(Properties.Settings.Default.SelectedBitrate) ? Properties.Settings.Default.SelectedBitrate : 128;
            viewModel.SelectedMmDeviceId = viewModel.MmDevices.FirstOrDefault(x => x.FriendlyName == Properties.Settings.Default.DeviceName)?.ID;
            viewModel.OutputFolder = Directory.Exists(Properties.Settings.Default.OutputPath) ? Properties.Settings.Default.OutputPath : null;
            viewModel.IsSkipCommertials = Properties.Settings.Default.SkipCommertials;
        }

        public static void Save ( MainViewModel viewModel )
        {
            Properties.Settings.Default.SelectedBitrate = viewModel.SelectedBitrate;
            Properties.Settings.Default.DeviceName = viewModel.SelectedMmDevice?.FriendlyName;
            Properties.Settings.Default.OutputPath = viewModel.OutputFolder;
            Properties.Settings.Default.SkipCommertials = viewModel.IsSkipCommertials;

            Properties.Settings.Default.Save();
        }

    }
}
