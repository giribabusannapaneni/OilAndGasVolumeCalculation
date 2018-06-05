/**
* InputParameters.cs
* Contains Entity encapsulates Inputparameters to calculate Volume
* @file *
* @date    $Date::             $
* @version $Revision::    $
* @author  $Author::           $
*/
using Utils;

namespace VolumeCalculationModel
{
    #region "Enums"
    public enum UnitofVolume { CubicFeet, CubicMeter, Barrel };
    #endregion
    public class InputParameters
    {
        #region MemeberVariables
        private UnitofVolume m_unitofVolume;
        private double m_GridCellArea;
        private double m_BaseHorizonDiffValue;
        private double m_FluidContact;
        #endregion

        #region "Constructor"
        public InputParameters(UnitofVolume i_unitOfVolume, double i_GridCellArea, double i_BaseHorizonDiffValue, double i_FluidContact)
        {
            this.m_unitofVolume = i_unitOfVolume;
            this.m_GridCellArea = i_GridCellArea;
            this.m_BaseHorizonDiffValue = i_BaseHorizonDiffValue * Constants.MeterToFeet;
            this.m_FluidContact = i_FluidContact * Constants.MeterToFeet;
        }
        #endregion

        #region "Properties"       
        public UnitofVolume UnitsOfVolume
        {
            get { return m_unitofVolume; }
        }
        public double GridCellArea
        {
            get { return m_GridCellArea; }
        }

        public double BaseHorizonDiffValue
        {
            get { return m_BaseHorizonDiffValue; }
        }

        public double FluidContact
        {
            get { return m_FluidContact; }
        }
        #endregion

    }
}
