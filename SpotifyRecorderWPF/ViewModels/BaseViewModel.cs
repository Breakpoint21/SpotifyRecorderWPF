using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SpotifyRecorderWPF.ObjectModel;

namespace SpotifyRecorderWPF.ViewModels
{
    public class BaseViewModel : INotifyDataErrorInfo
    {
        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

        protected readonly List<ValidationRule> Rules = new List< ValidationRule > (); 

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)
            || !_validationErrors.ContainsKey(propertyName))
                return null;

            return _validationErrors[propertyName];
        }

        public bool HasErrors { get { return _validationErrors.Count > 0; } }
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public void RaiseErrorsChanged([CallerMemberName]  string propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void Validate ( )
        {
            foreach ( var rule in Rules )
            {
                if ( rule.Apply() )
                {
                    AddValidationError ( rule.PropertyName, rule.ErrorMessage );
                }
                else
                {
                    FreeValidationError ( rule.PropertyName, rule.ErrorMessage );
                }
            }
        }

        protected void Validate ( [ CallerMemberName ] string propertyName = null )
        {
            foreach (var rule in Rules.Where(rule => rule.PropertyName == propertyName))
            {
                if (rule.Apply())
                {
                    AddValidationError(rule.PropertyName, rule.ErrorMessage);
                }
                else
                {
                    FreeValidationError(rule.PropertyName, rule.ErrorMessage);
                }
            }
        }

        private void AddValidationError ( string propertyName, string error )
        {
            if (!_validationErrors.ContainsKey(propertyName) || _validationErrors[propertyName] == null )
            {
                _validationErrors[propertyName] = new HashSet< string > ();
            }
            _validationErrors[propertyName].Add(error);
        }

        private void FreeValidationError ( string propertyName, string error )
        {
            if ( _validationErrors.ContainsKey(propertyName) && _validationErrors[propertyName] != null )
            {
                _validationErrors [ propertyName ].Remove ( error );
            }
        }
    }
}
