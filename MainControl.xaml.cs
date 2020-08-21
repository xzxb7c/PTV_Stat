using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace PTV_Stat
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        public StructureSet set = null;
        public PlanningItem plan = null;
        public PlanSetup PS = null;
        public PlanSum SumPlan = null;
        public int comboindex = 0;
        public bool isPsum = false;
        public string stType = null;
        public DoseValue planTD = new DoseValue();



        List<dvh_stats> dvhs = new List<dvh_stats>();
        List<string> planT = new List<string>();

        
        public MainControl()
        {
            InitializeComponent();

        }

        public void ComputeTargets()
        {
            
            dvhs.Clear();
            planT.Clear();
             
            if (structCmb.SelectedIndex != -1)
            {
                double outputDose = 0;


                if (!isPsum)
                {
                    cb_doseAbs.IsEnabled = true;
                    PS = (PlanSetup)plan;
                    planT.Add(PS.TargetVolumeID);

                }
                else
                {

                    cb_doseAbs.IsChecked = true;
                    cb_doseAbs.IsEnabled = false;

                    SumPlan = (PlanSum)plan;

                    foreach (var pss in SumPlan.PlanSetups)
                    {

                        planT.Add(pss.TargetVolumeID);
                      
                    }

                }

                Double.TryParse(tbRxdose.Text, out outputDose);

                DoseValue totalDose = new DoseValue(outputDose, DoseValue.DoseUnit.cGy);
               
                foreach (var st in plan.StructureSet.Structures)
                {


                    if (!st.IsEmpty && (st.DicomType.Contains("PTV") || st.DicomType.Contains("CTV")))
                    {

                        //Structure s = plan.StructureSet.Structures.First(x => x.Id == structCmb.SelectedItem.ToString());

                        DVHData dvhData = plan.GetDVHCumulativeData(st,
                                             DoseValuePresentation.Absolute,
                                             VolumePresentation.Relative, 1);


                        DoseValue maxdose35 = plan.GetDoseAtVolume(st, 0.035, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute);

                        //  MessageBox.Show("B " + st.Id);




                        if (planT.FirstOrDefault(s => s == st.Id) != null)
                        {
                            stType = "Target " + st.DicomType;
                            if (isPsum)
                            {
                                foreach (var pss in SumPlan.PlanSetups)
                                {
                                    if (pss.TargetVolumeID == st.Id)
                                        planTD = pss.TotalDose;
                                }
                                dvhs.Add(new dvh_stats
                                {
                                    //  stLine = new SolidColorBrush(st.Color),
                                    StructureID = st.Id + " @ " + planTD.ToString(),
                                    structType = stType,
                                    V100 = Math.Round(plan.GetVolumeAtDose(st, planTD, VolumePresentation.Relative), 2).ToString() + "%",
                                    V95 = Math.Round(plan.GetVolumeAtDose(st, 0.95 * planTD, VolumePresentation.Relative), 2).ToString() + "%",
                                    V90 = Math.Round(plan.GetVolumeAtDose(st, 0.9 * planTD, VolumePresentation.Relative), 2).ToString() + "%",
                                    // RX_Dose = outputDose.ToString() + " cGy",
                                    //   D100_Dose = plan.GetDoseAtVolume(st, 100, VolumePresentation.Relative, DoseValuePresentation.Absolute).ToString(),
                                    MaxDose = plan.GetDoseAtVolume(st, 0.035, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute).ToString(),
                                    MinDose = dvhData.MinDose.ToString(),
                                    MeanDose = dvhData.MinDose.ToString(),
                                    dataColor = System.Windows.Media.Brushes.Blue
                                });
                            }

                        }
                        else
                            stType = st.DicomType;

                        dvhs.Add(new dvh_stats
                        {
                            //   stLine = new SolidColorBrush(st.Color),
                            StructureID = st.Id + " @ " + totalDose.ToString(),
                            structType = stType,
                            V100 = Math.Round(plan.GetVolumeAtDose(st, totalDose, VolumePresentation.Relative), 2).ToString() + "%",
                            V95 = Math.Round(plan.GetVolumeAtDose(st, 0.95 * totalDose, VolumePresentation.Relative), 2).ToString() + "%",
                            V90 = Math.Round(plan.GetVolumeAtDose(st, 0.9 * totalDose, VolumePresentation.Relative), 2).ToString() + "%",
                            // RX_Dose = outputDose.ToString() + " cGy",
                            //   D100_Dose = plan.GetDoseAtVolume(st, 100, VolumePresentation.Relative, DoseValuePresentation.Absolute).ToString(),
                            MaxDose = plan.GetDoseAtVolume(st, 0.035, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute).ToString(),
                            MinDose = dvhData.MinDose.ToString(),
                            MeanDose = dvhData.MinDose.ToString(),
                            dataColor = System.Windows.Media.Brushes.Black
                        });

                    }
                }
                dvh_sp.DataContext = dvhs;
                dvh_sp.Items.Refresh();
            }
        }

        private void print_btn_Click(object sender, RoutedEventArgs e)
        {

            PrintDialog pd = new PrintDialog();
            pd.PrintVisual(this, "DVH Report");
        }



        public class dvh_stats
        {
            public SolidColorBrush stLine { get; set; }
            public string StructureID { get; set; }

            public string structType { get; set; }

            public string V100 { get; set; }

            public string V95 { get; set; }

            public string V90 { get; set; }

            public string MaxDose { get; set; }
            public string MinDose { get; set; }
            public string MeanDose { get; set; }
            public SolidColorBrush dataColor { get; set; }
        }


        private void ComputeDVM_Click(object sender, RoutedEventArgs e)
        {

            string structure = structCmb.SelectedItem.ToString();
            Structure st = null;

            double dose = 0;
            double volume = 0;
            double volume2 = 0;
            var doseUnit = DoseValue.DoseUnit.cGy;
            var dosePres = DoseValuePresentation.Absolute;
            var volPres = VolumePresentation.AbsoluteCm3;
            string volPresTxt = "cc";


            foreach (var planst in plan.StructureSet.Structures)
            {
                if (planst.Id == structure)
                    st = planst;
            }

            volPres = cb_volAbs.IsChecked == true ? VolumePresentation.AbsoluteCm3 : VolumePresentation.Relative;

            if (cb_volAbs.IsChecked == true)
            {
                volPres = VolumePresentation.AbsoluteCm3;
                volPresTxt = "cc";
            }
            else
            {
                volPres = VolumePresentation.Relative;
                volPresTxt = "%";
            }

            doseUnit = cb_doseAbs.IsChecked == true ? DoseValue.DoseUnit.cGy : DoseValue.DoseUnit.Percent;
            dosePres = cb_doseAbs.IsChecked == true ? DoseValuePresentation.Absolute : DoseValuePresentation.Relative;


            if (cb_VolDose.IsChecked == true)
            {

                if (tbDose.Text != null)
                {
                    Double.TryParse(tbDose.Text, out dose);

                    DoseValue doseVal = new DoseValue(dose, doseUnit);

                    volume = Math.Round(plan.GetVolumeAtDose(st, doseVal, volPres), 2);

                    TextBlock volatdose = new TextBlock();
                    volatdose.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    volatdose.FontSize = 14;

                    volatdose.Text = String.Format("{0} Volume at {1} = {2} {3}", structCmb.Text, doseVal.ToString(), volume.ToString(), volPresTxt);
                    sp_Stats.Children.Add(volatdose);
                }
            }

            if (cb_DoseVol.IsChecked == true)
            {

                if (tbVolume.Text != null)
                {
                    Double.TryParse(tbVolume.Text, out volume2);

                    DoseValue DV = new DoseValue();
                    DV = plan.GetDoseAtVolume(st, volume2, volPres, dosePres);

                    TextBlock doseatvol = new TextBlock();
                    doseatvol.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    doseatvol.FontSize = 14;
                    doseatvol.Foreground = System.Windows.Media.Brushes.Blue;

                    doseatvol.Text = String.Format("{0} Dose at {1} {2} = {3}", structCmb.Text, volume2.ToString(), volPresTxt, DV.ToString());
                    sp_Stats.Children.Add(doseatvol);
                }
            }
        }

        private void cb_volAbs_Checked(object sender, RoutedEventArgs e)
        {
            lbl_volUnit.Content = cb_volAbs.IsChecked == true ? "cc" : "%";
        }

        private void cb_doseAbs_Checked(object sender, RoutedEventArgs e)
        {
            lbl_doseUnit.Content = cb_doseAbs.IsChecked == true ? "cGy" : "%";
        }

        private void btnComputePTV_Click(object sender, RoutedEventArgs e)
        {
            ComputeTargets();
        }
    }
}
