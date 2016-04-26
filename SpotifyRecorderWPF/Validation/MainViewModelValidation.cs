using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyRecorderWPF.ViewModels;

namespace SpotifyRecorderWPF.Validation
{
    public class MainViewModelValidation
    {
        private readonly MainViewModel _viewModel;
        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

        public MainViewModelValidation ( MainViewModel viewModel )
        {
            _viewModel = viewModel;
        }

        public void ValidateOutputFolder()
        {
            if (string.IsNullOrEmpty(_viewModel.OutputFolder))
            {
                _validationErrors["OutputFolder"] = new List<string>() { "Output Folder can not be empty!" };
                _viewModel.RaiseErrorsChanged("OutputFolder");
            }
            else if (!Directory.Exists(_viewModel.OutputFolder))
            {
                _validationErrors["OutputFolder"] = new List<string>() { "Output Folder is not a valid Folder!" };
                _viewModel.RaiseErrorsChanged("OutputFolder");
            }
            else
            {
                if (_validationErrors.ContainsKey("OutputFolder"))
                {
                    _validationErrors.Remove("OutputFolder");
                }
            }
        }

        
    }
}
