using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyRecorderWPF.ObjectModel
{
    public class ValidationRule
    {
        private readonly Func< bool > _rule;

        public ValidationRule (string propertyName, string errorMessage, Func<bool> rule )
        {
            _rule = rule;
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }

        public string PropertyName { get; }
        public string ErrorMessage { get; }

        public bool Apply ( )
        {
            return _rule ( );
        }
    }
}
