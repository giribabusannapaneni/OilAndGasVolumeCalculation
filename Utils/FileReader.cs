/**
* Constants.cs
* Contains File access related functions and validations related to File data
* @file *
* @date    $Date::             $
* @version $Revision::    $
* @author  $Author::           $
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;
namespace Utils
{
    public class FileReader
    {
        #region "Constructor"
        public FileReader()
        {
            m_fileData = string.Empty;
            m_topHorizonData = new List<int>();
            m_errorMessage = new StringBuilder();
        }
        #endregion
        #region "Private members"
        string m_fileData;
        List<int> m_topHorizonData;
        StringBuilder m_errorMessage;
        #endregion

        #region "Methods"
        /// <summary>
        /// Open File Dailog window to allow user to select file       
        /// </summary>
        public void OpenFileDialogWindow()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.ShowReadOnly = true;
                openFileDialog.ReadOnlyChecked = true;
                openFileDialog.CheckFileExists = true;
                openFileDialog.CheckPathExists = true;
                openFileDialog.DefaultExt = "csv";
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                { m_fileData = File.ReadAllText(openFileDialog.FileName); }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Returns File data in string Format
        /// </summary>
        /// <returns></returns>
        public string GetFileData()
        {
            return m_fileData;
        }
        /// <summary>
        /// Returns Parsed File Data into List
        /// </summary>
        /// <returns></returns>
        public List<int> GetTopHorizonData()
        {
            return m_topHorizonData;
        }
        /// <summary>
        /// Validates File Data
        /// </summary>
        /// <returns></returns>
        public bool isValidData()
        {
            try
            {
                m_errorMessage.Clear();
                short dataRowIndex = 0;              
               
                if(!string.IsNullOrEmpty(m_fileData.ToString().Trim()) && !string.IsNullOrWhiteSpace(m_fileData.ToString().Trim()))
                {
                    //split  data into rows
                    var rowData = Regex.Split(m_fileData, "\r\n|\r|\n");
                    List<string> dataRows = rowData.ToList();
                    foreach (string strRowItem in dataRows)
                    {
                        //split Row data into columns
                        List<string> dataColumns = strRowItem.Split(new char[0]).ToList();
                        short dataColIndex = 0;
                        foreach (string strColumn in dataColumns)
                        {
                            int result = 0;
                            if (int.TryParse(strColumn, out result))
                            {
                                if (result > 0)
                                {
                                    m_topHorizonData.Add(result);
                                }
                                else
                                {
                                    m_errorMessage.AppendLine();
                                    m_errorMessage.Append("File Data at Row " + (dataRowIndex + 1) + " and column " + (dataColIndex + 1) + " should be greater than zero");
                                }
                            }
                            else
                            {
                                m_errorMessage.AppendLine();
                                m_errorMessage.Append("File Data at Row " + (dataRowIndex + 1) + " and column " + (dataColIndex + 1) + " is not a number");
                            }
                            dataColIndex++;
                            dataRowIndex++;
                        }
                    }
                }
                if (m_errorMessage.Length > 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Returns error messages
        /// </summary>
        /// <returns></returns>
        public StringBuilder GetErrorMessages()
        {
            return m_errorMessage;
        }
        #endregion
    }

}

