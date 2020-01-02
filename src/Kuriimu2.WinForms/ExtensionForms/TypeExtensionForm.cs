﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Kore.Batch;
using Kuriimu2.WinForms.ExtensionForms.Models;
using Kuriimu2.WinForms.Properties;

namespace Kuriimu2.WinForms.ExtensionForms
{
    public abstract partial class TypeExtensionForm<TExtension, TResult> : Form
    {
        private bool _isDirectory;
        private BatchExtensionProcessor<TExtension, TResult> _batchProcessor;
        private ParameterBuilder _parameterBuilder;

        protected abstract string TypeExtensionName { get; }

        public TypeExtensionForm()
        {
            InitializeComponent();

            _batchProcessor = new BatchExtensionProcessor<TExtension, TResult>(ProcessFile);
            _parameterBuilder = new ParameterBuilder(gbTypeExtensionParameters);

            var loadedExtensions = LoadExtensionTypes();
            foreach (var loadedExtension in loadedExtensions)
                cmbExtensions.Items.Add(loadedExtension);

            cmbExtensions.SelectedIndex = 0;

            Text = TypeExtensionName + " Extensions";
            label1.Text = Text + ":";
        }

        protected abstract IList<ExtensionType> LoadExtensionTypes();

        protected abstract TExtension CreateExtensionType(ExtensionType selectedExtension);

        protected abstract TResult ProcessFile(TExtension extensionType, string filePath);

        protected abstract void FinalizeProcess(IList<(string, TResult)> results, string rootDir);

        private void btnFolder_Click(object sender, EventArgs e)
        {
            var sfd = new FolderBrowserDialog
            {
                SelectedPath = Settings.Default.TypeExtensionLastDirectory
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            Settings.Default.TypeExtensionLastDirectory = sfd.SelectedPath;

            txtPath.Text = sfd.SelectedPath;
            _isDirectory = true;
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                InitialDirectory = Settings.Default.TypeExtensionLastDirectory,
                Filter = "All Files (*.*)|*.*"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            Settings.Default.TypeExtensionLastDirectory = Path.GetDirectoryName(ofd.FileName);

            txtPath.Text = ofd.FileName;
            _isDirectory = false;
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            Execute();
        }

        private void Execute()
        {
            if (!VerifyInput())
                return;

            var selectedType = (ExtensionType)cmbExtensions.SelectedItem;

            if (!TryParseParameters(selectedType.Parameters.Values.ToArray()))
                return;

            // Create type
            var createdType = CreateExtensionType(selectedType);

            ToggleUi(false);

            // Execute processing
            ExecuteInternal(txtPath.Text, _isDirectory, chkSubDirectories.Checked, createdType);

            ToggleUi(true);
        }

        private bool VerifyInput()
        {
            if (cmbExtensions.SelectedIndex < 0)
            {
                MessageBox.Show($"Select a {TypeExtensionName}.", $"No {TypeExtensionName}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(txtPath.Text))
            {
                MessageBox.Show("Select a file or directory to process.", "No file/folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private bool TryParseParameters(ExtensionTypeParameter[] parameters)
        {
            foreach (var parameter in parameters)
            {
                var control = gbTypeExtensionParameters.Controls.Find(parameter.Name, false)[0];

                // If type is an enum
                if (parameter.ParameterType.IsEnum)
                {
                    var enumName = ((ComboBox)control).SelectedText;
                    if (!Enum.IsDefined(parameter.ParameterType, enumName))
                        return false;

                    parameter.Value = Enum.Parse(parameter.ParameterType, enumName);
                    continue;
                }

                // If type is a boolean
                if (parameter.ParameterType == typeof(bool))
                {
                    parameter.Value = ((CheckBox)control).Checked;
                    continue;
                }

                // If type is text based
                var textBox = (TextBox)control;
                if (parameter.ParameterType == typeof(char))
                {
                    if (!char.TryParse(textBox.Text, out var result))
                        return false;

                    parameter.Value = result;
                    continue;
                }

                if (parameter.ParameterType == typeof(string))
                {
                    parameter.Value = textBox.Text;
                    continue;
                }

                if (!TryParseNumber(parameter.ParameterType, textBox.Text, out var value))
                    return false;

                parameter.Value = value;
            }

            return true;
        }

        private bool TryParseNumber(Type numberType, string stringValue, out object parsedValue)
        {
            parsedValue = null;

            var method = numberType.GetMethod("TryParse", new[] { typeof(string), numberType.MakeByRefType() });
            var inputObjects = new[] { stringValue, Activator.CreateInstance(numberType) };

            if (!(bool)method.Invoke(null, inputObjects))
                return false;

            parsedValue = inputObjects[1];
            return true;
        }

        private void ToggleUi(bool toggle)
        {
            cmbExtensions.Enabled = toggle;
            gbTypeExtensionParameters.Enabled = toggle;

            btnExecute.Enabled = btnFile.Enabled = btnFolder.Enabled = toggle;

            chkSubDirectories.Enabled = toggle;
            chkAutoExecute.Enabled = toggle;
        }

        private void ExecuteInternal(string path, bool isDirectory, bool searchSubDirectories, TExtension extensionType)
        {
            // Process all files
            var results = _batchProcessor.Process(path, isDirectory, searchSubDirectories, extensionType);

            // Finalize the processing/create a report
            FinalizeProcess(results, isDirectory ? path : Path.GetDirectoryName(path));
        }

        private void cmbExtensions_SelectedIndexChanged(object sender, EventArgs e)
        {
            _parameterBuilder.Reset();

            var extensionType = cmbExtensions.SelectedItem as ExtensionType;

            _parameterBuilder.AddParameters(extensionType.Parameters.Values.ToArray());
        }

        private void TypeExtensionForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            var path = paths[0];
            txtPath.Text = path;

            _isDirectory = Directory.Exists(path);

            if (chkAutoExecute.Checked)
                Execute();

        }

        private void TypeExtensionForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }
}
