/**
* VolumeCalculationViewModel.cs
* View model for view ,contains commands and binding properties.
* @file *
* @date    $Date::             $
* @version $Revision::    $
* @author  $Author::           $
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using Utils;
using VolumeCalculationModel;
using System.Reflection;
using System.Configuration;
using System.Text;

namespace VolumeCalculationViewModels
{
    /// <summary>
    /// Delegate Command to bind to buttons in View
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action action;
        private Action<object> buttonClickCommandHandler;
        private Predicate<object> predicate;

        public DelegateCommand(Action i_action)
        {
            action = i_action;
        }

        public DelegateCommand(Action<object> i_buttonClickCommandHandler)
        {
            buttonClickCommandHandler = i_buttonClickCommandHandler;
        }

        public DelegateCommand(Action<object> i_buttonClickCommandHandler, Predicate<object> i_predicate) : this(i_buttonClickCommandHandler)
        {
            predicate = i_predicate;
        }

        public void Execute(object parameter)
        {
            switch ((parameter.ToString().Trim()))
            {
                case "OpenFile":
                    action();
                    break;
                default:
                    buttonClickCommandHandler(parameter);
                    break;
            }

        }
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        public bool CanExecute(object parameter)
        {
            if (parameter.ToString().Trim().Equals("Calculate Volume"))
            {
                return predicate(parameter);
            }
            return true;

        }

#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67
    }
    /// <summary>
    /// VolumeCalculationViewModel for view
    /// </summary>
    public class VolumeCalculationViewModel : INotifyPropertyChanged, IDataErrorInfo
    {

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private Member variables      
        private string m_fileData;
        private List<int> m_TopHorizonDepthValues;
        public double m_volume;
        private bool m_cubicFeet;
        private bool m_cubicMeter;
        private bool m_barrel;
        private double m_gridCellArea;
        private double m_twoHorizonsDepthDiff;
        private double m_fluidContactDepth;
        private StringBuilder m_errorMessages;
        private string m_errorInfo;
        private bool m_isValuesReadFromConfig;      
        #endregion

        #region Methods

        #region "Constructor"
        public VolumeCalculationViewModel()
        {
            CommonBttonCommand = new DelegateCommand(GetButtonClickCommandHandler(), GetValidation());
            CubicFeet = true;
            m_errorMessages = new StringBuilder();
            m_isValuesReadFromConfig = false;
        }
        #endregion
        private Action<object> GetButtonClickCommandHandler()
        {
            return ButtonClickCommandHandler;
        }
        private System.Predicate<object> GetValidation()
        {
            return Validation;
        }
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler CanExecuteChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            // take a copy to prevent thread issues
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void OpenFileDialogWindow()
        {
            try
            {
                m_errorMessages.Clear();
                FileReader objFileReader = new FileReader();
                objFileReader.OpenFileDialogWindow();
                if (objFileReader.isValidData())
                {
                    FileData = objFileReader.GetFileData();
                    m_TopHorizonDepthValues = objFileReader.GetTopHorizonData();
                }
                else
                {
                    m_errorMessages = objFileReader.GetErrorMessages();
                    FileData = string.Empty;
                    Volume = 0.0;
                }
                Error = m_errorMessages.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Handles Volume Calculate Volume(Oil/Gas) and Exit Buttons Interactions
        /// </summary>
        /// <param name="parameter"></param>
        public void ButtonClickCommandHandler(object parameter)
        {
            try
            {
                switch ((parameter.ToString().Trim()))
                {
                    case "Calculate Volume":
                        if (m_TopHorizonDepthValues != null && m_TopHorizonDepthValues.Count > 0)
                        {
                            UnitofVolume unitofVolume = UnitofVolume.CubicFeet;
                            if (CubicMeter)
                            {
                                unitofVolume = UnitofVolume.CubicMeter;
                            }
                            else if (Barrel)
                            {
                                unitofVolume = UnitofVolume.Barrel;
                            }
                            if (!m_isValuesReadFromConfig)
                            {
                                ReadConfigurationValues();
                            }
                            InputParameters objParams = new InputParameters(unitofVolume, m_gridCellArea, m_twoHorizonsDepthDiff, m_fluidContactDepth);
                            VolumeCalculator objVCalc = new VolumeCalculator(objParams, m_TopHorizonDepthValues);
                            Volume = objVCalc.ComputeVolume();
                        }
                        break;
                    case "Exit":
                        Application.Current.Shutdown();
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Validates user input before calculation
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool Validation(object parameter)
        {
            if (parameter != null && parameter.ToString().Trim().Equals("Calculate Volume") && string.IsNullOrEmpty(FileData))
            {
                m_errorMessages.AppendLine();
                m_errorMessages.Append("Please Load File Data");
                Error = m_errorMessages.ToString();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Reads parameters to calculate volume from Config file.
        /// </summary>
        private void ReadConfigurationValues()
        {
            var exePath = Assembly.GetEntryAssembly().Location;
            var config = ConfigurationManager.OpenExeConfiguration(exePath);
            double cellsize_Horizontal;
            double cellsize_Vertical;
            if (ConfigurationManager.AppSettings["cellsize_Horizontal"] != null && ConfigurationManager.AppSettings["cellsize_Vertical"] != null && double.TryParse(config.AppSettings.Settings["cellsize_Horizontal"].Value, out cellsize_Horizontal)
                  && double.TryParse(config.AppSettings.Settings["cellsize_Vertical"].Value, out cellsize_Vertical))
            {
                m_gridCellArea = cellsize_Horizontal * cellsize_Vertical;
            }
            else
            {
                m_errorMessages.AppendLine();
                m_errorMessages.Append("cellsize_Horizontal and cellsize_Vertical not configured in App.Config");
                Error = m_errorMessages.ToString();
                return;
            }

            if (ConfigurationManager.AppSettings["Diff_Between_Top_Bottom_Horizons_Depth"] != null &&
                !double.TryParse(config.AppSettings.Settings["Diff_Between_Top_Bottom_Horizons_Depth"].Value, out m_twoHorizonsDepthDiff))
            {
                m_errorMessages.AppendLine();
                m_errorMessages.Append("Diff_Between_Top_Bottom_Horizons_Depth not configured in App.Config");
                Error = m_errorMessages.ToString();
                return;
            }

            if (ConfigurationManager.AppSettings["Fluid_contact_Depth"] != null &&
                !double.TryParse(config.AppSettings.Settings["Fluid_contact_Depth"].Value, out m_fluidContactDepth))
            {
                m_errorMessages.AppendLine();
                m_errorMessages.Append("Fluid_contact_Depth not configured in App.Config");
                Error = m_errorMessages.ToString();
                return;
            }
            m_isValuesReadFromConfig = true;
        }

        #endregion

        #region Commands
        public ICommand OpenFilCommand
        {
            get { return new DelegateCommand(OpenFileDialogWindow); }
        }
        public ICommand CommonBttonCommand
        {
            get; set;            
        }
        #endregion

        #region Properties

        public string FileData
        {
            get
            {
                return m_fileData;
            }

            set
            {
                m_fileData = value;
                RaisePropertyChanged("FileData");
                (CommonBttonCommand as DelegateCommand).RaiseCanExecuteChanged();
            }
        }

        public double Volume
        {
            get
            {
                return m_volume;
            }
            set
            {
                m_volume = value;
                RaisePropertyChanged("Volume");
            }
        }

        public bool CubicFeet
        {
            get
            {
                return m_cubicFeet;
            }

            set
            {
                m_cubicFeet = value;
            }
        }

        public bool CubicMeter
        {
            get
            {
                return m_cubicMeter;
            }

            set
            {
                m_cubicMeter = value;
            }
        }

        public bool Barrel
        {
            get
            {
                return m_barrel;
            }

            set
            {
                m_barrel = value;
            }
        }

        public string Error
        {
            get
            {
                return m_errorInfo;
            }
            set
            {
                m_errorInfo = value;
                RaisePropertyChanged("Error");
            }
        }

        public string this[string columnName]
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        #endregion


    }
}
