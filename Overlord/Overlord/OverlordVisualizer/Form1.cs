﻿using MySql.Data.MySqlClient;
using Overlord;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace OverlordVisualizer
{
	/// <summary>
	/// This is the main form class used for generating 3D visualization of Neural Network training data output.
	/// </summary>
	/// <seealso cref="System.Windows.Forms.Form" />
	public partial class Form1 : Form
    {
		/// <summary>
		/// The data identifier.
		/// </summary>
		private int _dataId;

		/// <summary>
		/// The axis x that we will use to retrieve Variable 1. Food, Wood, ...etc.
		/// </summary>
		private int _axisX;

		/// <summary>
		/// The axis y that we will use to retrieve Variable 2. Any other attribute that is not axis X.
		/// </summary>
		private int _axisY;

		/// <summary>
		/// The ordinal identifier. Just an index column used to help find data.
		/// </summary>
		private int _ordinalId;

		/// <summary>
		/// Initializes a new instance of the <see cref="Form1"/> class.
		/// </summary>
		public Form1()
        {
            InitializeComponent();
			// GenerateChart();
			GenerateNewChart(0,1);
		}


		/// <summary>
		/// Generates the chart. Old method just used to test drawing charts.
		/// </summary>
		void GenerateChart()
        {
            // Set 3D chart settings
            chart1.ChartAreas["Default"].Area3DStyle.Enable3D = true;
            chart1.ChartAreas["Default"].Area3DStyle.IsRightAngleAxes = false;
            
            chart1.ChartAreas["Default"].Area3DStyle.Inclination = 40;
            chart1.ChartAreas["Default"].Area3DStyle.Rotation = 20;
            chart1.ChartAreas["Default"].Area3DStyle.LightStyle = LightStyle.Realistic;
            chart1.ChartAreas["Default"].Area3DStyle.PointDepth = 1000;

            // Populate series with random data
            Random random = new Random();
            for (int pointIndex = 0; pointIndex < 10; pointIndex++)
            {
                chart1.Series["Series1"].Points.AddY(random.Next(45, 95));
                chart1.Series["Series2"].Points.AddY(random.Next(5, 75));
            }

            // Set series chart type
            chart1.Series["Series1"].ChartType = SeriesChartType.Line;
            chart1.Series["Series2"].ChartType = SeriesChartType.Line;

            // Set point labels
            chart1.Series["Series1"].IsValueShownAsLabel = true;
            chart1.Series["Series2"].IsValueShownAsLabel = true;

			// Enable X axis margin
			//chart1.ChartAreas["Default"].AxisX.IsMarginVisible = true;


			// Enable the ShowMarkerLines
			// chart1.Series["Series1"]["ShowMarkerLines"] = "true";
		    // chart1.Series["Series2"]["ShowMarkerLines"] = "true";
        }

		/// <summary>
		/// Generates the new chart. Is used to query data and plot the 3D chart.
		/// </summary>
		/// <param name="axisX">The axis x.</param>
		/// <param name="axisY">The axis y.</param>
		public void GenerateNewChart(int axisX, int axisY)
        {
			// First get the new max Id from  the datatable.
			int maxDataId = 0;
            int maxOrdinalId = 0;
			double toleranceLevel = 0;

            this.chart1.Series.Clear();
			
			string readCmd = string.Format(@"
                SELECT DataId, ToleranceLevel, AxisX, AxisY, OrdinalId
				FROM ai_plotset WHERE AxisX = {0} AND AxisY = {1}
					
				ORDER BY DataId DESC, OrdinalId DESC LIMIT 1;
			", axisX, axisY);


			ReadSql((MySqlDataReader msdr, MySqlCommand cmd) =>
			{
				if (msdr.Read() && !Convert.IsDBNull(msdr["DataId"]))
				{
					maxDataId = Convert.ToInt32(msdr["DataId"]);
					toleranceLevel = Convert.ToDouble(msdr["ToleranceLevel"]);
                    maxOrdinalId = Convert.ToInt32(msdr["OrdinalId"]);
                }
			}, readCmd);

            _dataId = maxDataId;
            _ordinalId = maxOrdinalId;
            _axisX = axisX;
            _axisY = axisY;

            // Read fresh data from database.
            string readPlotableSql = @"SELECT X,Y,Z  FROM ai_plotable_data WHERE DataId = " + maxDataId + " AND OrdinalId = " + maxOrdinalId + ";";
			List<VectorN> vectors = new List<VectorN>(10000); // initialized to 10,000 units. i.e. 100X100

			ReadSql((MySqlDataReader msdr, MySqlCommand cmd) =>
			{
				while (msdr.Read())
				{
					VectorN tempVector = new VectorN(
						new double[] {
							Convert.ToDouble(msdr["X"]),
							Convert.ToDouble(msdr["Y"]),
							Convert.ToDouble(msdr["Z"])
					});

					vectors.Add(tempVector);
				}
			}, readPlotableSql);

			// Take a list of vectors, and plot them!
			ChartArea chartArea1 = new ChartArea();
			List<Series> seriesSet = new List<Series>(100);

			//this.chart1 = new Chart();
			((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
			this.SuspendLayout();

			chart1.ChartAreas["Default"].Area3DStyle.Enable3D = true;
			chart1.ChartAreas["Default"].Area3DStyle.IsRightAngleAxes = false;

			chart1.ChartAreas["Default"].Area3DStyle.Inclination = 40;
			chart1.ChartAreas["Default"].Area3DStyle.Rotation = 20;
			chart1.ChartAreas["Default"].Area3DStyle.WallWidth = 10;
            chart1.ChartAreas["Default"].Area3DStyle.LightStyle = LightStyle.Realistic;
			chart1.ChartAreas["Default"].Area3DStyle.PointDepth = 100;
			//chartArea1.Name = "Default";
			//this.chart1.ChartAreas.Add(chartArea1);
			//this.chart1.Location = new System.Drawing.Point(12, 12);
			//this.chart1.Name = "chart1";

			int size = 100;
			double min = 100000;
			double max = 0;
			// adds 100 series!
			for (int i = 0; i < size; i++)
			{
				seriesSet.Add(new Series());
				seriesSet[i].ChartArea = "Default";
				seriesSet[i].Name = "" + i;

				for (int j = 0; j < size; j++)
				{
					if (vectors.Count > j + i * size)
					{
						double normalized = vectors[j + i * size][1];
                        seriesSet[i].Points.AddXY(j, normalized);

						if (normalized > max)
						{
							max = normalized;
						}

						if (normalized < min)
						{
							min = normalized;
						}
					}
				}

				for (int j = 0; j < size; j++)
				{
					if (vectors.Count > j + i * size)
					{
						double normalized = seriesSet[i].Points[j].YValues[0];
						double diff = max - min;
						// total is 255,  we need to find scaling factor.
						double redRange = (( normalized - min ) / diff) * 50; //red range!
						double greenRange = ((normalized - min) / diff) * 100; //red range!
						seriesSet[i].Points[j].Color = Color.FromArgb((int)redRange, (int)greenRange, 0);
					}
				}

				seriesSet[i].Legend = "Legend1";
				seriesSet[i].ChartType = SeriesChartType.Line;
				//seriesSet[i].Color = Color.FromArgb(100-i, i, 0);
				// Set point labels
				//seriesSet[i].IsValueShownAsLabel = true;

				this.chart1.Series.Add(seriesSet[i]);
			}


			// this.chart1.Size = new System.Drawing.Size(1319, 720);
			this.chart1.TabIndex = 0;
			((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
			this.ResumeLayout(false);
			this.Refresh();

		}

		/// <summary>
		/// This method allows me to make more database connections.
		/// Maybe I should keep one open in another method?
		/// </summary>
		/// <param name="buildDataSet">The build data set.</param>
		/// <param name="cmdString">The command string.</param>
		public static void ReadSql(Action<MySqlDataReader, MySqlCommand> buildDataSet, string cmdString)
        {
            MySqlConnection conn = new MySqlConnection(Configurations.ConnectionString);

            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(cmdString, conn);
                cmd.Prepare();
                MySqlDataReader msdr = cmd.ExecuteReader();
                // Gets all that good data.
                buildDataSet(msdr, cmd);
            }
            catch (MySqlException mse)
            {
                throw mse;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

		/// <summary>
		/// This button maps the Food, Wood axis to the X,Z coordinate (Y is up).
		/// These functions get the various combinations of pairs of Input variable to find the highest degree of corelation.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void button2_Click(object sender, EventArgs e)
        {
            GenerateNewChart(0, 1);
        }

		/// <summary>
		/// Handles the Click event of the Axis02Ordinal2 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Axis02Ordinal2_Click(object sender, EventArgs e)
        {
            GenerateNewChart(0, 2);
        }

		/// <summary>
		/// Handles the Click event of the Axis03Ordinal3 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Axis03Ordinal3_Click(object sender, EventArgs e)
        {
            GenerateNewChart(0, 3);
        }

		/// <summary>
		/// Handles the Click event of the Axis04Ordinal4 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Axis04Ordinal4_Click(object sender, EventArgs e)
        {
            GenerateNewChart(0, 4);
        }

		/// <summary>
		/// Handles the Click event of the Axis12Ordinal5 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Axis12Ordinal5_Click(object sender, EventArgs e)
        {
            GenerateNewChart(1, 2);
        }

		/// <summary>
		/// Handles the Click event of the Axis13Ordinal6 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Axis13Ordinal6_Click(object sender, EventArgs e)
        {
            GenerateNewChart(1, 3);
        }

		/// <summary>
		/// Handles the Click event of the Axis14Ordinal7 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Axis14Ordinal7_Click(object sender, EventArgs e)
        {
            GenerateNewChart(1, 4);
        }

		/// <summary>
		/// Handles the Click event of the Axis15Ordinal8 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Axis15Ordinal8_Click(object sender, EventArgs e)
        {
            GenerateNewChart(2, 3);
        }

		/// <summary>
		/// Handles the Click event of the Axis16Ordinal9 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Axis16Ordinal9_Click(object sender, EventArgs e)
        {
            GenerateNewChart(2, 4);
        }

		/// <summary>
		/// Handles the Click event of the Axis17Ordinal10 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Axis17Ordinal10_Click(object sender, EventArgs e)
        {
            GenerateNewChart(3, 4);
        }

		/// <summary>
		/// Handles the Click event of the button1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void button1_Click(object sender, EventArgs e)
        {
            AIOutputView aiView = new AIOutputView(_dataId, _axisX, _axisY, _ordinalId, 1);
            aiView.Show();
        }

		/// <summary>
		/// Handles the Click event of the SubGraph2 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void SubGraph2_Click(object sender, EventArgs e)
        {
            AIOutputView aiView = new AIOutputView(_dataId, _axisX, _axisY, _ordinalId, 2);
            aiView.Show();
        }

		/// <summary>
		/// Handles the Click event of the SubGraph3 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void SubGraph3_Click(object sender, EventArgs e)
        {
            AIOutputView aiView = new AIOutputView(_dataId, _axisX, _axisY, _ordinalId, 3);
            aiView.Show();
        }

		/// <summary>
		/// Handles the Click event of the SubGraph4 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void SubGraph4_Click(object sender, EventArgs e)
        {
            AIOutputView aiView = new AIOutputView(_dataId, _axisX, _axisY, _ordinalId, 4);
            aiView.Show();
        }

		/// <summary>
		/// Handles the Click event of the TurnLeft control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void TurnLeft_Click(object sender, EventArgs e)
		{
			if (chart1.ChartAreas["Default"].Area3DStyle.Rotation - 2 > -180)
			{
				chart1.ChartAreas["Default"].Area3DStyle.Rotation = chart1.ChartAreas["Default"].Area3DStyle.Rotation - 2;
			}
			else
			{
				chart1.ChartAreas["Default"].Area3DStyle.Rotation = 180;
			}
		}

		/// <summary>
		/// Handles the Click event of the TurnRight control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void TurnRight_Click(object sender, EventArgs e)
		{
			if (chart1.ChartAreas["Default"].Area3DStyle.Rotation + 2 < 180)
			{
				chart1.ChartAreas["Default"].Area3DStyle.Rotation = chart1.ChartAreas["Default"].Area3DStyle.Rotation + 2;
			}
			else
			{
				chart1.ChartAreas["Default"].Area3DStyle.Rotation = -180;
            }
		}
	}
}
