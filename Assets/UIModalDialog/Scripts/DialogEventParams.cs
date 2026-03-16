using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.UIModalDialog.Scripts
{
    /// <summary>
    /// General dialog event arguments
    /// </summary>
    public class DialogEventArgs : EventArgs
    {
        public DialogEventArgs()
        {
        }

        public Transform _dialogPanel = null;

        public Transform DialogPanel
        {
            get { return _dialogPanel; }
            internal set { _dialogPanel = value; }
        }

        private String _dialogName = null;

        public String DialogName
        {
            get { return _dialogName; }
            internal set { _dialogName = value; }
        }
    }

    /// <summary>
    /// Event argument for dialog is closed
    /// </summary>
    public class DialogClosedEventArgs : EventArgs
    {
        public DialogClosedEventArgs()
        {
        }

        private String _dialogName = null;

        public String DialogName
        {
            get { return _dialogName; }
            internal set { _dialogName = value; }
        }

        private Button _closeButton = null;

        public Button CloseButton
        {
            get { return _closeButton; }
            internal set { _closeButton = value; }
        }
    }

    /// <summary>
    /// Event arguments for closing modal dialog
    /// </summary>
    public class DialogClosingEventArgs : EventArgs
    {
        private bool _cancel = false;

        public DialogClosingEventArgs()
        {
            _cancel = false;
        }

        private Button _closeButton = null;

        public Button CloseButton
        {
            get { return _closeButton; }
            internal set { _closeButton = value; }
        }

        public Transform _dialogPanel = null;

        public Transform DialogPanel
        {
            get { return _dialogPanel; }
            internal set { _dialogPanel = value; }
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
    }
}
