﻿using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;

namespace QueryFirstVSExt
{
    public class VSOutputWindow
    {
        private OutputWindowPane _outPane;
        public VSOutputWindow(DTE2 env)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OutputWindow outputWindow = env.ToolWindows.OutputWindow;

            for (uint i = 1; i <= outputWindow.OutputWindowPanes.Count; i++)
            {
                if (outputWindow.OutputWindowPanes.Item(i).Name.Equals("QueryFirstVSExt", StringComparison.CurrentCultureIgnoreCase))
                {
                    _outPane = outputWindow.OutputWindowPanes.Item(i);
                    break;
                }
            }

            if (_outPane == null)
                _outPane = outputWindow.OutputWindowPanes.Add("QueryFirstVSExt");

        }
        public void Write(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _outPane.OutputString(message);
        }
    }
}