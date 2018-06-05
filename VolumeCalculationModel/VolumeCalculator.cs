/**
* VolumeCalculator.cs
* Contains business logic to compute Volume of Oil and Gas
* @file *
* @date    $Date::             $
* @version $Revision::    $
* @author  $Author::           $
*/
using System.Collections.Generic;
using Utils;
namespace VolumeCalculationModel
{
    public class VolumeCalculator
    {
        #region "Member Variables"
        private List<int> m_topHorizonDepthValues;
        private InputParameters m_volumeInputData;
        #endregion

        #region "Constructor"
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="i_voulumeInputData">Input parameters Data</param>
        /// <param name="i_topHorizonDepthValues">Depth values of Top Horizon</param>
        public VolumeCalculator(InputParameters i_voulumeInputData, List<int> i_topHorizonDepthValues)
        {
            this.m_volumeInputData = i_voulumeInputData;
            this.m_topHorizonDepthValues = i_topHorizonDepthValues;

        }
        #endregion

        #region Methods
        /// <summary>
        /// Computes total Volume Of Oil and gas Above Fuild Contact in reservoir 
        /// </summary>
        /// <returns></returns>
        public double ComputeVolume()
        {

            double totalDepthOfOilAndGas = 0.0;
            double volume = 0.0;

            foreach (long topHorizonDepthValue in m_topHorizonDepthValues)
            {
                totalDepthOfOilAndGas += ComputeDepthOfOilAndGasAtEachCell(m_volumeInputData, topHorizonDepthValue);
            }

            switch (m_volumeInputData.UnitsOfVolume)
            {
                case UnitofVolume.CubicFeet:
                    volume = m_volumeInputData.GridCellArea * totalDepthOfOilAndGas;
                    break;
                case UnitofVolume.CubicMeter:
                    volume = m_volumeInputData.GridCellArea * totalDepthOfOilAndGas * Constants.CubicFeetToCubicMeter;
                    break;
                case UnitofVolume.Barrel:
                    volume = m_volumeInputData.GridCellArea * totalDepthOfOilAndGas * Constants.CubicFeetToBarrel;
                    break;
                default:
                    break;
            }

            return volume;
        }

        /// <summary>
        /// Computes Oil and gas Depth based on Input Params and Top Horizon depth Value at each Grid Cell
        /// </summary>
        /// <param name="i_inputParam">Inpu parameters</param>
        /// <param name="i_topHorizonDepthValue">Top horizon depth value at each cell</param>
        /// <returns></returns>
        double ComputeDepthOfOilAndGasAtEachCell(InputParameters i_inputParams, double i_topHorizonDepthValue)
        {
            //Note:My Understanding in the problem is Fluid Contact Will Always exits only at 3000 meter depth from top of Reservoir , it will not exist above 3000m or below 3000 meters.

            double bottomHorizonDepthValue = i_topHorizonDepthValue + m_volumeInputData.BaseHorizonDiffValue;
            if (i_topHorizonDepthValue >= i_inputParams.FluidContact)
            {
                return 0.0;
            }
            else
            {
                return (bottomHorizonDepthValue > i_inputParams.FluidContact ? i_inputParams.FluidContact - i_topHorizonDepthValue : bottomHorizonDepthValue - i_topHorizonDepthValue);               
            }

            //Note:Above if and else Could have been written as below as commented, for better readability i have written like above.
            /*return i_topHorizonDepthValue >= i_inputParams.FluidContact ? 0.0 : bottomHorizonDepthValue > i_inputParams.FluidContact ? i_inputParams.FluidContact - i_topHorizonDepthValue : bottomHorizonDepthValue - i_topHorizonDepthValue;*/
        }
        #endregion
    }
}
