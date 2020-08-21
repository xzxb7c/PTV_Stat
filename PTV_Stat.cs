using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;


// Do not change namespace and class name
// otherwise Eclipse will not be able to run the script.
namespace VMS.TPS
{
    class Script
    {
        public Script()
        {
        }

        PlanningItem SelectedPlanningItem { get; set; }
        StructureSet SelectedStructureSet { get; set; }
        Structure SelectedStructure { get; set; }

        public void Execute(ScriptContext context, Window window)
        {
            PlanSetup planSetup = context.PlanSetup;
            PlanSum psum = context.PlanSumsInScope.FirstOrDefault();
            bool first = true;

            SelectedPlanningItem = planSetup != null ? (PlanningItem)planSetup : (PlanningItem)psum;
            SelectedStructureSet = planSetup != null ? planSetup.StructureSet : psum.PlanSetups.First().StructureSet;

      

            //    MessageBox.Show(String.Format("3 Planning Iten Sum = {0}", SelectedPlanningItem.Id));

            if (SelectedStructureSet == null)
                throw new ApplicationException("The selected plan does not reference a StructureSet.");

            // For this example we will retrieve first available structure of PTV type


            // Add existing WPF control to the script window.
            var mainControl = new PTV_Stat.MainControl();
            window.Content = mainControl;
            window.Width = 850;
            window.Height = 670;

            window.Title = "Plan : " + SelectedPlanningItem.Id;// + ", Structure : " + target.Id;


            mainControl.tbPatName.Text = context.Patient.Name.Split('(').First();
            mainControl.tbPatID.Text = context.Patient.Id;
            mainControl.tbPlanId.Text = SelectedPlanningItem.Id;
            mainControl.tbDate.Text = DateTime.Today.ToString("MMMM d, yyyy");

            foreach (var structure in SelectedStructureSet.Structures)
            {
                if (!structure.IsEmpty)
                {
                    mainControl.structCmb.Items.Add(structure.Id);
                    if (structure.Id.ToString().ToUpper().StartsWith("PTV") && first)
                    {

                        first = false;
                        mainControl.comboindex = mainControl.structCmb.Items.IndexOf(structure.Id);
                    }
                }
            }

            mainControl.structCmb.SelectedIndex = mainControl.comboindex;

            mainControl.plan = SelectedPlanningItem; //pass planSetup from here to Maincontrol
            mainControl.set = SelectedStructureSet;

            mainControl.isPsum = planSetup != null ? false : true;

            if (!mainControl.isPsum)
            {
                mainControl.tbRxdose.Text = planSetup.TotalDose.ToString().Split(' ').First();
                mainControl.ComputeTargets();
            }
        }


    }


}


/*
 *  <DataGridTemplateColumn Header="Color" Width="50">
                    <DataGridTemplateColumn.HeaderStyle>
                        <Style TargetType="DataGridColumnHeader">
                            <Setter Property="HorizontalContentAlignment"
                                    Value="Center" />
                        </Style>
                    </DataGridTemplateColumn.HeaderStyle>

                    <DataGridTemplateColumn.CellTemplate >
                        <DataTemplate>
                            <Canvas Height="10" Width="30">
                                <Line X1 ="5" X2="25" Y1="5" Y2="5" Stroke="{Binding stLine}" StrokeThickness="4"/>
                            </Canvas>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
*/