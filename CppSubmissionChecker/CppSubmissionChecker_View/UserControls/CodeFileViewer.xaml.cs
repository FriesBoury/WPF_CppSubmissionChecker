﻿using CppSubmissionChecker_ViewModel;
using CppSubmissionChecker_ViewModel.Viewmodels.FilePreview;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Xml;

namespace CppSubmissionChecker_View.UserControls
{
    /// <summary>
    /// Interaction logic for CodeFileViewer.xaml
    /// </summary>
    public partial class CodeFileViewer : UserControl
    {
        private CodeFileViewer_VM? _viewModel;
        public CodeFileViewer()
        {
            this.DataContextChanged += CodeFileViewer_DataContextChanged;
            InitializeComponent();

        }

        private void CodeFileViewer_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as CodeFileViewer_VM;
        }

        public void OpenFile(string? path)
        {
            _viewModel?.AddCodeFile(path);
            _emptyTabControl.Visibility = Visibility.Hidden;
        }

        public void CloseAllFiles()
        {
            _viewModel?.Clear();
            _emptyTabControl.Visibility = Visibility.Visible;
        }

        private void File_Close(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement visual)
            {
                if (visual.DataContext is FilePreviewBaseVM fileVm)
                {
                    fileVm.Close();
                    if (_viewModel != null && _viewModel.CodeFiles.Count == 0)
                    {
                        _emptyTabControl.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        #region CodeFile

        private IHighlightingDefinition? LoadAvalonEditSyntaxHighlighting(string language)
        {
            //if (_syntaxHighlighting != null) return;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            var resourceName = language switch 
            {
				"xml" => $"CppSubmissionChecker_View.Resources.XML_Dark.xml",
				"json" => $"CppSubmissionChecker_View.Resources.JSON_Dark.xml",
				"c#" => $"CppSubmissionChecker_View.Resources.CSharp_Dark.xml",
                _=> $"CppSubmissionChecker_View.Resources.CSharp_Dark.xml"
			};
            using (Stream? s = assembly.GetManifestResourceStream(resourceName))
            {
                if (s == null)
                    return null;

                using (XmlReader reader = XmlReader.Create(s, new XmlReaderSettings { }))
                {
                    return ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);

                }
            }

        }
        private void _fileTxt_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
			if (sender is TextEditor txtEditor && txtEditor.DataContext is CodeFile_VM vm)
			{
				txtEditor.Text = vm.FileContent;
				txtEditor.SyntaxHighlighting = LoadAvalonEditSyntaxHighlighting(vm.SyntaxHighlighting);
				txtEditor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.White);
			}
		}

        private void _fileTxt_Loaded(object sender, RoutedEventArgs e)
        {

            if (sender is TextEditor txtEditor && txtEditor.DataContext is CodeFile_VM vm)
            {
                txtEditor.Text = vm.FileContent;
                txtEditor.SyntaxHighlighting = LoadAvalonEditSyntaxHighlighting(vm.SyntaxHighlighting);
				txtEditor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.White);
            }

        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var codeFile_vm = ((sender as FrameworkElement)?.DataContext as CodeFile_VM);
            string? path = codeFile_vm?.Path;
            string? text = codeFile_vm?.FileContent;

            if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(text))
            {
                File.WriteAllText(path, text);
            }
        }

        private void ShowInExplorer_Click(object sender, RoutedEventArgs e)
        {
           
        }
        #endregion

        private void _fileTxt_TextChanged(object sender, EventArgs e)
        {
            TextEditor? editor = sender as TextEditor;
            if (editor == null) return;

            CodeFile_VM? file_vm = _viewModel?.SelectedCodeFile as CodeFile_VM;
            if (file_vm != null) file_vm.FileContent = editor.Text;
        }
    }
}
