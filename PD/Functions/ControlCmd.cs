using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using PD.Models;
using PD.ViewModel;
using PD.AnalysisModel;
using PD.NavigationPages;
using OxyPlot;
//using DiCon.Instrument.HP;
using PD.GPIB;
using DiCon.UCB.Communication;
using DiCon.UCB.Communication.RS232;
using NationalInstruments.NI4882;

namespace PD.Functions
{
    class ControlCmd
    {
        ComViewModel vm;
        Analysis anly;

        public ControlCmd(ComViewModel vm)
        {
            this.vm = vm;
            anly = new Analysis(vm);
        }

        public ControlCmd()
        {

        }


        public void CommandList_Compile()
        {
            #region Compile
            CommandList.Dictionary_Flag.Clear();
            foreach (ComMember cm in vm.ComMembers)
            {
                if (cm.Command == null) continue;
                if (cm.Command.Equals("FLAG"))
                {
                    if (!CommandList.Dictionary_Flag.ContainsKey(cm.Value_1))
                    {
                        CommandList.Dictionary_Flag.Add(cm.Value_1, int.Parse(cm.No));
                    }
                }
            }
            #endregion
        }

        public async void CommandList_Start(int startIndex, bool isCleanChart)
        {
            vm.IsGoOn = false;
            vm.isStop = false;
            vm.timer3_count = 0;

            vm.Save_Log(new LogMember()
            {
                Status = "Command List Start",
                Message = "",
                isShowMSG = true
            });

            if (isCleanChart)
                Clean_Chart();

            if (!string.IsNullOrEmpty(vm.Selected_Comport))
            {
                vm.watch = new System.Diagnostics.Stopwatch();
                vm.watch.Start();

                vm.CMDStartIndex = startIndex;

                while (await CommandListLoop(vm.CMDStartIndex))
                {
                    if (string.IsNullOrEmpty(vm.lastCMD.Value_1)) break;
                    vm.ComMembers.Clear();
                    CSVFunctions.Read_Ref_CSV(vm.lastCMD.Value_1, "page_commandList", vm);

                    for (int i = 0; i < vm.ComMembers.Count; i++)
                    {
                        vm.ComMembers[i].No = i.ToString();
                    }

                    #region Compile
                    CommandList_Compile();
                    #endregion
                }

                var elapsedSec = Math.Round(vm.watch.Elapsed.TotalSeconds, 1);
                string time_sum = string.Format("{0} s", elapsedSec);
                vm.msgModel.msg_3 = time_sum;
                //await Save_Chart();

                vm.Save_Log(new LogMember()
                {
                    Status = "Command List End",
                    Message = vm.Str_cmd_read,
                    TimeSpan = time_sum,
                    isShowMSG = true
                });
            }
            else
                vm.msgModel.msg_2 = "Comport is empty";
        }

        public async Task<bool> CommandListLoop(int startIndex)
        {
            string cmName = "";
            try
            {
                int cmd_count = vm.ComMembers.Count;
                for (int i = startIndex; i < cmd_count; i++)
                {
                    if (vm.isStop) break;

                    if (!vm.ComMembers[i].YN) continue;

                    if (string.IsNullOrEmpty(vm.ComMembers[i].Command)) continue;

                    vm._Page_Command.dataGrid.SelectedIndex = i;

                    ComMember cm = vm.ComMembers[i];
                    cmName = cm.Command;
                    vm.lastCMD = cm;

                    CmdMsgModel cmdMsgModel = await CommandSwitch(cm);
                    if (cm.Command.Equals("LOOPE") || cm.Command.Equals("WHILEND"))
                    {
                        if (cmdMsgModel.isLoop)
                        {
                            i = vm.loop_start_index;
                        }
                    }
                    else if (cm.Command.Equals("CALL"))
                    {
                        return true;
                    }
                    else if (cm.Command.Equals("End"))
                        return true;

                    if (cmdMsgModel.isJump) i = cmdMsgModel.JumpIndex;
                }
            }
            catch
            {
                vm.Save_Log(new LogMember()
                {
                    Status = "Run cmd list",
                    Message = string.Format("Cmd {0} error. Value: {1}", vm.lastCMD.Command, vm.lastCMD.Value_1),
                    isShowMSG = true
                });
            }

            return false;
        }

        async void _timer_Tick(object sender, EventArgs e)
        {
            if (vm.IsGoOn && !vm.isStop)
            {
                #region Run Command
                try
                {
                    if (vm.IsGoOn)
                    {
                        //根據Command List最後一個資料下指令
                        if (vm.ComMembers.Count > 0)
                        {
                            int i = vm.ComMembers.Count - vm.Cmd_Count;
                            if (i >= 0)
                            {
                                ComMember cm = vm.ComMembers[i];
                                string c = cm.Command;

                                //根據指令決定動作
                                if (cm != null)
                                    if (!string.IsNullOrEmpty(cm.Command))
                                    {
                                        await CommandSwitch(cm);
                                        vm.Cmd_Count++;
                                    }
                            }
                            else
                            {
                                if (!vm.PD_or_PM)
                                {
                                    if (vm.station_type == ComViewModel.StationTypes.Fast_Calibration)
                                        await CommandSwitch(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "PD?", Type = "PD" });
                                    else
                                        await CommandSwitch(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "P0?", Type = "PD" });

                                }
                                else
                                    await CommandSwitch(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "GETPOWER", Type = "PM", Channel = vm.switch_index.ToString() });
                            }
                            vm.timer3_count++;
                        }
                        else
                            await Task.Delay(vm.Int_Read_Delay);
                    }
                }
                catch
                {
                    vm.Str_cmd_read = "Run cmd error";
                    return;
                }
                #endregion
            }
            else
            {
                vm._timer.Stop();
            }
        }

        public async Task<bool> CommandListCycle()
        {
            #region Run Command
            while (vm.IsGoOn && !vm.isStop)
            {
                if (vm.IsGoOn && !vm.isStop)
                {
                    try
                    {
                        if (vm.IsGoOn)
                        {
                            //根據Command List最後一個資料下指令
                            if (vm.ComMembers.Count > 0)
                            {
                                int i = vm.ComMembers.Count - vm.Cmd_Count;
                                if (i >= 0)
                                {
                                    ComMember cm = vm.ComMembers[i];
                                    string c = cm.Command;

                                    //根據指令決定動作
                                    if (cm != null)
                                        if (!string.IsNullOrEmpty(cm.Command))
                                        {
                                            await CommandSwitch(cm);
                                            vm.Cmd_Count++;
                                        }
                                        else { vm.ComMembers.RemoveAt(i); }
                                }
                                //No command which is P0?
                                else
                                {
                                    if (!vm.IsDistributedSystem)
                                    {
                                        //PD mode
                                        if (!vm.PD_or_PM)
                                        {
                                            if (vm.station_type == ComViewModel.StationTypes.Fast_Calibration)
                                                await CommandSwitch(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "PD?", Type = "PD" });
                                            else
                                                await CommandSwitch(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "P0?", Type = "PD" });

                                            await Task.Delay(200);
                                        }
                                        //PM mode
                                        else
                                            await CommandSwitch(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "GETPOWER", Type = "PM", Channel = vm.switch_index.ToString() });
                                    }
                                    //distribution system
                                    else
                                    {
                                        for (int channel = 0; channel < vm.ch_count; channel++)
                                        {
                                            await CommandSwitch(new ComMember()
                                            {
                                                YN = true,
                                                No = vm.Cmd_Count.ToString(),
                                                Channel = vm.list_ChannelModels[channel].channel.Replace("ch ", ""),
                                                Comport = vm.list_ChannelModels[channel].PM_Board_Port,
                                                Command = "GETPOWER"
                                            });
                                        }

                                    }
                                }
                                vm.timer3_count++;
                            }
                            else
                                await Task.Delay(vm.Int_Read_Delay);
                        }
                    }
                    catch (Exception ex)
                    {
                        vm.Str_cmd_read = "Run cmd error";
                        MessageBox.Show(ex.ToString());
                        return true;
                    }
                }
                else
                {
                    vm._timer.Stop();
                }
            }
            #endregion

            return true;
        }

        VariableModel varM = new VariableModel();
        public async Task<CmdMsgModel> CommandSwitch(ComMember cm)
        {
            CmdMsgModel cmdMsg = new CmdMsgModel();
            //bool isLoop = true;

            switch (cm.Command.ToUpper())
            {
                case "ID?":
                    if (string.IsNullOrEmpty(cm.Comport))
                        await vm.Port_ReOpen(vm.Selected_Comport);
                    else await vm.Port_ReOpen(cm.Comport);

                    vm.port_PD.Write("ID?" + "\r");
                    await Task.Delay(vm.Int_Read_Delay);

                    string statusMsg = await Cmd_RecieveData_BufferJudge("ID?", false);
                    statusMsg = statusMsg.Equals(string.Empty) ? vm.Str_Status : statusMsg;

                    vm.port_PD.DiscardInBuffer();
                    vm.port_PD.DiscardOutBuffer();
                    vm.port_PD.Close();

                    break;
                case "PD?":
                    if (vm.station_type == ComViewModel.StationTypes.Fast_Calibration)
                    {
                        if (!vm.PD_or_PM)  //PD mode
                        {
                            if (string.IsNullOrEmpty(cm.Comport))
                                await vm.Port_ReOpen(vm.Selected_Comport);
                            else await vm.Port_ReOpen(cm.Comport);

                            vm.port_PD.Write("P0?\r");

                            await Task.Delay(vm.Int_Read_Delay);

                            await Cmd_RecieveData_BufferJudge("PD_Read", false);

                            #region Set Chart data points                                           

                            double sec = (double)Math.Round((decimal)vm.timer2_count * vm.Int_Read_Delay / 1000, 2);

                            if (vm.isTimerOn)
                            {
                                if (sec > vm.int_timer_timespan)
                                {
                                    vm.IsGoOn = false;
                                    vm.isTimerOn = false;
                                }

                                // 更新Timer顯示的"剩餘時間"
                                TimeSpan time = new TimeSpan(0, 0, vm.int_timer_timespan - (int)sec);
                                vm.int_timer_hrs = time.Hours;
                                vm.int_timer_min = time.Minutes;
                                vm.int_timer_sec = time.Seconds;
                            }

                            for (int i = 0; i < vm.Double_Powers.Count; i++)
                            {
                                vm.list_GaugeModels[i].GaugeValue = vm.Double_Powers[i].ToString();  //Update gauge value
                                vm.Save_All_PD_Value[i].Add(new DataPoint(sec, vm.Double_Powers[i]));

                                if (vm.Plot_Series.Count > i)
                                    vm.Plot_Series[i].Points.Add(new DataPoint(sec, vm.Double_Powers[i]));
                            }

                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries

                            vm.PlotViewModel.InvalidatePlot(true);

                            vm.timer2_count++;
                            #endregion

                            #region Cal. Delta IL  
                            //Update_DeltaIL(vm.ChartNowModel.list_dataPoints[0].Count);
                            #endregion
                        }
                    }
                    break;
                case "P0?":
                    if (!vm.PD_or_PM)  //PD mode
                    {
                        //16 channel
                        if (vm.station_type == ComViewModel.StationTypes.Chamber_S_16ch)
                        {
                            await vm.Port_ReOpen(vm.PD_A_ChannelModel.Board_Port);
                            await vm.Port_PD_B_ReOpen(vm.PD_B_ChannelModel.Board_Port);

                            vm.port_PD.Write("P0?\r");
                            vm.port_PD_B.Write("P0?\r");

                            await Task.Delay(vm.Int_Read_Delay);

                            await Cmd_RecieveData_BufferJudge("P0_Read", false);

                            #region Set A Chart data points                                           
                            //if (vm.timer2_count > 30000)  //Default 28800 , two hours
                            //    vm.Save_PD_Value.RemoveAt(0);  //Make sure points count less than 36000

                            double sec = (double)Math.Round((decimal)vm.timer2_count * vm.Int_Read_Delay / 1000, 2);

                            if (vm.isTimerOn)
                            {
                                if (sec > vm.int_timer_timespan)
                                {
                                    vm.IsGoOn = false;
                                    vm.isTimerOn = false;
                                }

                                // 更新Timer顯示的"剩餘時間"
                                TimeSpan time = new TimeSpan(0, 0, vm.int_timer_timespan - (int)sec);
                                vm.int_timer_hrs = time.Hours;
                                vm.int_timer_min = time.Minutes;
                                vm.int_timer_sec = time.Seconds;
                            }

                            for (int i = 0; i < vm.Double_Powers.Count; i++)
                            {
                                vm.list_GaugeModels[i].GaugeValue = vm.Double_Powers[i].ToString();  //Update gauge value
                                                                                                     //vm.list_GaugeModels[i].GaugeEndAngle = anly.Read_PM_to_Gauge(vm.Double_Powers[i], (i + 1));
                                vm.Save_All_PD_Value[i].Add(new DataPoint(sec, vm.Double_Powers[i]));
                            }


                            //vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            //vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries
                            //vm.timer2_count++;
                            #endregion

                            await Cmd_B_RecieveData_BufferJudge("P0_Read", false);

                            #region Set B Chart data points                                           

                            for (int i = 8; i < vm.Double_Powers.Count + 8; i++)
                            {
                                vm.list_GaugeModels[i].GaugeValue = vm.Double_Powers[i - 8].ToString();  //Update gauge value
                                                                                                         //vm.list_GaugeModels[i].GaugeEndAngle = anly.Read_PM_to_Gauge(vm.Double_Powers[i], (i + 1));
                                vm.Save_All_PD_Value[i].Add(new DataPoint(sec, vm.Double_Powers[i - 8]));
                            }

                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries
                            vm.timer2_count++;
                            #endregion

                            #region Cal. Delta IL  
                            Update_DeltaIL(vm.ChartNowModel.list_dataPoints[0].Count);
                            #endregion
                        }

                        //8 channel
                        else
                        {
                            if (string.IsNullOrEmpty(cm.Comport))
                            {
                                if (string.IsNullOrEmpty(vm.Selected_Comport))
                                {
                                    vm.isStop = true;
                                    vm.Save_Log(new LogMember() { isShowMSG = true, Message = "Comport is empty" });
                                    break;
                                }
                                else
                                    await vm.Port_ReOpen(vm.Selected_Comport);
                            }
                            else await vm.Port_ReOpen(cm.Comport);

                            vm.port_PD.Write("P0?\r");

                            await Task.Delay(vm.Int_Read_Delay);

                            string result = await Cmd_RecieveData_BufferJudge("P0_Read", false);

                            #region Set Chart data points                                           
                            if (vm.timer2_count == 0)
                            {
                                vm.maxIL = new List<double>(vm.Double_Powers);
                                vm.minIL = new List<double>(vm.Double_Powers);
                            }

                            //double sec = (double)Math.Round((decimal)vm.timer2_count * vm.Int_Read_Delay / 1000, 2);
                            double sec = vm.watch.Elapsed.TotalSeconds;

                            if (vm.isTimerOn)
                            {
                                if (sec > vm.int_timer_timespan)
                                {
                                    vm.IsGoOn = false;
                                    vm.isTimerOn = false;
                                }

                                // 更新Timer顯示的"剩餘時間"
                                TimeSpan time = new TimeSpan(0, 0, vm.int_timer_timespan - (int)sec);
                                vm.int_timer_hrs = time.Hours;
                                vm.int_timer_min = time.Minutes;
                                vm.int_timer_sec = time.Seconds;
                            }

                            for (int i = 0; i < vm.Double_Powers.Count; i++)
                            {
                                vm.list_GaugeModels[i].GaugeValue = vm.Double_Powers[i].ToString();  //Update gauge value
                                                                                                     //vm.list_GaugeModels[i].GaugeEndAngle = anly.Read_PM_to_Gauge(vm.Double_Powers[i], (i + 1));
                                //vm.Save_All_PD_Value[i].Add(new DataPoint(sec, vm.Double_Powers[i]));

                                if (vm.Plot_Series.Count > i)
                                    vm.Plot_Series[i].Points.Add(new DataPoint(sec, vm.Double_Powers[i]));
                            }

                            //vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            //vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries

                            vm.Update_ALL_PlotView();

                            if (vm.timer2_count == 0)
                                vm.timer2_count++;
                            #endregion

                            #region Cal. Delta IL  
                            //Update_DeltaIL(vm.ChartNowModel.list_dataPoints[0].Count);
                            //Update_DeltaIL();
                            #endregion
                        }
                    }
                    break;
                case "PAUSE":
                    while (vm.lastCMD.YN)
                    {
                        await Task.Delay(300);
                    }
                    vm.lastCMD.YN = true;
                    break;
                case "MSG":
                    if (!string.IsNullOrEmpty(cm.Value_1))
                    {
                        vm.Str_Status = cm.Value_1;
                    }
                    if (!string.IsNullOrEmpty(cm.Value_2))
                    {
                        vm.Str_cmd_read = cm.Value_2;
                    }
                    if (!string.IsNullOrEmpty(cm.Value_3))
                    {
                        vm.msgModel.msg_3 = cm.Value_3;
                    }
                    if (!string.IsNullOrEmpty(cm.Value_4))
                    {
                        int value4 = 0;
                        if (int.TryParse(cm.Value_4, out value4))
                        {
                            if (value4 == 0) break;
                            else if (value4 == 1)
                            {
                                while (vm.lastCMD.YN)
                                {
                                    await Task.Delay(300);
                                }
                                vm.lastCMD.YN = true;
                            }
                        }
                    }
                    break;
                case "MSGOFF":
                    vm.Str_Status = string.Empty;
                    vm.Str_cmd_read = string.Empty;
                    break;
                case "CALL":
                    if (string.IsNullOrEmpty(cm.Value_1))
                    {
                        vm.Save_Log("Get CSV", "Call csv path is empty", false);
                        break;
                    }

                    if (!File.Exists(cm.Value_1)) vm.Save_Log("Get CSV", "CSV is not exist", true);

                    vm.list_scriptModels.Last().ScriptExcuteIndex = int.Parse(cm.No);
                    vm.list_scriptModels.Add(new ScriptModel() { ScriptName = Path.GetFileNameWithoutExtension(cm.Value_1), ScriptPath = cm.Value_1, });
                    vm.CMDStartIndex = 0;
                    break;

                case "END":
                    int indexScript = vm.list_scriptModels.Count - 2;  //Back to the upper level script

                    if (indexScript >= 0)
                    {
                        string path = vm.list_scriptModels[indexScript].ScriptPath;
                        if (!File.Exists(path)) vm.Save_Log("Get CSV", "CSV is not exist", true);
                        else
                        {
                            vm.list_scriptModels.RemoveAt(vm.list_scriptModels.Count - 1);
                            vm.CMDStartIndex = vm.list_scriptModels[indexScript].ScriptExcuteIndex + 1;
                            vm.lastCMD.Value_1 = path;
                        }
                    }
                    else
                    {
                        vm.isStop = true;
                        vm.Save_Log("", "Command List End", true);
                    }
                    break;

                case "LOOP":
                    cmdMsg.isLoop = true;
                    vm.loop_start_index = int.Parse(cm.No);
                    if (!string.IsNullOrEmpty(cm.Value_1))
                    {
                        int value = 0;
                        if (int.TryParse(cm.Value_1, out value))
                            vm.loop_total_cycle = value;  //計錄總圈數
                        vm.loop_cycle_count = 0;
                    }
                    else
                    {
                        vm.Save_Log("Run cmd list.", "Cmd 'Loop' error", false);
                        break;
                    }
                    break;
                case "LOOPE":
                    vm.loop_cycle_count++;     //Loop cycle count +1

                    vm.loop_end_index = int.Parse(cm.No);   //Save loop end position

                    if (vm.loop_cycle_count < vm.loop_total_cycle)       //若cycle number 小於目標圈數
                    {
                        //loopCount = vm.loop_start_index;   //Set cmd no. to loop start position
                    }
                    else   //cycle已結束, reset loop counting value
                    {
                        vm.loop_total_cycle = 0;
                        vm.loop_cycle_count = 0;
                        //loopCount = vm.loop_end_index;
                        cmdMsg.isLoop = false;
                    }
                    break;

                case "WHILE":
                    cmdMsg.isLoop = true;

                    //Get start / end index
                    vm.loop_start_index = int.Parse(cm.No);
                    for (int i = vm.loop_start_index + 1; i < vm.ComMembers.Count; i++)
                    {
                        if (vm.ComMembers[i].Command.Equals("WHILEND"))
                        {
                            vm.loop_end_index = i;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(cm.Value_1))
                    {
                        if (cm.Value_1.ToCharArray()[0].Equals('@'))
                        {
                            varM = vm.list_VarBoolModels[int.Parse(cm.Value_1.Remove(0, 1))];
                        }
                        else
                        {
                            varM = vm.list_VarBoolModels[int.Parse(cm.Value_1)];
                        }
                    }
                    else
                    {
                        vm.Save_Log("Run cmd list.", "Cmd 'Loop' error", false);
                        return cmdMsg;
                    }
                    break;

                case "WHILEND":
                    if (!varM.VariableBool)
                        cmdMsg.isLoop = false;
                    break;

                case "WRITE":
                    if (!string.IsNullOrEmpty(cm.Comport))
                        await vm.Port_ReOpen(cm.Comport);
                    else await vm.Port_ReOpen(vm.Selected_Comport);

                    vm.port_PD.Write(cm.Value_1 + "\r");
                    await Task.Delay(125);
                    await Write_Cmd(cm.Value_1, true);
                    break;

                case "WRITEDAC":
                    if (string.IsNullOrEmpty(cm.Channel)) cm.Channel = "1";
                    if (!string.IsNullOrEmpty(cm.Comport)) vm.Selected_Comport = cm.Comport;

                    if (!vm.port_PD.IsOpen)
                        await vm.Port_ReOpen(vm.Selected_Comport);

                    string[] dac123 = new string[] { cm.Value_1, cm.Value_2, cm.Value_3 };

                    if (!string.IsNullOrEmpty(cm.Value_1) || !string.IsNullOrEmpty(cm.Value_2))
                    {
                        if (cm.Value_1 != null)
                            if (cm.Value_1.First().Equals('*'))
                                dac123[0] = vm.list_VariableModels[int.Parse(cm.Value_1.Remove(0, 1))].VariableContent.ToString();

                        if (cm.Value_2 != null)
                            if (cm.Value_2.First().Equals('*'))
                                dac123[1] = vm.list_VariableModels[int.Parse(cm.Value_2.Remove(0, 1))].VariableContent.ToString();

                        if (cm.Value_3 != null)
                            if (cm.Value_3.First().Equals('*'))
                                dac123[2] = vm.list_VariableModels[int.Parse(cm.Value_3.Remove(0, 1))].VariableContent.ToString();
                    }
                    else if (cm.Value_3 != null) //PD , select V3
                    {
                        int dac = int.Parse(cm.Value_3);
                        if (dac >= -65535 && dac <= 65535)
                        {
                            string cd = string.Format("VOA{0} {1}", cm.Channel, dac); //Write Dac
                            vm.port_PD.Write(cd + "\r");
                            await Task.Delay(vm.Int_Write_Delay);
                            vm.Str_Status = "Set Dac"; vm.Str_cmd_read = cd;
                            break;
                        }
                    }

                    bool isDacFitRange = true;
                    //If Dac if out of range
                    foreach (string s in dac123)
                    {
                        if (string.IsNullOrEmpty(s)) continue;
                        int dac = int.Parse(s);
                        if (dac < -65535 || dac > 65535)
                        {
                            isDacFitRange = false;
                            vm.Save_Log(new LogMember()
                            {
                                Status = "Set Dac",
                                Message = "Dac out of range",
                                isShowMSG = false
                            });
                        }
                    }

                    if (isDacFitRange)
                    {
                        await Task.Delay(20);
                        vm.Cmd_WriteDac(cm.Channel, dac123[0], dac123[1], dac123[2]);  //Write Dac

                        if (vm.PD_or_PM)
                            await Get_Dac("D1?");  //Ask dac and show in the gauge
                    }

                    break;
                case "DAC?":
                    if (string.IsNullOrEmpty(cm.Comport)) await vm.Port_ReOpen(vm.Selected_Comport);
                    else
                    {
                        if (!vm.port_PD.IsOpen || vm.port_PD.PortName != cm.Comport)
                            await vm.Port_ReOpen(cm.Comport);
                    }

                    string cmd = "";
                    try
                    {
                        if (cm.Type.Equals("PD"))  //PD mode
                        {
                            if (!cm.Channel.Equals("0"))
                            {
                                cmd = "D" + cm.Channel + "?";
                                await Task.Delay(20);
                                await Get_Dac(cmd);
                            }
                        }
                        else
                            await Get_Dac("D1?");

                        vm.Str_Status = "Get Dac";
                    }
                    catch
                    {
                        vm.Str_cmd_read = "Command Channel is null";
                    }

                    break;
                case "TLSACT":
                    bool isAct = false;
                    bool.TryParse(cm.Value_1, out isAct);
                    Set_TLS_Active(isAct);
                    break;
                case "SETPOWER":
                    try
                    {
                        double var_TLSPower = 0;
                        if (!double.TryParse(cm.Value_1, out var_TLSPower)) break;

                        Set_TLS_Power(var_TLSPower, false);
                        vm.Double_Laser_Power = Convert.ToDouble(var_TLSPower);
                    }
                    catch
                    {
                        vm.Save_Log("Set TLS Power", "Set power error", true);
                    }
                    break;
                case "GETPOWER":

                    if (!vm.IsDistributedSystem)
                    {
                        //PD mode
                        if (!vm.PD_or_PM)
                        {
                            string comport = "";
                            if (string.IsNullOrEmpty(cm.Comport)) comport = vm.Selected_Comport;
                            else comport = cm.Comport;

                            int ch = 1;
                            if (!int.TryParse(cm.Channel, out ch)) ch = 0;
                            await Get_PD_Value(comport);  //Y axis value

                            if (ch == 0)
                            {
                                for (int i = 0; i < vm.ch_count; i++)
                                {
                                    vm.list_GaugeModels[i].GaugeValue = Math.Round(vm.Double_Powers[i], vm.decimal_place).ToString();
                                }
                            }
                            else
                            {
                                double p = Math.Round(vm.Double_Powers[ch - 1], vm.decimal_place);
                                vm.list_GaugeModels[ch - 1].GaugeValue = p.ToString();
                            }


                            double var_GETPOWER;
                            //X axis judgement
                            if (string.IsNullOrEmpty(cm.Value_1))
                            {
                                var elapsedSec = Math.Round(vm.watch.Elapsed.TotalSeconds, 1);
                                var_GETPOWER = elapsedSec;
                            }
                            else
                            {
                                if (cm.Value_1.First().Equals('*'))
                                {
                                    int index = int.Parse(cm.Value_1.Remove(0, 1));
                                    var_GETPOWER = vm.list_VariableModels[index].VariableContent;
                                }
                                else
                                {
                                    var_GETPOWER = vm.list_VariableModels[int.Parse(cm.Value_1)].VariableContent;
                                }
                            }

                            #region Update Chart data points   
                            if (ch == 0)
                            {
                                for (int i = 0; i < vm.ch_count; i++)
                                {
                                    Update_Chart(var_GETPOWER, vm.Double_Powers[i], i);
                                }
                            }
                            else
                            {
                                vm.kModel.dataPoints.Add(new DataPoint(var_GETPOWER, vm.Double_Powers[ch - 1]));  //offer max function to calculation 

                                //If value2 is not null, set the power to a var
                                if (!string.IsNullOrEmpty(cm.Value_2))
                                {
                                    string s = cm.Value_2;
                                    if (cm.Value_2.Contains("*")) s = s.Remove(0, 1);
                                    vm.list_VariableModels[int.Parse(s)].VariableContent = vm.Double_Powers[ch - 1];
                                }

                                Update_Chart(var_GETPOWER, vm.Double_Powers[ch - 1], 0);
                            }
                            Update_DeltaIL(vm.timer3_count);

                            #endregion
                        }

                        //PM mode
                        else
                        {
                            int ch = 1;
                            int.TryParse(cm.Channel, out ch);
                            double p = await Get_PM_Value((ch - 1));  //Y axis value

                            //vm.list_GaugeModels[ch - 1].GaugeValue = p.ToString();

                            double var_GETPOWER;
                            //X axis judgement
                            if (string.IsNullOrEmpty(cm.Value_1))
                            {
                                var elapsedSec = Math.Round(vm.watch.Elapsed.TotalSeconds, 1);
                                var_GETPOWER = elapsedSec;
                            }
                            else
                            {
                                if (cm.Value_1.ToCharArray()[0].Equals('*'))
                                {
                                    int index = int.Parse(cm.Value_1.Remove(0, 1));
                                    var_GETPOWER = vm.list_VariableModels[index].VariableContent;
                                }
                                else
                                {
                                    var_GETPOWER = vm.list_VariableModels[int.Parse(cm.Value_1)].VariableContent;
                                }
                            }

                            //vm.kModel.dataPoints.Add(new DataPoint(var_GETPOWER, p));  //offer max function to calculation
                            vm.ChartNowModel.list_dataPoints[ch - 1].Add(new DataPoint(var_GETPOWER, p));

                            foreach (GaugeModel GM in vm.list_GaugeModels)
                            {
                                if (GM.GaugeChannel.Equals(cm.Channel))
                                    vm.list_GaugeModels[ch - 1].GaugeValue = Math.Round(p, vm.decimal_place).ToString();
                            }

                            //If value2 is not null, set the power to a var
                            if (!string.IsNullOrEmpty(cm.Value_2))
                            {
                                string s = cm.Value_2;
                                if (cm.Value_2.Contains("*")) s = s.Remove(0, 1);
                                vm.list_VariableModels[int.Parse(s)].VariableContent = p;
                            }

                            #region Update Chart data points   
                            if (vm.is_update_chart) Update_Chart(var_GETPOWER, p, vm.switch_index - 1);
                            //Update_Chart(var_GETPOWER, p, vm.switch_index-1);
                            Update_DeltaIL(vm.ChartNowModel.list_dataPoints[0].Count);
                            #endregion
                        }
                    }
                    //Distribution System
                    else
                    {
                        if (string.IsNullOrEmpty(cm.Channel)) break;

                        int ch = 1;
                        if (!int.TryParse(cm.Channel, out ch)) ch = 1;

                        string rs232_cmd = vm.list_ChannelModels[ch - 1].PM_GetPower_CMD;

                        if (vm.list_ChannelModels[ch - 1].PM_Type == "GPIB")
                        {

                        }
                        else if (vm.list_ChannelModels[ch - 1].PM_Type == "RS232")
                        {
                            string comport = "";
                            if (string.IsNullOrEmpty(cm.Comport)) comport = vm.Selected_Comport;
                            else comport = cm.Comport;

                            await Get_IL_rs232(ch, comport, rs232_cmd);  //Y axis value

                            if (ch == 0)
                            {
                                for (int i = 0; i < vm.ch_count; i++)
                                {
                                    vm.list_GaugeModels[i].GaugeValue = Math.Round(vm.Double_Powers[i], vm.decimal_place).ToString();
                                }
                            }
                            else
                            {
                                double p = vm.Double_Powers[ch - 1];
                                vm.list_GaugeModels[ch - 1].GaugeValue = Math.Round(p, vm.decimal_place).ToString();
                            }
                            
                            double var_GETPOWER;
                            //X axis judgement
                            if (string.IsNullOrEmpty(cm.Value_1))
                            {
                                var elapsedSec = Math.Round(vm.watch.Elapsed.TotalSeconds, 1);
                                var_GETPOWER = elapsedSec;
                            }
                            else
                            {
                                if (cm.Value_1.First().Equals('*'))
                                {
                                    int index = int.Parse(cm.Value_1.Remove(0, 1));
                                    var_GETPOWER = vm.list_VariableModels[index].VariableContent;
                                }
                                else
                                {
                                    var_GETPOWER = vm.list_VariableModels[int.Parse(cm.Value_1)].VariableContent;
                                }
                            }

                            vm.ChartNowModel.list_dataPoints[ch - 1].Add(new DataPoint(var_GETPOWER, vm.Double_Powers[ch - 1]));

                            #region Update Chart data points   
                            if (ch == 0)
                            {
                                for (int i = 0; i < vm.ch_count; i++)
                                {
                                    Update_Chart(var_GETPOWER, vm.Double_Powers[i], i);
                                }
                            }
                            else
                            {
                                vm.kModel.dataPoints.Add(new DataPoint(var_GETPOWER, vm.Double_Powers[ch - 1]));  //offer max function to calculation 

                                //If value2 is not null, set the power to a var
                                if (!string.IsNullOrEmpty(cm.Value_2))
                                {
                                    string s = cm.Value_2;
                                    if (cm.Value_2.Contains("*")) s = s.Remove(0, 1);
                                    vm.list_VariableModels[int.Parse(s)].VariableContent = vm.Double_Powers[ch - 1];
                                }

                                Update_Chart(var_GETPOWER, vm.Double_Powers[ch - 1], 0);
                            }
                            Update_DeltaIL(vm.timer3_count);

                            #endregion
                        }
                    }
                    break;

                case "SETSWITCH":
                    #region switch re-open
                    try
                    {
                        await vm.Port_Switch_ReOpen();
                    }
                    catch (Exception ex)
                    {
                        vm.Str_cmd_read = "Switch Port re-open Error";
                        vm.Save_Log(vm.Str_Status, cm.Channel, vm.Str_cmd_read);
                        vm.isStop = true;
                        MessageBox.Show(ex.StackTrace.ToString());
                        return cmdMsg;
                    }
                    #endregion

                    await Task.Delay(100);

                    #region Switch write Cmd
                    vm.list_GaugeModels[vm.switch_index - 1].GaugeValue = "";

                    int switch_index;
                    if (!int.TryParse(cm.Channel, out switch_index)) return cmdMsg;

                    if (switch_index > 0)   //Switch 1~12
                    {
                        try
                        {
                            if (vm.port_Switch.IsOpen)
                            {
                                //vm.Str_Command = "SW0 " + switch_index.ToString();

                                vm.Str_Command = $"SW1 {switch_index}";
                                vm.port_Switch.Write(vm.Str_Command + "\r");
                                await Task.Delay(vm.Int_Write_Delay);

                                vm.Str_Command = $"SW2 {switch_index}";
                                vm.port_Switch.Write(vm.Str_Command + "\r");
                                await Task.Delay(vm.Int_Write_Delay);

                                vm.switch_index = switch_index;
                                vm.ch = switch_index - 1;   //Save Switch channel

                                vm.port_Switch.DiscardInBuffer();       // RX
                                vm.port_Switch.DiscardOutBuffer();      // TX

                                vm.port_Switch.Close();

                                vm.switch_index = switch_index;

                            }

                        }
                        catch (Exception ex) { vm.Str_cmd_read = "Set Switch Error"; vm.Save_Log(vm.Str_Status, switch_index.ToString(), vm.Str_cmd_read); MessageBox.Show(ex.StackTrace.ToString()); }
                    }
                    #endregion

                    break;

                case "CLRDP":
                    vm.kModel.dataPoints.Clear();
                    break;

                case "SETWL":
                    try
                    {
                        double var_wl = 1550;

                        if (cm.Value_1.ToArray()[0].Equals('*'))
                        {
                            var_wl = vm.list_VariableModels[int.Parse(cm.Value_1.Remove(0, 1))].VariableContent;

                            vm.tls.SetWL(var_wl);
                            vm.Double_Laser_Wavelength = var_wl;
                            break;
                        }

                        if (!double.TryParse(cm.Value_1, out var_wl)) break;

                        double wl = var_wl;
                        await Set_WL(wl, false);

                        vm.Double_Laser_Wavelength = wl;
                    }
                    catch { }
                    break;

                case "DELAY":
                    await Task.Delay(int.Parse(cm.Value_1));
                    cmdMsg.isJump = true;
                    cmdMsg.JumpIndex = int.Parse(cm.No);
                    break;

                case "MESSAGEBOX":
                    Window_MessageBox window_MessageBox = new Window_MessageBox(vm, cm.Value_1);
                    window_MessageBox.ShowDialog();
                    break;

                case "MAXPOWER":
                    //Value_1
                    List<DataPoint> dataPoints = vm.kModel.dataPoints.Where(x => x.Y == vm.kModel.dataPoints.Max(y => y.Y)).ToList();
                    if (dataPoints.Count > 0)
                    {
                        int var;
                        if (!int.TryParse(cm.Value_1, out var)) break;
                        vm.list_VariableModels[var].VariableContent = dataPoints[0].Y;
                    }
                    break;

                case "MAXPOWDAC":
                    //Value_1
                    dataPoints = vm.kModel.dataPoints.Where(x => x.Y == vm.kModel.dataPoints.Max(y => y.Y)).ToList();
                    if (dataPoints.Count > 0)
                    {
                        int var;
                        if (!int.TryParse(cm.Value_1, out var)) break;
                        vm.list_VariableModels[var].VariableContent = dataPoints[0].X;
                    }
                    break;

                case "STRPATH":
                    if (File.Exists(@cm.Value_1))
                    {
                        vm.pageName_LogCmd = "page_stringList";
                        CSVFunctions.Read_Ref_CSV(@cm.Value_1, vm.pageName_LogCmd, vm);
                    }
                    break;

                case "SAVEPOWER":
                    if (vm.PD_or_PM)  //PM mode
                    {
                        int ch = 1;
                        int.TryParse(cm.Channel, out ch);
                        double p = await Get_PM_Value((ch - 1));  //Y axis value                       

                        if (!string.IsNullOrEmpty(cm.Value_1))
                        {
                            string s = cm.Value_1;
                            if (cm.Value_1.Contains("*")) s = s.Remove(0, 1);
                            vm.list_VariableModels[int.Parse(s)].VariableContent = p;
                        }
                    }
                    break;

                case "SAVECHART":
                    await Save_Chart();
                    break;

                case "SETVAR":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2))
                    {
                        string s = cm.Value_1;
                        if (s.Contains("*")) s = s.Remove(0, 1);

                        string s2 = cm.Value_2;
                        if (!s2.Contains("*"))
                            vm.list_VariableModels[int.Parse(s)].VariableContent = double.Parse(s2);
                        else
                            vm.list_VariableModels[int.Parse(s)].VariableContent = vm.list_VariableModels[int.Parse(s2.Remove(0, 1))].VariableContent;
                    }
                    break;

                case "SETBOOL":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2))
                    {
                        string s = cm.Value_1;
                        if (s.Contains("@")) s = s.Remove(0, 1);

                        string s2 = cm.Value_2;
                        if (!s2.Contains("@"))
                            vm.list_VarBoolModels[int.Parse(s)].VariableBool = bool.Parse(s2);
                        else
                            vm.list_VariableModels[int.Parse(s)].VariableBool = vm.list_VariableModels[int.Parse(s2.Remove(0, 1))].VariableBool;
                    }
                    break;

                case "ADD":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2))
                    {
                        string s = cm.Value_1;
                        if (s.First().Equals('*')) s = s.Remove(0, 1);

                        double d = 0;
                        if (cm.Value_2.First().Equals('*'))
                        {
                            vm.list_VariableModels[int.Parse(s)].VariableContent += vm.list_VariableModels[int.Parse(cm.Value_2.Remove(0, 1))].VariableContent;
                        }
                        else
                        {
                            d = double.Parse(cm.Value_2);
                            vm.list_VariableModels[int.Parse(s)].VariableContent += d;
                        }
                    }
                    break;

                case "SUB":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2))
                    {
                        string s = cm.Value_1;
                        if (s.Contains("*")) s = s.Remove(0, 1);

                        double d = 0;
                        if (cm.Value_2.Contains("*"))
                        {
                            vm.list_VariableModels[int.Parse(s)].VariableContent -= vm.list_VariableModels[int.Parse(cm.Value_2.Remove(0, 1))].VariableContent;
                        }
                        else
                        {
                            d = double.Parse(cm.Value_2);
                            vm.list_VariableModels[int.Parse(s)].VariableContent -= d;
                        }
                    }
                    break;

                case "MULT":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2))
                    {
                        string s = cm.Value_1;
                        if (s.Contains("*")) s = s.Remove(0, 1);

                        double d = 0;
                        if (cm.Value_2.Contains("*"))
                        {
                            vm.list_VariableModels[int.Parse(s)].VariableContent *= vm.list_VariableModels[int.Parse(cm.Value_2.Remove(0, 1))].VariableContent;
                        }
                        else
                        {
                            d = double.Parse(cm.Value_2);
                            vm.list_VariableModels[int.Parse(s)].VariableContent *= d;
                        }
                    }
                    break;

                case "DIV":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2))
                    {
                        string s = cm.Value_1;
                        if (s.Contains("*")) s = s.Remove(0, 1);

                        double d = 0;
                        if (cm.Value_2.Contains("*"))
                        {
                            vm.list_VariableModels[int.Parse(s)].VariableContent /= vm.list_VariableModels[int.Parse(cm.Value_2.Remove(0, 1))].VariableContent;
                        }
                        else
                        {
                            d = double.Parse(cm.Value_2);
                            vm.list_VariableModels[int.Parse(s)].VariableContent /= d;
                        }
                    }
                    break;

                case "CMPGT":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2) && !string.IsNullOrEmpty(cm.Value_3))
                    {
                        string s1 = cm.Value_1;
                        if (s1.Contains("*")) s1 = s1.Remove(0, 1);
                        int var_CMPGT_1;
                        if (!int.TryParse(s1, out var_CMPGT_1)) break;

                        string s2 = cm.Value_2;
                        if (s2.Contains("*")) s2 = s2.Remove(0, 1);
                        int var_CMPGT_2;
                        if (!int.TryParse(s2, out var_CMPGT_2)) break;

                        string s3 = cm.Value_3;
                        if (s3.Contains("*")) s3 = s3.Remove(0, 1);
                        int var_CMPGT_3;
                        if (!int.TryParse(s3, out var_CMPGT_3)) break;

                        if (vm.list_VariableModels[var_CMPGT_1].VariableContent >= vm.list_VariableModels[var_CMPGT_2].VariableContent)
                        {
                            vm.list_VarBoolModels[var_CMPGT_3].VariableBool = true;
                        }
                        else { vm.list_VarBoolModels[var_CMPGT_3].VariableBool = false; }

                        if (vm.list_VarBoolModels[var_CMPGT_3].VariableBool)
                            if (!string.IsNullOrEmpty(cm.Value_4))
                            {
                                //Flag
                                if (cm.Value_4.Contains('f') || cm.Value_4.Contains('F'))
                                {
                                    cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_4.Remove(0, 1)];
                                    cmdMsg.isJump = true;
                                }
                                else
                                {
                                    cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_4];
                                    cmdMsg.isJump = true;
                                }
                            }
                    }

                    break;

                case "CMPGE":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2) && !string.IsNullOrEmpty(cm.Value_3))
                    {
                        string s1 = cm.Value_1;
                        if (s1.Contains("*")) s1 = s1.Remove(0, 1);
                        int var_CMPGE_1;
                        if (!int.TryParse(s1, out var_CMPGE_1)) break;

                        string s2 = cm.Value_2;
                        if (s2.Contains("*")) s2 = s2.Remove(0, 1);
                        int var_CMPGE_2;
                        if (!int.TryParse(s2, out var_CMPGE_2)) break;

                        string s3 = cm.Value_3;
                        if (s3.Contains("*")) s3 = s3.Remove(0, 1);
                        int var_CMPGE_3;
                        if (!int.TryParse(s3, out var_CMPGE_3)) break;

                        if (vm.list_VariableModels[var_CMPGE_1].VariableContent >= vm.list_VariableModels[var_CMPGE_2].VariableContent)
                        {
                            vm.list_VarBoolModels[var_CMPGE_3].VariableBool = true;
                        }
                        else { vm.list_VarBoolModels[var_CMPGE_3].VariableBool = false; }

                        if (vm.list_VarBoolModels[var_CMPGE_3].VariableBool)
                            if (!string.IsNullOrEmpty(cm.Value_4))
                            {
                                //Flag
                                if (cm.Value_4.Contains('f') || cm.Value_4.Contains('F'))
                                {
                                    cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_4.Remove(0, 1)];
                                    cmdMsg.isJump = true;
                                }
                                else
                                {
                                    cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_4];
                                    cmdMsg.isJump = true;
                                }
                            }
                    }

                    break;

                case "CMPLE":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2) && !string.IsNullOrEmpty(cm.Value_3))
                    {
                        string s1 = cm.Value_1;
                        if (s1.Contains("*")) s1 = s1.Remove(0, 1);
                        int var_CMPLE_1;
                        if (!int.TryParse(s1, out var_CMPLE_1)) break;

                        string s2 = cm.Value_2;
                        if (s2.Contains("*")) s2 = s2.Remove(0, 1);
                        int var_CMPLE_2;
                        if (!int.TryParse(s2, out var_CMPLE_2)) break;

                        string s3 = cm.Value_3;
                        if (s3.Contains("*")) s3 = s3.Remove(0, 1);
                        int var_CMPLE_3;
                        if (!int.TryParse(s3, out var_CMPLE_3)) break;

                        if (vm.list_VariableModels[var_CMPLE_1].VariableContent <= vm.list_VariableModels[var_CMPLE_2].VariableContent)
                        {
                            vm.list_VarBoolModels[var_CMPLE_3].VariableBool = true;
                        }
                        else { vm.list_VarBoolModels[var_CMPLE_3].VariableBool = false; }

                        if (vm.list_VarBoolModels[var_CMPLE_3].VariableBool)
                            if (!string.IsNullOrEmpty(cm.Value_4))
                            {
                                //Flag
                                if (cm.Value_4.Contains('f') || cm.Value_4.Contains('F'))
                                {
                                    cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_4.Remove(0, 1)];
                                    cmdMsg.isJump = true;
                                }
                                else
                                {
                                    cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_4];
                                    cmdMsg.isJump = true;
                                }
                            }
                    }
                    break;

                case "CMPLT":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2) && !string.IsNullOrEmpty(cm.Value_3))
                    {
                        string s1 = cm.Value_1;
                        if (s1.Contains("*")) s1 = s1.Remove(0, 1);
                        int var_CMPLT_1;
                        if (!int.TryParse(s1, out var_CMPLT_1)) break;

                        string s2 = cm.Value_2;
                        if (s2.Contains("*")) s2 = s2.Remove(0, 1);
                        int var_CMPLT_2;
                        if (!int.TryParse(s2, out var_CMPLT_2)) break;

                        string s3 = cm.Value_3;
                        if (s3.Contains("*")) s3 = s3.Remove(0, 1);
                        int var_CMPLT_3;
                        if (!int.TryParse(s3, out var_CMPLT_3)) break;

                        if (vm.list_VariableModels[var_CMPLT_1].VariableContent < vm.list_VariableModels[var_CMPLT_2].VariableContent)
                        {
                            vm.list_VarBoolModels[var_CMPLT_3].VariableBool = true;
                        }
                        else { vm.list_VarBoolModels[var_CMPLT_3].VariableBool = false; }

                        if (vm.list_VarBoolModels[var_CMPLT_3].VariableBool)
                            if (!string.IsNullOrEmpty(cm.Value_4))
                            {
                                //Flag
                                if (cm.Value_4.Contains('f') || cm.Value_4.Contains('F'))
                                {
                                    cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_4.Remove(0, 1)];
                                    cmdMsg.isJump = true;
                                }
                                else
                                {
                                    cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_4];
                                    cmdMsg.isJump = true;
                                }
                            }
                    }
                    break;

                case "IFON":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2))
                    {
                        string s = cm.Value_1;
                        if (cm.Value_1.Contains("@")) s = s.Remove(0, 1);

                        if (vm.list_VarBoolModels[int.Parse(s)].VariableBool)
                        {
                            //Flag
                            if (cm.Value_2.Contains('f') || cm.Value_2.Contains('F'))
                            {
                                cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_2.Remove(0, 1)];
                                cmdMsg.isJump = true;
                            }
                            else
                            {
                                //cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_2];
                                cmdMsg.JumpIndex = int.Parse(cm.Value_2);
                                cmdMsg.isJump = true;
                            }
                        }
                    }
                    break;

                case "IFOFF":
                    if (!string.IsNullOrEmpty(cm.Value_1) && !string.IsNullOrEmpty(cm.Value_2))
                    {
                        string s = cm.Value_1;
                        if (cm.Value_1.Contains("@")) s = s.Remove(0, 1);

                        if (!vm.list_VarBoolModels[int.Parse(s)].VariableBool)
                        {
                            //Flag
                            if (cm.Value_2.Contains('f') || cm.Value_2.Contains('F'))
                            {
                                cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_2.Remove(0, 1)];
                                cmdMsg.isJump = true;
                            }
                            else
                            {
                                //cmdMsg.JumpIndex = CommandList.Dictionary_Flag[cm.Value_2];
                                cmdMsg.JumpIndex = int.Parse(cm.Value_2);
                                cmdMsg.isJump = true;
                            }
                        }
                    }
                    break;

                case "UVSETPOW":
                    try
                    {
                        int baudrate_before = vm.BoudRate;
                        string com_before = "";
                        if (!string.IsNullOrEmpty(cm.Comport))
                        {
                            com_before = vm.Selected_Comport;
                            vm.Selected_Comport = cm.Comport;
                        }
                        if (vm.port_PD.PortName != vm.Selected_Comport || !vm.port_PD.IsOpen)
                        {
                            vm.BoudRate = 9600;
                            await vm.Port_ReOpen(vm.Selected_Comport);
                        }

                        string str_cmd = string.Empty;
                        if (cm.Channel == "1")
                            str_cmd = "L1 " + cm.Value_1 + "F";
                        else if (cm.Channel == "2")
                            str_cmd = "L3 " + cm.Value_1 + "F";

                        vm.port_PD.Write(str_cmd);
                        await Task.Delay(250);

                        if (vm.port_PD.IsOpen)
                        {
                            vm.port_PD.DiscardInBuffer();
                            vm.port_PD.DiscardOutBuffer();
                            vm.port_PD.Close();
                        }

                        vm.BoudRate = baudrate_before;
                        vm.Selected_Comport = com_before;
                    }
                    catch { }
                    break;

                case "UVSETTIMER":
                    try
                    {
                        int baudrate_before = vm.BoudRate;
                        string com_before = "";
                        if (!string.IsNullOrEmpty(cm.Comport))
                        {
                            com_before = vm.Selected_Comport;
                            vm.Selected_Comport = cm.Comport;
                        }
                        if (vm.port_PD.PortName != vm.Selected_Comport || !vm.port_PD.IsOpen)
                        {
                            vm.BoudRate = 9600;
                            await vm.Port_ReOpen(vm.Selected_Comport);
                        }

                        string str_cmd = string.Empty;
                        if (cm.Channel == "1")
                            str_cmd = "L2 " + cm.Value_1 + "F";
                        else if (cm.Channel == "2")
                            str_cmd = "L4 " + cm.Value_1 + "F";

                        vm.port_PD.Write(str_cmd);
                        await Task.Delay(250);

                        if (vm.port_PD.IsOpen)
                        {
                            vm.port_PD.DiscardInBuffer();
                            vm.port_PD.DiscardOutBuffer();
                            vm.port_PD.Close();
                        }

                        vm.BoudRate = baudrate_before;
                        vm.Selected_Comport = com_before;
                    }
                    catch { }
                    break;

                case "UVSTART":
                    try
                    {
                        int baudrate_before = vm.BoudRate;
                        string com_before = "";
                        if (!string.IsNullOrEmpty(cm.Comport))
                        {
                            com_before = vm.Selected_Comport;
                            vm.Selected_Comport = cm.Comport;
                        }
                        if (vm.port_PD.PortName != vm.Selected_Comport || !vm.port_PD.IsOpen)
                        {
                            vm.BoudRate = 9600;
                            await vm.Port_ReOpen(vm.Selected_Comport);
                        }

                        string str_cmd = "LE 100F";
                        vm.port_PD.Write(str_cmd);
                        await Task.Delay(250);

                        if (vm.port_PD.IsOpen)
                        {
                            vm.port_PD.DiscardInBuffer();
                            vm.port_PD.DiscardOutBuffer();
                            vm.port_PD.Close();
                        }

                        vm.BoudRate = baudrate_before;
                        vm.Selected_Comport = com_before;
                    }
                    catch { }
                    break;

                case "UVSTOP":
                    try
                    {
                        int baudrate_before = vm.BoudRate;
                        string com_before = "";
                        if (!string.IsNullOrEmpty(cm.Comport))
                        {
                            com_before = vm.Selected_Comport;
                            vm.Selected_Comport = cm.Comport;
                        }
                        if (vm.port_PD.PortName != vm.Selected_Comport || !vm.port_PD.IsOpen)
                        {
                            vm.BoudRate = 9600;
                            await vm.Port_ReOpen(vm.Selected_Comport);
                        }

                        string str_cmd = "LE 50F";
                        vm.port_PD.Write(str_cmd);
                        await Task.Delay(250);

                        if (vm.port_PD.IsOpen)
                        {
                            vm.port_PD.DiscardInBuffer();
                            vm.port_PD.DiscardOutBuffer();
                            vm.port_PD.Close();
                        }

                        vm.BoudRate = baudrate_before;
                        vm.Selected_Comport = com_before;
                    }
                    catch { }
                    break;

                case "FLAG":
                    if (!CommandList.Dictionary_Flag.ContainsKey(cm.Value_1))
                    {
                        CommandList.Dictionary_Flag.Add(cm.Value_1, int.Parse(cm.No));
                    }
                    break;

                case "JUMP":
                    if (cm.Value_1.Contains("F") || cm.Value_1.Contains("f"))
                    {
                        string s = cm.Value_1.Remove(0, 1);
                        cmdMsg.JumpIndex = CommandList.Dictionary_Flag[s];
                    }
                    else
                    {
                        cmdMsg.JumpIndex = int.Parse(cm.Value_1) - 1;
                    }
                    cmdMsg.isJump = true;
                    break;

                case "BSET":
                    vm.list_VarBoolModels[anly.JudgeVariable(cm.Value_1, 1).VariableIndex].VariableBool = true;
                    break;

                case "BRST":
                    vm.list_VarBoolModels[anly.JudgeVariable(cm.Value_1, 1).VariableIndex].VariableBool = false;
                    break;

                case "CLSPORT":
                    if (vm.port_PD != null)
                    {
                        if (vm.port_PD.IsOpen)
                        {
                            vm.port_PD.DiscardInBuffer();
                            vm.port_PD.DiscardOutBuffer();
                            vm.port_PD.Close();
                        }
                    }
                    break;

                default:
                    if (string.Compare(vm.station_type.ToString(), "UV_Curing") == 0)
                    {
                        vm.port_PD.Write(cm.Command);
                        await Task.Delay(200);
                    }

                    else
                        vm.port_PD.Write(cm.Command + "/r");

                    await Task.Delay(vm.Int_Write_Delay);
                    break;
            }
            return cmdMsg;
        }

        /// <summary>
        /// High resolution delay by SpinWait and Stopwatch
        /// </summary>
        /// <param name="millisecond"></param>
        public static void Delay(int millisecond)
        {
            SpinWait sw = new SpinWait();
            var swatch = Stopwatch.StartNew();

            while (millisecond > swatch.ElapsedMilliseconds)
            {
                sw.SpinOnce();
            }
            swatch.Stop();
        }

        private async Task<string> Cmd_RecieveData_BufferJudge(string cmd, bool _is_port_close_after_CmdWrite)
        {
            string result = "";
            try
            {
                if (!vm.port_PD.IsOpen) return result;

                for (int i = 0; i < 7; i++)
                {
                    int size = vm.port_PD.BytesToRead;

                    if (size == 0)
                    {
                        Delay(10);

                        if (i != 6)
                            continue;
                        else return "";
                    }
                    else
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            Delay(5);

                            if (size == vm.port_PD.BytesToRead)
                                break;

                            size = vm.port_PD.BytesToRead;
                        }
                    }

                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                    //Show read back message
                    string msg = anly.Read_analysis(cmd, dataBuffer);

                    #region Analyze Dx? and show data
                    if (cmd.First() == 'D' && cmd.Count() == 7) //D1?, D2?...
                    {
                        int ch = int.Parse(cmd.ToCharArray(0, 3)[1].ToString());

                        ObservableCollection<string> list_words = new ObservableCollection<string>();  //one channel list[v1.v2.v3]
                        string[] words = msg.Split(',');  //V1,V2,V3 

                        if (words.Length == 3)
                        {
                            vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                            vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                            vm.list_GaugeModels[ch - 1].GaugeD0_3 = words[2];
                        }
                        else if (words.Length == 2)
                        {
                            vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                            vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                        }
                    }
                    #endregion

                    #region Get Board No.
                    if (cmd.Equals("ID?"))
                    {
                        if (msg.Equals("DiCon Fiberoptics Inc, MEMS UFA"))
                        {
                            vm.port_PD.Write("SN?" + "\r");
                            await Task.Delay(125);
                            size = vm.port_PD.BytesToRead;
                            dataBuffer = new byte[size];
                            length = vm.port_PD.Read(dataBuffer, 0, size);
                            result = anly.GetMessage(dataBuffer);
                        }
                    }
                    else
                    {
                        result = msg;
                    }

                    #endregion

                    result = msg;

                    if (_is_port_close_after_CmdWrite)
                    {
                        vm.port_PD.DiscardInBuffer();       // RX
                        vm.port_PD.DiscardOutBuffer();      // TX
                        vm.port_PD.Close();
                    }

                    break;
                }
            }
            catch { }

            return result;
        }


        private async Task<string> Cmd_RecieveData(string cmd, bool _is_port_close_after_CmdWrite)
        {
            string result = "";
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                    //Show read back message
                    string msg = anly.Read_analysis(cmd, dataBuffer);

                    #region Analyze Dx? and show data
                    if (cmd.First() == 'D' && cmd.Count() == 7) //D1?, D2?...
                    {
                        int ch = int.Parse(cmd.ToCharArray(0, 3)[1].ToString());

                        ObservableCollection<string> list_words = new ObservableCollection<string>();  //one channel list[v1.v2.v3]
                        string[] words = msg.Split(',');  //V1,V2,V3 

                        if (words.Length == 3)
                        {
                            vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                            vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                            vm.list_GaugeModels[ch - 1].GaugeD0_3 = words[2];
                        }
                        else if (words.Length == 2)
                        {
                            vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                            vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                        }
                    }
                    #endregion

                    #region Get Board No.
                    if (cmd.Equals("ID?"))
                    {
                        if (msg.Equals("DiCon Fiberoptics Inc, MEMS UFA"))
                        {
                            vm.port_PD.Write("SN?" + "\r");
                            await Task.Delay(125);
                            size = vm.port_PD.BytesToRead;
                            dataBuffer = new byte[size];
                            length = vm.port_PD.Read(dataBuffer, 0, size);
                            result = anly.GetMessage(dataBuffer);
                        }
                    }
                    else
                    {
                        result = msg;
                    }

                    #endregion

                    if (_is_port_close_after_CmdWrite)
                    {
                        vm.port_PD.DiscardInBuffer();       // RX
                        vm.port_PD.DiscardOutBuffer();      // TX
                        vm.port_PD.Close();
                    }
                }
            }
            catch { }

            return result;
        }

        public async Task D0_show()
        {
            if (!vm.PD_or_PM)  //PD mode
            {
                for (int i = 1; i <= vm.ch_count; i++)
                {
                    if (vm.list_GaugeModels[i - 1].boolGauge || vm.BoolAllGauge)
                    {
                        if (vm.IsGoOn)
                        {
                            vm.Save_cmd(new ComMember()
                            {
                                No = vm.Cmd_Count.ToString(),
                                Type = "PD",
                                Comport = vm.Selected_Comport,
                                Command = "DAC?",
                                Channel = i.ToString()
                            });
                            //vm.Cmd_Count++;
                        }
                        else
                        {
                            string cmd = "D" + i.ToString() + "?";

                            await vm.Port_ReOpen(vm.Selected_Comport);

                            try
                            {
                                await Get_Dac(cmd);

                                if (!vm.IsGoOn) vm.port_PD.Close();
                            }
                            catch { vm.Save_Log("Port is closed"); return; }
                        }
                    }
                }
            }

            else  //PM mode
            {
                string str_selected_com;
                if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                {
                    for (int ch = 0; ch < vm.ch_count; ch++)
                    {
                        if (!vm.list_GaugeModels[ch].boolGauge && !vm.BoolAllGauge) continue;

                        if (!string.IsNullOrEmpty(vm.SNMembers[ch].ProductType))
                        {
                            if (!vm.SNMembers[ch].ProductType.Contains("UFA")) continue;
                        }
                        else continue;

                        string cmd = "D1?";

                        if (!string.IsNullOrEmpty(vm.list_Board_Setting[ch][1]))
                        {
                            str_selected_com = vm.list_Board_Setting[ch][1];
                            vm.Save_Log("K V3", (ch + 1).ToString(), "Board:" + str_selected_com);
                            //MessageBox.Show("Ch:" + ch.ToString() + ", Board Com:" + str_selected_com);
                            try
                            {
                                if (vm.port_PD.PortName != str_selected_com)
                                    await vm.Port_ReOpen(str_selected_com);
                            }
                            catch { }
                        }
                        else
                        {
                            vm.list_D_All.Add(new ObservableCollection<string>());  //Add one channel list to All channel list
                            continue;
                        }

                        try
                        {
                            if (!vm.IsGoOn)  //Go is off
                            {
                                await anly.Port_ReOpen();
                                await Get_Dac(cmd);
                            }
                            else  //Go is on
                            {
                                vm.Save_cmd(new ComMember()
                                {
                                    YN = true,
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PM",
                                    Command = CommandList.DAC,
                                    Comport = vm.Selected_Comport
                                });
                            }
                        }
                        catch { vm.Save_Log(str_selected_com + " is closed"); return; }
                    }
                }
                else
                {
                    for (int ch = 0; ch < vm.ch_count; ch++)
                    {
                        string cmd = "D1?";

                        try
                        {
                            if (!vm.IsGoOn)  //Go is off
                            {
                                await vm.Port_ReOpen(vm.Selected_Comport);
                                await Get_Dac(cmd);
                            }
                            else  //Go is on
                            {
                                vm.Save_cmd(new ComMember()
                                {
                                    YN = true,
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PM",
                                    Command = CommandList.DAC,
                                    Comport = vm.Selected_Comport
                                });
                            }
                        }
                        catch { vm.Save_Log(vm.Selected_Comport + " is closed"); return; }
                    }
                }
            }
        }

        public async Task<string> Cmd_B_RecieveData_BufferJudge(string cmd, bool _is_port_close_after_CmdWrite)
        {
            string result = "";
            try
            {
                if (vm.port_PD_B.IsOpen)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        int size = vm.port_PD_B.BytesToRead;

                        if (size == 0)
                        {
                            Delay(10);

                            if (i != 6)
                                continue;
                            else return "";
                        }
                        else
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                Delay(5);

                                if (size == vm.port_PD_B.BytesToRead)
                                    break;

                                size = vm.port_PD_B.BytesToRead;
                            }
                        }

                        byte[] dataBuffer = new byte[size];
                        int length = vm.port_PD_B.Read(dataBuffer, 0, size);

                        //Show read back message
                        string msg = anly.Read_analysis(cmd, dataBuffer);

                        #region Analyze Dx? and show data
                        if (cmd.First() == 'D' && cmd.Count() == 7) //D1?, D2?...
                        {
                            int ch = int.Parse(cmd.ToCharArray(0, 3)[1].ToString());

                            ObservableCollection<string> list_words = new ObservableCollection<string>();  //one channel list[v1.v2.v3]
                            string[] words = msg.Split(',');  //V1,V2,V3 

                            if (words.Length == 3)
                            {
                                vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                                vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                                vm.list_GaugeModels[ch - 1].GaugeD0_3 = words[2];
                            }
                            else if (words.Length == 2)
                            {
                                vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                                vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                            }
                        }
                        #endregion

                        #region Get Board No.
                        if (cmd.Equals("ID?"))
                        {
                            if (msg.Equals("DiCon Fiberoptics Inc, MEMS UFA"))
                            {
                                vm.port_PD_B.Write("SN?" + "\r");
                                await Task.Delay(125);
                                size = vm.port_PD_B.BytesToRead;
                                dataBuffer = new byte[size];
                                length = vm.port_PD_B.Read(dataBuffer, 0, size);
                                result = anly.GetMessage(dataBuffer);
                            }
                        }
                        else
                        {
                            result = msg;
                        }

                        #endregion

                        if (_is_port_close_after_CmdWrite)
                        {
                            vm.port_PD_B.DiscardInBuffer();       // RX
                            vm.port_PD_B.DiscardOutBuffer();      // TX
                            vm.port_PD_B.Close();
                        }
                    }
                }
            }
            catch { }

            return result;
        }

        public async Task<bool> Cmd_Write_RecieveData(string cmd, bool _is_port_close_after_CmdWrite, int ch)
        {
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    vm.port_PD.Write(cmd + "\r");

                    await Task.Delay(vm.Int_Read_Delay);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                    //Show read back message
                    vm = anly._PM_read_analysis(cmd, dataBuffer, ch);

                    if (vm.Str_Command != "ID?")
                    {
                        #region Analyze Dx?
                        if (cmd.ToCharArray(0, 1)[0] == 'D' && cmd.ToCharArray(2, 1)[0] == '?') //D1?, D2?...
                        {
                            ObservableCollection<string> list_words = new ObservableCollection<string>();  //one channel list[v1.v2.v3]
                            string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 
                            foreach (string s in words) list_words.Add(s);  //Convert array to list
                            vm.list_D_All.Add(list_words);  //Add one channel list to All channel list
                            vm.List_D0_value = new ObservableCollection<ObservableCollection<string>>(vm.list_D_All); //Make propertychanged event happen                        
                        }
                        #endregion
                    }

                    if (_is_port_close_after_CmdWrite)
                    {
                        vm.port_PD.DiscardInBuffer();       // RX
                        vm.port_PD.DiscardOutBuffer();      // TX
                        vm.port_PD.Close();
                    }
                }
            }
            catch { }

            return vm.IsGoOn;
        }

        public async Task<bool> Write_Cmd(string cmd, bool Is_Port_Close)
        {
            try
            {
                if (vm.port_PD != null)
                    if (!vm.port_PD.IsOpen)
                    {
                        await vm.Port_ReOpen(vm.Selected_Comport);
                    }

                if (vm.port_PD.IsOpen)
                {
                    vm.port_PD.Write(cmd + "\r");

                    await Task.Delay(vm.Int_Read_Delay);

                    if (Is_Port_Close)
                    {
                        vm.port_PD.DiscardInBuffer();       // RX
                        vm.port_PD.DiscardOutBuffer();      // TX
                        vm.port_PD.Close();
                    }
                }
            }
            catch { }

            return vm.IsGoOn;
        }


        //private void ConnectGoLightTLS()
        //{
        //    if (vm.tls_GL.Open(vm.Golight_ChannelModel.Board_Port))
        //    {
        //        vm.isConnected = true;

        //        string ReadWLMinMax = vm.tls_GL.ReadWL_MinMax();
        //        string[] wl_min_max = ReadWLMinMax.Split(',');

        //        if (wl_min_max != null)
        //        {
        //            if (wl_min_max.Length == 2)
        //            {
        //                vm.float_TLS_WL_Range[0] = float.Parse(wl_min_max[0]);
        //                vm.float_TLS_WL_Range[1] = float.Parse(wl_min_max[1]);
        //            }
        //        }

        //        vm.Save_Log(new Models.LogMember()
        //        {
        //            isShowMSG = false,
        //            Message = "Golight TLS connected",
        //        });

        //        vm.Save_Log(new Models.LogMember()
        //        {
        //            isShowMSG = false,
        //            Message = "Get TLS WL Range",
        //            Result = ReadWLMinMax
        //        });
        //    }
        //    else
        //        vm.Save_Log(new Models.LogMember()
        //        {
        //            isShowMSG = true,
        //            Message = "Connect Golight TLS Fail"
        //        });


        //}

        private bool ConnectGolightTLS(DiCon.Instrument.HP.GLTLS.GLTLS port, string portName)
        {
            try
            {
                port.Open(portName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
                return false;
            }

            return true;
        }



        public async Task Connect_TLS()
        {
            try
            {
                if (vm.station_type == ComViewModel.StationTypes.BR && vm.is_BR_OSA)
                {
                    vm.Save_Log(new LogMember()
                    {
                        Status = "Connect TLS",
                        Message = "Station type is BR and isOSA on",
                        Result = "Connect Failed"
                    });
                    return;
                }

                switch (vm.Laser_type)
                {
                    case ComViewModel.LaserType.Agilent:

                        #region Tunable Laser setting
                        if (!vm.isConnected)
                        {
                            vm.tls = new HPTLS();
                            vm.tls.BoardNumber = vm.tls_BoardNumber;
                            vm.tls.Addr = vm.tls_Addr;

                            try
                            {
                                if (!vm.tls.Open())
                                {
                                    vm.Str_cmd_read = "GPIB Setting Error, Check Address.";
                                    vm.Show_Bear_Window(vm.Str_cmd_read, false, "String", false);
                                    return;
                                }
                                else
                                {
                                    double d = vm.tls.ReadWL();
                                    if (string.IsNullOrWhiteSpace(d.ToString()) || d < 0)
                                    {
                                        vm.Str_cmd_read = $"{vm.Laser_type} Laser Connection Failed";
                                        vm.Show_Bear_Window(vm.Str_cmd_read, false, "String", false);
                                        return;
                                    }
                                }
                                vm.tls.init();

                                vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                            }
                            catch (Exception ex)
                            {
                                vm.Str_cmd_read = "TLS GPIB Setting Error";
                                vm.Show_Bear_Window(vm.Str_cmd_read, false, "String", false);
                                MessageBox.Show(ex.StackTrace.ToString());
                            }

                            return;
                        }

                        #endregion

                        #region PowerMeter Setting
                        if (!vm.isConnected)
                        {
                            vm.pm = new HPPM();
                            vm.pm.Addr = vm.pm_Addr;
                            vm.pm.Slot = vm.PM_slot;
                            vm.pm.BoardNumber = vm.pm_BoardNumber;
                            if (vm.pm.Open() == false)
                            {
                                vm.Str_cmd_read = "PM GPIB Setting Error.  Check  Address.";
                                vm.Show_Bear_Window(vm.Str_cmd_read, false, "String", false);
                                return;
                            }
                            vm.pm.init();
                            vm.pm.setUnit(1);
                            vm.pm.AutoRange(true);
                            vm.pm.aveTime(vm.PM_AveTime);

                            try
                            {
                                if (vm.pm.Open())
                                    vm.Double_PM_Wavelength = vm.pm.ReadWL();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.StackTrace.ToString());
                            }
                        }

                        vm.isConnected = true;
                        #endregion                      

                        break;

                    case ComViewModel.LaserType.Keysight:

                        #region Tunable Laser setting
                        if (!vm.isConnected)
                        {
                            vm.tls = new HPTLS();
                            vm.tls.protocol = 1;
                            vm.tls.Addr_TCPIP = vm.TLS_TCPIP;

                            try
                            {
                                if (!vm.tls.Open())
                                {
                                    vm.Str_cmd_read = "Connect Keysight TLS error, Check setting.";
                                    return;
                                }
                                else
                                {
                                    double d = vm.tls.ReadWL();
                                    if (string.IsNullOrWhiteSpace(d.ToString()) || d < 0)
                                    {
                                        //vm.Str_cmd_read = "Laser Connection Failed";
                                        vm.Str_cmd_read = $"{vm.Laser_type} Laser Connection Failed";
                                        vm.Show_Bear_Window(vm.Str_cmd_read, false, "String", false);
                                        return;
                                    }
                                }
                                vm.tls.init();

                                vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                                //vm.isConnected = true;
                            }
                            catch (Exception ex)
                            {
                                vm.Str_cmd_read = "Keysight TLS Setting Error";
                                //vm.Show_Bear_Window(vm.Str_cmd_read, false, "String", false);
                                MessageBox.Show(ex.StackTrace.ToString());
                            }
                        }

                        #endregion

                        #region PowerMeter Setting
                        if (!vm.isConnected)
                        {
                            vm.pm = new HPPM();
                            vm.pm.Addr = vm.pm_Addr;
                            vm.pm.Slot = vm.PM_slot;
                            vm.pm.BoardNumber = vm.pm_BoardNumber;
                            if (vm.pm.Open() == false)
                            {
                                vm.Str_cmd_read = "PM GPIB Setting Error.  Check  Address.";
                                vm.Show_Bear_Window(vm.Str_cmd_read, false, "String", false);
                                return;
                            }
                            vm.pm.init();
                            vm.pm.setUnit(1);
                            vm.pm.AutoRange(true);
                            vm.pm.aveTime(vm.PM_AveTime);

                            try
                            {
                                if (vm.pm.Open())
                                    vm.Double_PM_Wavelength = vm.pm.ReadWL();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.StackTrace.ToString());
                            }
                        }

                        vm.isConnected = true;
                        #endregion                      

                        break;

                    case ComViewModel.LaserType.Golight:

                        if (!vm.isConnected)
                        {
                            if (!string.IsNullOrEmpty(vm.Golight_ChannelModel.Board_Port))
                            {
                                var task = Task.Run(() => ConnectGolightTLS(vm.tls_GL, vm.Golight_ChannelModel.Board_Port));
                                var result = (task.Wait(1500)) ? task.Result : false;

                                if (result)
                                {
                                    vm.isConnected = true;

                                    await Task.Delay(250);

                                    string ReadWLMinMax = vm.tls_GL.ReadWL_MinMax();
                                    string[] wl_min_max = ReadWLMinMax.Split(',');

                                    if (wl_min_max != null)
                                    {
                                        if (wl_min_max.Length == 2)
                                        {
                                            vm.float_TLS_WL_Range[0] = float.Parse(wl_min_max[0]);
                                            vm.float_TLS_WL_Range[1] = float.Parse(wl_min_max[1]);
                                        }
                                    }

                                    vm.Save_Log(new Models.LogMember()
                                    {
                                        isShowMSG = false,
                                        Message = "Golight TLS connected",
                                    });

                                    vm.Save_Log(new Models.LogMember()
                                    {
                                        isShowMSG = false,
                                        Message = "Get TLS WL Range",
                                        Result = ReadWLMinMax
                                    });
                                }
                                else
                                {
                                    Console.WriteLine("Timeout");

                                    vm.Save_Log(new Models.LogMember()
                                    {
                                        isShowMSG = false,
                                        Message = "Connect Golight TLS Fail"
                                    });
                                }
                            }
                            else
                                vm.Save_Log(new Models.LogMember()
                                {
                                    isShowMSG = true,
                                    Message = "Golight comport is null or empty"
                                });
                        }

                        break;

                    default:

                        #region Tunable Laser setting
                        if (!vm.isConnected)
                        {
                            vm.tls = new HPTLS();
                            vm.tls.BoardNumber = vm.tls_BoardNumber;
                            vm.tls.Addr = vm.tls_Addr;

                            try
                            {
                                if (!vm.tls.Open())
                                {
                                    vm.Str_cmd_read = "GPIB Setting Error, Check Address.";
                                    vm.Show_Bear_Window(vm.Str_cmd_read, false, "String", false);
                                    return;
                                }
                                else
                                {
                                    double d = vm.tls.ReadWL();
                                    if (string.IsNullOrWhiteSpace(d.ToString()) || d < 0)
                                    {
                                        //vm.Str_cmd_read = "Laser Connection Failed";
                                        vm.Str_cmd_read = $"{vm.Laser_type} Laser Connection Failed";
                                        vm.Show_Bear_Window(vm.Str_cmd_read, false, "String", false);
                                        return;
                                    }
                                }
                                vm.tls.init();

                                vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                            }
                            catch (Exception ex)
                            {
                                vm.Str_cmd_read = "TLS GPIB Setting Error";
                                vm.Show_Bear_Window(vm.Str_cmd_read, false, "String", false);
                                MessageBox.Show(ex.StackTrace.ToString());
                            }

                            return;
                        }

                        #endregion

                        #region PowerMeter Setting
                        if (!vm.isConnected)
                        {
                            vm.pm = new HPPM();
                            vm.pm.Addr = vm.pm_Addr;
                            vm.pm.Slot = vm.PM_slot;
                            vm.pm.BoardNumber = vm.pm_BoardNumber;
                            if (vm.pm.Open() == false)
                            {
                                vm.Str_cmd_read = "PM GPIB Setting Error.  Check  Address.";
                                vm.Show_Bear_Window(vm.Str_cmd_read, false, "String", false);
                                return;
                            }
                            vm.pm.init();
                            vm.pm.setUnit(1);
                            vm.pm.AutoRange(true);
                            vm.pm.aveTime(vm.PM_AveTime);

                            try
                            {
                                if (vm.pm.Open())
                                    vm.Double_PM_Wavelength = vm.pm.ReadWL();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.StackTrace.ToString());
                            }
                        }

                        vm.isConnected = true;
                        #endregion                      

                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        public async void Set_TLS_Active(bool _laserActive)
        {
            try
            {
                if (!vm.isConnected) await Connect_TLS();

                if (vm.isConnected)
                {
                    switch (vm.Laser_type)
                    {
                        case ComViewModel.LaserType.Agilent:
                            vm.tls.SetActive(_laserActive);
                            break;

                        case ComViewModel.LaserType.Golight:
                            vm.tls_GL.SetActive(_laserActive);
                            break;

                        case ComViewModel.LaserType.Keysight:
                            vm.tls.SetActive(_laserActive);
                            break;
                    }

                    await Task.Delay(vm.Int_Set_WL_Delay + 100);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        HPPDL pdl;

        public void PDL_Connect()
        {
            pdl = new HPPDL();
            pdl.BoardNumber = vm.pdl_BoardNumber;
            pdl.Addr = vm.pdl_Addr;
            pdl.Open();
            pdl.init();
            pdl.scanRate(8);
        }

        public void PDL_Start()
        {
            pdl = new HPPDL();
            pdl.BoardNumber = vm.pdl_BoardNumber;
            pdl.Addr = vm.pdl_Addr;
            pdl.Open();
            pdl.init();
            pdl.scanRate(8);

            if (pdl != null)
                pdl.startPolarizationScan();
        }

        public void PDL_Stop()
        {
            if (pdl != null)
            {
                pdl.stopPolarizationScan();
                pdl.Close();
            }
        }

        public async Task<double> PDL_Cal(int TimeSpan_Second)
        {
            PDL_Connect();
            PDL_Start();

            await Get_Power(0, true);
            await Task.Delay(100);

            DateTime dateTime_start = DateTime.Now;
            List<double> list_PDL_IL = new List<double>();
            while (true)
            {
                if (vm.isStop) break;

                await Get_Power(0, true);
                list_PDL_IL.Add(vm.Double_Powers[0]);

                TimeSpan dt = DateTime.Now - dateTime_start;
                if (dt.TotalSeconds > TimeSpan_Second) break;

                vm.Str_cmd_read = $"PDL Scan Timespan : {Math.Round(dt.TotalSeconds, 1)} s";
            }

            PDL_Stop();

            double Delta_IL = 0;

            if (list_PDL_IL.Count > 0)
                Delta_IL = Math.Round(list_PDL_IL.Max() - list_PDL_IL.Min(), 2);

            return Delta_IL;
        }

        //private string WL_Range_Analyze(double wl)
        //{
        //    //if (wl >= 1523 && wl <= 1620)
        //    //    return "C+L Band";
        //    if (wl >= 1625 && wl <= 1675)
        //        return "U Band";
        //    else if (wl >= 1560 && wl <= 1625)
        //        return "L Band";
        //    else if (wl >= 1520 && wl <= 1573)
        //        return "C Band";
        //    else if (wl >= 1460 && wl <= 1520)
        //        return "S Band";
        //    else if (wl >= 1360 && wl <= 1460)
        //        return "E Band";
        //    else if (wl >= 1260 && wl <= 1360)
        //        return "O Band";

        //    else return "";  //Out of range
        //}

        DiCon.Instrument.HP816x hp816x = new DiCon.Instrument.HP816x();
        DiCon.Instrument.LambdaScan lambda_scan = new DiCon.Instrument.LambdaScan();
        private bool m_HiSpeed = true;    // 40 nm/s
        public bool isLambdaInit = false;

        public bool Init_Lambda_Scan()
        {
            try
            {
                // 初始化            
                lambda_scan.Initial(hp816x, vm.tls_Addr, (int)vm.Double_Laser_Power);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Lambda_Scan_Setting(double Start_WL, double End_WL)
        {
            if (vm.Laser_type == ComViewModel.LaserType.Agilent)
            {
                double StepWL = 0.16;   //nm, wavelength sweep step 要改為 0.16nm 這是固定值. 因為韌體在移動Mirror時, 就是以 0.16nm/4ms

                lambda_scan.LambdaScan_STF_Mode_Setting(hp816x, vm.tls_Addr, Start_WL, End_WL, StepWL, m_HiSpeed, (int)vm.Double_Laser_Power);
            }
        }

        public void MTF_Scan(decimal m_WL_Start, decimal m_WL_End)
        {
            try
            {
                string cmd = string.Empty;

                m_WL_Start = Math.Round(m_WL_Start, 2, MidpointRounding.AwayFromZero);
                m_WL_End = Math.Round(m_WL_End, 2, MidpointRounding.AwayFromZero);

                cmd = $"SW {m_WL_Start},{m_WL_End}";

                vm.port_TLS_Filter.Write(cmd + "\r");
            }
            catch { }
        }

        public void TLS_Agilent_Sweep()
        {
            lambda_scan.LambdaScan_Sweep_Start(hp816x);
        }

        private bool _TLS_Unit = false;
        public void Switch_TLS_Unit()
        {
            _TLS_Unit = !_TLS_Unit;
            int tlsUt = _TLS_Unit ? 1 : 0;
            vm.tls.setUnit(tlsUt);
        }

        private bool _PM_Unit = false;
        public void Switch_PM_Unit()
        {
            _PM_Unit = !_PM_Unit;
            int pmUt = _TLS_Unit ? 1 : 0;
            vm.pm.setUnit(pmUt);
        }

        public async void Set_WL()
        {
            try
            {
                if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                {
                    if (vm.selected_band.Equals("C Band"))
                    {
                        switch (vm.product_type)
                        {
                            case "UFA":
                                await Set_WL(1548.5, false);
                                break;

                            case "UFA(H)":
                                await Set_WL(1548.5, false);
                                break;

                            case "UTF":
                                await Set_WL(1527.5, false);
                                break;

                            case "CTF":
                                await Set_WL(1527.5, false);
                                break;

                            case "MTF":
                                //cmd.Set_WL(1548.5);
                                break;
                        }

                    }
                    else if (vm.selected_band == "L Band")
                    {
                        switch (vm.product_type)
                        {
                            case "UFA":
                                Set_WL(1591, false, true);
                                break;

                            case "UFA(H)":
                                Set_WL(1591, false, true);
                                break;

                            case "UTF":
                                Set_WL(1568, false, true);
                                break;

                            case "CTF":
                                //cmd.Set_WL(1527.5);
                                break;

                            case "MTF":
                                //cmd.Set_WL(1548.5);
                                break;
                        }
                    }
                }
            }
            catch { }
        }

        public async Task<bool> Set_WL(double wl, bool readback)
        {
            try
            {
                wl = Math.Round(wl, 2);
                string band = Analysis.WL_Range_Analyze(wl);
                if (string.IsNullOrEmpty(band))
                {
                    vm.Str_cmd_read = "WL out of range";
                    vm.Save_Log(new LogMember() { Message = vm.Str_cmd_read,Result= wl.ToString(), isShowMSG = false });
                }
                else
                    vm.selected_band = band;

                await Connect_TLS();

                switch (vm.Laser_type)
                {
                    case ComViewModel.LaserType.Agilent:
                        await Task.Run(() => vm.tls.SetWL(wl));
                        break;

                    case ComViewModel.LaserType.Golight:
                        await Task.Run(() => vm.tls_GL.SetWL((float)wl));
                        break;

                    case ComViewModel.LaserType.Keysight:
                        await Task.Run(() => vm.tls.SetWL(wl));
                        break;

                    default:
                        await Task.Run(() => vm.tls.SetWL(wl));
                        break;
                }

                if (!vm.IsDistributedSystem)
                {
                    if (vm.PD_or_PM)
                        if (vm.PM_sync)
                            if (vm.pm != null) vm.pm.SetWL(wl);
                }

                Set_TLS_Filter(wl, false);

                if (readback)
                {
                    double wl_read = 0;
                    switch (vm.Laser_type)
                    {
                        case ComViewModel.LaserType.Agilent:
                            wl_read = vm.tls.ReadWL();
                            if (wl_read > 0)
                                vm.Double_Laser_Wavelength = wl_read;
                            else
                                vm.Save_Log(new LogMember() { Result = "Read WL:" + wl_read.ToString(), Message = "ReadWL back error" });
                            break;

                        case ComViewModel.LaserType.Golight:

                            wl_read = vm.tls_GL.ReadWL();

                            if (wl_read > 0)
                                vm.Double_Laser_Wavelength = wl_read;

                            if (wl_read != wl)
                                vm.Save_Log(new LogMember() { Result = "Read WL:" + wl_read.ToString(), Message = "SetWL failed" });
                            break;

                        case ComViewModel.LaserType.Keysight:
                            wl_read = vm.tls.ReadWL();
                            if (wl_read > 0)
                                vm.Double_Laser_Wavelength = wl_read;
                            else
                                vm.Save_Log(new LogMember() { Result = "Read WL:" + wl_read.ToString(), Message = "ReadWL back error" });
                            break;
                    }

                    if (vm.PD_or_PM)
                        if (vm.PM_sync)
                            if (vm.pm != null)
                                vm.Double_PM_Wavelength = vm.pm.ReadWL();
                }

                if (vm.station_type == ComViewModel.StationTypes.Testing && vm.float_WL_Ref.Count != 0)
                        vm.Double_PM_Ref = vm.float_WL_Ref[0];
            }
            catch { vm.Save_Log("Set WL", "Set WL Error", false); }

            return true;
        }

        public async void Set_WL(double wl, bool readback, bool isCloseTLS_Filter)
        {
            try
            {
                wl = Math.Round(wl, 2);
                string band = Analysis.WL_Range_Analyze(wl);
                if (string.IsNullOrEmpty(band))
                {
                    vm.Str_cmd_read = "WL out of range";
                    vm.Save_Log(new LogMember() { Message = vm.Str_cmd_read, isShowMSG = false });
                }
                else
                    vm.selected_band = band;

                await Connect_TLS();

                switch (vm.Laser_type)
                {
                    case ComViewModel.LaserType.Agilent:
                        await Task.Run(() => vm.tls.SetWL(wl));
                        break;

                    case ComViewModel.LaserType.Golight:
                        await Task.Run(() => vm.tls_GL.SetWL((float)wl));
                        break;

                    case ComViewModel.LaserType.Keysight:
                        await Task.Run(() => vm.tls.SetWL(wl));
                        break;

                    default:
                        await Task.Run(() => vm.tls.SetWL(wl));
                        break;
                }

                if (!vm.IsDistributedSystem)
                {
                    if (vm.PD_or_PM)
                        if (vm.PM_sync)
                            if (vm.pm != null) vm.pm.SetWL(wl);
                }

                Set_TLS_Filter(wl, isCloseTLS_Filter);

                await Task.Delay(vm.Int_Read_Delay);

                if (readback)
                {
                    double wl_read = 0;
                    switch (vm.Laser_type)
                    {
                        case ComViewModel.LaserType.Agilent:
                            wl_read = vm.tls.ReadWL();
                            if (wl_read > 0)
                                vm.Double_Laser_Wavelength = wl_read;
                            else
                                vm.Save_Log(new LogMember() { Result = "Read WL:" + wl_read.ToString(), Message = "ReadWL back error" });
                            break;

                        case ComViewModel.LaserType.Golight:

                            wl_read = vm.tls_GL.ReadWL();

                            if (wl_read > 0)
                                vm.Double_Laser_Wavelength = wl_read;

                            if (wl_read != wl)
                                vm.Save_Log(new LogMember() { Result = "Read WL:" + wl_read.ToString(), Message = "SetWL failed" });
                            break;

                        case ComViewModel.LaserType.Keysight:
                            wl_read = vm.tls.ReadWL();
                            if (wl_read > 0)
                                vm.Double_Laser_Wavelength = wl_read;
                            else
                                vm.Save_Log(new LogMember() { Result = "Read WL:" + wl_read.ToString(), Message = "ReadWL back error" });
                            break;

                        default:
                            wl_read = vm.tls.ReadWL();
                            if (wl_read > 0)
                                vm.Double_Laser_Wavelength = wl_read;
                            else
                                vm.Save_Log(new LogMember() { Result = "Read WL:" + wl_read.ToString(), Message = "ReadWL back error" });
                            break;
                    }

                    if (vm.PD_or_PM)
                        if (vm.PM_sync)
                            if (vm.pm != null)
                                vm.Double_PM_Wavelength = vm.pm.ReadWL();
                }

                if (vm.station_type == ComViewModel.StationTypes.Testing)
                {
                    if (vm.float_WL_Ref.Count != 0)
                        vm.Double_PM_Ref = vm.float_WL_Ref[0];
                }
            }
            catch { vm.Save_Log("Set WL", "Set WL Error", false); }
        }

        public async void Set_TLS_Power(double power, bool readback)
        {
            try
            {
                if (!vm.isConnected) await Connect_TLS();

                switch (vm.Laser_type)
                {
                    case ComViewModel.LaserType.Agilent:
                        await Task.Run(() => vm.tls.SetPower(power));
                        break;

                    case ComViewModel.LaserType.Golight:
                        await Task.Run(() => vm.tls_GL.SetPower(power));
                        break;

                    case ComViewModel.LaserType.Keysight:
                        await Task.Run(() => vm.tls.SetPower(power));
                        break;

                    default:
                        await Task.Run(() => vm.tls.SetPower(power));
                        break;
                }

                await Task.Delay(vm.Int_Set_WL_Delay);

                if (readback)
                {
                    if (vm.Laser_type.Equals("Golight"))
                        vm.Double_Laser_Power = Math.Round(vm.tls_GL.ReadPower(), 1);
                }
            }
            catch { vm.Save_Log("Set TLS Power", "Set Power Error", true); }
        }

        //public void SendCommand(string cmd)
        //{
        //    try
        //    {
        //        if (vm.device != null)
        //        {
        //            vm.device.Write(cmd);
        //        }
        //    }
        //    catch { }
        //}

        //public string ReadGPIB()
        //{
        //    try
        //    {
        //        if (vm.device != null)
        //        {
        //            return vm.device.ReadString();
        //        }
        //        return "-100";
        //    }
        //    catch 
        //    {
        //        return "-100";
        //    }
        //}

        //public async void init_OSA()
        //{
        //    SendCommand("*RST");
        //    SendCommand("DISP:WIND:TRAC:STAT TRB,ON");
        //    SendCommand("INIT:CONT OFF");
        //    SendCommand("*OPC?");
        //    while (Convert.ToInt32(ReadGPIB()) != 1)
        //    {
        //        //Thread.Sleep(200);
        //        await Task.Delay(200);
        //    }
        //}

        public bool Init_TLS_Filter()
        {
            try
            {
                if (vm.is_TLS_Filter)
                {
                    if (vm.port_TLS_Filter != null)
                        if (!string.IsNullOrEmpty(vm.port_TLS_Filter.PortName))
                        {
                            if (!vm.port_TLS_Filter.IsOpen)
                                vm.port_TLS_Filter.Open();
                        }
                }

                if (vm.port_TLS_Filter != null)
                {
                    return vm.port_TLS_Filter.IsOpen;
                }
                else return false;
            }
            catch
            {
                return false;
            }
        }

        public void Close_TLS_Filter()
        {
            if (vm.is_TLS_Filter && vm.port_TLS_Filter != null)
                if (vm.port_TLS_Filter.IsOpen)
                {
                    vm.port_TLS_Filter.DiscardInBuffer();
                    vm.port_TLS_Filter.DiscardOutBuffer();

                    vm.port_TLS_Filter.Close();
                }
        }


        public async void Set_TLS_Filter(double wl)
        {
            try
            {
                if (vm.is_TLS_Filter)
                {
                    if (vm.port_TLS_Filter != null)
                        if (!string.IsNullOrEmpty(vm.port_TLS_Filter.PortName))
                        {
                            if (!vm.port_TLS_Filter.IsOpen)
                                vm.port_TLS_Filter.Open();

                            await Task.Delay(100);

                            vm.port_TLS_Filter.Write($"WL {wl}\r");

                            await Task.Delay(vm.Int_Read_Delay);

                            if (vm.port_TLS_Filter.IsOpen)
                            {
                                vm.port_TLS_Filter.DiscardInBuffer();
                                vm.port_TLS_Filter.DiscardOutBuffer();

                                vm.port_TLS_Filter.Close();
                            }
                        }
                }
            }
            catch
            {
                vm.Str_cmd_read = "Set TLS Filter Error";
                vm.Save_Log(new LogMember() { isShowMSG = false, Status = "Set TLS WL", Result = "Set TLS Filter Failed", Message = $"WL : {wl}" });
            }
        }

        public async void Set_TLS_Filter(double wl, bool isClosePort)
        {
            try
            {
                if (vm.is_TLS_Filter)
                {
                    if (vm.port_TLS_Filter != null)
                        if (!string.IsNullOrEmpty(vm.port_TLS_Filter.PortName))
                        {
                            if (!vm.port_TLS_Filter.IsOpen)
                            {
                                vm.port_TLS_Filter.Open();
                                await Task.Delay(100);
                            }

                            vm.port_TLS_Filter.Write($"WL {wl}\r");

                            if (isClosePort)
                            {
                                await Task.Delay(vm.Int_Read_Delay);

                                if (vm.port_TLS_Filter.IsOpen)
                                {
                                    vm.port_TLS_Filter.DiscardInBuffer();
                                    vm.port_TLS_Filter.DiscardOutBuffer();

                                    vm.port_TLS_Filter.Close();
                                }
                            }
                        }
                }
            }
            catch
            {
                vm.Str_cmd_read = "Set TLS Filter Error";
                vm.Save_Log(new LogMember() { isShowMSG = false, Status = "Set TLS WL", Result = "Set TLS Filter Failed", Message = $"WL : {wl}" });
            }
        }

        public async void Set_TLS_Lambda_Scan()
        {
            try
            {
                if (!vm.isConnected) await Connect_TLS();

                vm.tls_GL.LambdaScan(vm.float_TLS_WL_Range[0], vm.float_TLS_WL_Range[1], 0.02);
            }
            catch { }
        }

        public async void Set_Dac(string comport, GaugeModel gm)
        {
            await vm.Port_ReOpen(comport);

            try
            {
                if (vm.PD_or_PM)  //PM mode
                {
                    if (vm.Control_board_type == 0)  //Control board type: UFV
                    {
                        string TF_or_VOA = "D", DAC;
                        switch (gm.GaugeD0_Select)
                        {
                            case "1":
                                if (!string.IsNullOrEmpty(gm.GaugeD0_1))
                                    DAC = string.Format("{0},0,{1}", gm.GaugeD0_1, gm.GaugeD0_3);
                                else
                                    return;
                                break;
                            case "2":
                                if (!string.IsNullOrEmpty(gm.GaugeD0_2))
                                    DAC = string.Format("0,{0},{1}", gm.GaugeD0_2, gm.GaugeD0_3);
                                else
                                    return;
                                break;
                            case "3":
                                if (!string.IsNullOrEmpty(gm.GaugeD0_1) && !string.IsNullOrEmpty(gm.GaugeD0_2))
                                {
                                    string[] preDac = await anly.Analyze_PreDAC(comport, gm.GaugeChannel);
                                    DAC = string.Format("{0},{1},{2}", preDac[0], preDac[1], gm.GaugeD0_3);
                                }
                                else
                                    return;
                                break;
                            default:
                                DAC = string.Format("{0},{0},{0}", gm.GaugeD0_1.ToString(), gm.GaugeD0_2.ToString(), gm.GaugeD0_3.ToString());
                                break;
                        }
                        vm.Str_Command = string.Format("{0}1 {1}", TF_or_VOA, DAC);
                    }
                    else  //Control board type: V
                    {
                        string TF_or_VOA = "D", DAC;
                        switch (gm.GaugeD0_Select)
                        {
                            case "1":
                                if (!string.IsNullOrEmpty(gm.GaugeD0_1))
                                    DAC = string.Format("{0},0", gm.GaugeD0_1.ToString());
                                else
                                    return;
                                break;
                            case "2":
                                if (!string.IsNullOrEmpty(gm.GaugeD0_2))
                                    DAC = string.Format("0,{0}", gm.GaugeD0_2.ToString());
                                else
                                    return;
                                break;
                            case "3":
                                return;
                            default:
                                vm.Str_cmd_read = "Selected Dac Textbox error";
                                return;
                        }
                        vm.Str_Command = string.Format("{0}1 {1}", TF_or_VOA, DAC);
                    }

                    vm.Str_cmd_read = vm.Str_Command;
                    vm.port_PD.Write(vm.Str_Command + "\r");
                    await Task.Delay(vm.Int_Write_Delay);
                    vm.port_PD.Close();
                }
                else   //PD mode
                {
                    string TF_or_VOA, DAC;

                    switch (gm.GaugeD0_Select)
                    {
                        case "1":
                            TF_or_VOA = "D";
                            if (!string.IsNullOrEmpty(gm.GaugeD0_1))
                                DAC = gm.GaugeD0_1.ToString();
                            else
                                DAC = "0";
                            break;
                        case "2":
                            TF_or_VOA = "D";
                            if (!string.IsNullOrEmpty(gm.GaugeD0_2))
                                DAC = "-" + gm.GaugeD0_2.ToString();
                            else
                                DAC = "0";
                            break;
                        case "3":
                            TF_or_VOA = "VOA";
                            if (!string.IsNullOrEmpty(gm.GaugeD0_2))
                                DAC = gm.GaugeD0_3.ToString();
                            else
                                DAC = "0";
                            break;
                        default:
                            vm.Str_cmd_read = "Selected Dac Textbox error";
                            return;
                    }

                    vm.Str_Command = string.Format("{0}{1} {2}", TF_or_VOA, gm.GaugeChannel, DAC);
                    vm.port_PD.Write(vm.Str_Command + "\r");
                    await Task.Delay(vm.Int_Write_Delay);

                    if (vm.IsDistributedSystem)
                        vm.port_PD.Close();
                }
            }
            catch { vm.Str_cmd_read = "Write Dac Error"; }
        }

        public async void Set_V3_Dac(string selected_comport, int dac)
        {
            //Reset COM port
            if (string.IsNullOrEmpty(selected_comport)) return;
            if (string.IsNullOrEmpty(dac.ToString())) return;

            await vm.Port_ReOpen(selected_comport);

            //Set Dac
            try
            {
                vm.Str_Command = "D1 0,0," + (dac).ToString();  //cmd = D1 0,0,1000
                vm.port_PD.Write(vm.Str_Command + "\r");
                await Task.Delay(vm.Int_Write_Delay);
                vm.port_PD.Close();
            }
            catch { vm.Str_cmd_read = "Write Dac Error"; }
        }

        public async void Set_V3_Volt(string selected_comport, double volt)
        {
            //Reset COM port
            if (string.IsNullOrEmpty(selected_comport)) return;
            if (string.IsNullOrEmpty(volt.ToString())) return;

            int final_dac = 0;

            await vm.Port_ReOpen(selected_comport);

            if (vm.station_type != ComViewModel.StationTypes.Hermetic_Test)
            {
                return;
                //If station is not Hermetic_Test, then we need a method to find out the name of this control board.
            }
            else
            {
                #region Read Board Table
                List<double> list_voltage = new List<double>();
                List<int> list_dac = new List<int>();

                int count = 0;
                foreach (string strline in vm.board_read[vm.switch_index - 1])
                {
                    string[] board_read = strline.Split(',');
                    if (board_read.Length <= 1)
                        continue;

                    double voltage = double.Parse(board_read[0]);
                    int board_dac = int.Parse(board_read[1]);

                    list_voltage.Add(voltage);
                    list_dac.Add(board_dac);

                    if (voltage >= volt && count > 0)
                    {
                        final_dac = board_dac;
                        break;
                    }

                    count++;
                }
                #endregion
            }

            //Set Dac
            try
            {
                vm.Str_Command = "D1 0,0," + (final_dac).ToString();  //cmd = D1 0,0,1000
                vm.port_PD.Write(vm.Str_Command + "\r");
                await Task.Delay(vm.Int_Write_Delay);
                vm.port_PD.Close();
            }
            catch { vm.Str_cmd_read = "Write Dac Error"; }
        }


        public async Task Get_Dac(string cmd)
        {
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    vm.port_PD.Write(cmd + "\r");

                    await Task.Delay(vm.Int_Read_Delay);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    if (size > 0)
                    {
                        dataBuffer = new byte[size];
                        int length = vm.port_PD.Read(dataBuffer, 0, size);
                    }

                    //資料分析並顯示於狀態列
                    anly.Read_analysis(cmd, dataBuffer);

                    #region Analyze Dx? and show data
                    if (cmd.ToCharArray(0, 1)[0] == 'D' && cmd.ToCharArray(2, 1)[0] == '?') //D1?, D2?...
                    {
                        int ch = int.Parse(cmd.ToCharArray(0, 3)[1].ToString());

                        string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 

                        if (words.Length == 2)
                        {
                            vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                            vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                        }
                        if (words.Length == 3)
                        {
                            vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                            vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                            vm.list_GaugeModels[ch - 1].GaugeD0_3 = words[2];
                        }
                    }
                    #endregion                    
                }
            }
            catch { }
        }

        public async Task<string> Get_Power()
        {
            string str = "";
            if (!vm.IsDistributedSystem)
                switch (vm.GetPWSettingModel.TypeName)
                {
                    case "PM":
                        int ch = 1;
                        if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test) ch = vm.switch_index;

                        double p = await Get_PM_Value((ch - 1));  //Y axis value
                        str = p.ToString();

                        break;

                    case "PD":
                        string comport = vm.GetPWSettingModel.Comport;
                        str = await Get_PD_Value(comport);  //Y axis value

                        for (int i = 0; i < vm.ch_count; i++)
                        {
                            if (vm.BoolAllGauge || vm.list_GaugeModels[i].boolGauge)
                            {
                                vm.list_GaugeModels[i].GaugeValue = vm.Double_Powers[i].ToString();
                            }
                        }

                        break;

                    case "Tablet":

                        break;
                }
            else  //Distribution System
            {
                for (int ch = 0; ch <= vm.list_ChannelModels.Count; ch++)
                {
                    string rs232_cmd = vm.list_ChannelModels[ch - 1].PM_GetPower_CMD;

                    if (vm.list_ChannelModels[ch - 1].PM_Type == "GPIB")
                    {

                    }
                    else if (vm.list_ChannelModels[ch - 1].PM_Type == "RS232")
                    {
                        string comport = vm.list_ChannelModels[ch - 1].PM_Board_Port;

                        await Get_IL_rs232(ch, comport, rs232_cmd);  //Y axis value

                        if (ch == 0)
                        {
                            for (int i = 0; i < vm.ch_count; i++)
                            {
                                vm.list_GaugeModels[i].GaugeValue = vm.Double_Powers[i].ToString();
                            }
                        }
                        else
                        {
                            double p = vm.Double_Powers[ch - 1];
                            vm.list_GaugeModels[ch - 1].GaugeValue = p.ToString();
                        }
                    }
                    str = vm.list_GaugeModels[0].GaugeValue;
                }
            }

            return str;
        }

        public async Task<List<List<double>>> Get_Power(int ch, bool isRead)
        {
            List<List<double>> listDD = new List<List<double>>();

            try
            {
                if (!vm.IsDistributedSystem)
                {
                    switch (vm.GetPWSettingModel.TypeName)
                    {
                        case "PM":
                            double p = await Get_PM_Value((ch));  //Y axis value

                            break;

                        case "PD":
                            string comport = vm.GetPWSettingModel.Comport;

                            if (!vm.Is_FastScan_Mode)
                            {
                                await Get_PD_Value(comport);  //Y axis value

                                for (int i = 0; i < vm.ch_count; i++)
                                {
                                    if (vm.BoolAllGauge || vm.list_GaugeModels[i].boolGauge)
                                    {
                                        vm.list_GaugeModels[i].GaugeValue = vm.Double_Powers[i].ToString();
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(comport))
                                {
                                    if (!vm.port_PD.IsOpen)
                                        await vm.Port_ReOpen(comport);

                                    if (vm.port_PD.IsOpen)
                                    {
                                        vm.port_PD.Write("P0?\r");

                                        if (vm.isGetPowerWaitForReadBack || (!vm.IsDistributedSystem && !vm.PD_or_PM && vm.Is_FastScan_Mode))
                                            await Task.Delay(vm.Int_Read_Delay);

                                        //for (int i = 0; i < 10; i++)
                                        //{
                                        //    int size1 = vm.port_PD.BytesToRead;
                                        //    if (size1 > 0) 
                                        //        break;
                                        //}

                                        int size = vm.port_PD.BytesToRead;
                                        byte[] dataBuffer = new byte[size];
                                        int length = vm.port_PD.Read(dataBuffer, 0, size);

                                        //Show read back message
                                        string msg = anly.Read_analysis("P0_Read", dataBuffer);

                                        if (vm.Double_Powers[ch] != 0 && !string.IsNullOrEmpty(msg))
                                        {
                                            int loop = vm.Double_Powers.Count / vm.ch_count;
                                            for (int i = 0; i < loop; i++)
                                            {
                                                vm.list_GaugeModels[ch].GaugeValue = vm.Double_Powers[ch + (8 * i)].ToString();

                                                //Only distributedSystem will auto update chart
                                                if (!vm.IsDistributedSystem && vm.Is_FastScan_Mode)
                                                {
                                                    DataPoint dp = new DataPoint(vm.wl_list[vm.ChartNowModel.list_dataPoints[ch].Count], vm.Double_Powers[ch + (8 * i)]);
                                                    vm.ChartNowModel.list_dataPoints[ch].Add(dp);
                                                    vm.Save_All_PD_Value[ch].Add(dp);
                                                    //vm.wl_list_index++;

                                                    //vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                                                    //vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries  
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            break;

                        case "Tablet":

                            break;
                    }
                }
                else
                {
                    ch++;
                    string rs232_cmd = vm.list_ChannelModels[ch - 1].PM_GetPower_CMD;

                    switch (vm.list_ChannelModels[ch - 1].PM_Type)
                    {
                        case "GPIB":
                            break;

                        case "RS232":

                            #region RS232
                            string comport = vm.list_ChannelModels[ch - 1].PM_Board_Port;

                            listDD = await Get_IL_rs232(ch, comport, rs232_cmd, isRead);  //Y axis value

                            //Analyze feedback power in buffer
                            if (!isRead)
                            {
                                if (vm.list_ChannelModels.Count == 1 && listDD.Count > 1)
                                    return listDD;
                                else
                                {
                                    if (ch == 0)
                                    {
                                        for (int i = 0; i < vm.ch_count; i++)
                                        {
                                            vm.list_GaugeModels[i].GaugeValue = Math.Round(vm.Double_Powers[i], vm.decimal_place).ToString();
                                        }
                                    }
                                    else
                                    {
                                        double p = Math.Round(vm.Double_Powers[ch - 1], vm.decimal_place);
                                        vm.list_GaugeModels[ch - 1].GaugeValue = p.ToString();
                                    }

                                    if (listDD.Count > 0)
                                        if (listDD[0].Count > 0)
                                        {
                                            foreach (double IL in listDD[0])
                                            {
                                                DataPoint dp = new DataPoint(vm.wl_list[vm.wl_list_index], IL);
                                                vm.ChartNowModel.list_dataPoints[ch - 1].Add(dp);
                                                vm.Save_All_PD_Value[ch - 1].Add(dp);
                                                vm.wl_list_index++;
                                            }
                                        }
                                }
                            }
                            else
                            {
                                if (ch == 0)
                                {
                                    for (int i = 0; i < vm.ch_count; i++)
                                    {
                                        vm.list_GaugeModels[i].GaugeValue = Math.Round(vm.Double_Powers[i], vm.decimal_place).ToString();
                                    }
                                }
                                else
                                {
                                    double p = Math.Round(vm.Double_Powers[ch - 1], vm.decimal_place);
                                    vm.list_GaugeModels[ch - 1].GaugeValue = p.ToString();
                                }

                                //Get the rest of IL
                                if (listDD.Count > 0)
                                {
                                    foreach (double IL in listDD[0])
                                    {
                                        vm.list_GaugeModels[ch - 1].GaugeValue = IL.ToString();

                                        if (vm.wl_list.Count > vm.wl_list_index)
                                        {
                                            DataPoint dp = new DataPoint(vm.wl_list[vm.wl_list_index], IL);
                                            vm.ChartNowModel.list_dataPoints[ch - 1].Add(dp);
                                            vm.Save_All_PD_Value[ch - 1].Add(dp);

                                            vm.Plot_Series[ch - 1].Points.Add(dp);

                                            vm.wl_list_index++;
                                        }
                                    }
                                }
                            }
                            #endregion

                            break;
                    }
                }
            }
            catch (Exception ex) { throw ex; }

            return listDD;
        }

        public async Task<string> Get_Board_ID(string comport, int ch)
        {
            string ID = "";

            try
            {
                string[] myPorts = System.IO.Ports.SerialPort.GetPortNames(); //取得所有的comport

                await Task.Delay(10);

                for (int i = 0; i < myPorts.Length; i++)
                {
                    try
                    {
                        vm.list_combox_comports.Add(myPorts[i]);
                    }
                    catch { }
                }

                #region Get Board ID     

                if (myPorts.Contains(comport))
                {
                    await vm.Port_ReOpen(comport);

                    await Cmd_Write_RecieveData($"SN{ch}?", true, 0);

                    //PM板指令與PD板不同，PM板指令錯誤時會回傳0
                    if (vm.Str_cmd_read.Equals("0"))
                    {
                        await vm.Port_ReOpen(comport);
                        await Cmd_Write_RecieveData($"SN?", true, 0);
                    }

                    ID = vm.Str_cmd_read;

                    vm.Save_Log(new LogMember()
                    {
                        Status = "Get Board ID",
                        Channel = ch.ToString(),
                        Message = ID
                    });
                }
                else
                    vm.Save_Log(new LogMember()
                    {
                        isShowMSG = true,
                        Status = "Get Board ID",
                        Channel = ch.ToString(),
                        Message = "Comport is not exist"
                    });

                #endregion                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }

            return ID;
        }

        public async Task<bool> Get_PD_Value()  //PD Value save in vm.Float_PD
        {
            try
            {
                vm.port_PD.Write("P0?" + "\r");
                await Task.Delay(vm.Int_Read_Delay);
            }
            catch { return false; }

            int size = vm.port_PD.BytesToRead;
            byte[] dataBuffer = new byte[size];
            int length = vm.port_PD.Read(dataBuffer, 0, size);

            if (dataBuffer.Length > 0)
                vm = anly._K_WL_analysis(dataBuffer);

            return true;
        }

        public async Task<string> Get_PD_Value_1ch(int ch)
        {
            string msg = "";
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    vm.Str_Command = $"P{ch + 1}?";
                    vm.port_PD.Write("P0?\r");

                    await Task.Delay(vm.Int_Read_Delay);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                    //Show read back message
                    msg = anly.GetMessage(dataBuffer);
                    vm.msg = msg;
                }
            }
            catch { }

            return msg;
        }

        public async Task<List<double>> Get_IL_rs232(int ch, string comport, string cmd)
        {
            if (!string.IsNullOrEmpty(comport))
            {
                if (!vm.port_PD.IsOpen)
                    await vm.Port_ReOpen(comport);
            }

            if (!vm.port_PD.IsOpen)
            {
                return new List<double>();
            }
            //for (int i = 0; i < 3; i++)
            //{
            //    if (!vm.port_PD.IsOpen)
            //        await Task.Delay(100);
            //    else break;
            //}

            vm.port_PD.Write(cmd + "\r");

            await Task.Delay(vm.Int_Read_Delay);

            int size = vm.port_PD.BytesToRead;
            byte[] dataBuffer = new byte[size];
            int length = vm.port_PD.Read(dataBuffer, 0, size);

            //Show read back message
            string msg = anly.Read_analysis(cmd, dataBuffer);

            if (msg != "ID?" && msg != "PD?" && msg != "P0?")
            {
                try
                {
                    if (double.TryParse(msg, out double power))
                    {
                        if (vm.dB_or_dBm)  //dB
                        {
                            if (vm.float_WL_Ref.Count > 0)
                                power = Math.Round(power - vm.float_WL_Ref[0], 4);

                            vm.Double_Powers[ch - 1] = power;
                        }
                        else  //dBm
                        {
                            vm.Double_Powers[ch - 1] = power;
                        }

                        await Task.Delay(vm.Int_Set_WL_Delay);
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.StackTrace.ToString()); }
            }

            return vm.Double_Powers;
        }

        public async Task<List<List<double>>> Get_IL_rs232(int ch, string comport, string cmd, bool isRead)
        {
            if (!string.IsNullOrEmpty(comport))
            {
                if (!vm.port_PD.IsOpen)
                    await vm.Port_ReOpen(comport);
            }

            List<List<double>> listDD = new List<List<double>>();

            vm.port_PD.Write(cmd + "\r");

            if (isRead)
                await Task.Delay(vm.Int_Read_Delay);

            int size = vm.port_PD.BytesToRead;
            byte[] dataBuffer = new byte[size];
            int length = vm.port_PD.Read(dataBuffer, 0, size);

            //Show read back message
            string msg = anly.Read_analysis(cmd, dataBuffer);

            if (vm.Is_FastScan_Mode)
            {
                listDD.Add(new List<double>());

                if (true)
                {
                    if (size > 0)
                        vm.IL_ALL += msg;

                    List<string> msgList = vm.IL_ALL.Split('-').ToList();
                    string str_saved_IL = "";

                    if (msgList.Count > 2)  //at least two IL in list
                    {
                        msgList.RemoveAt(0);

                        for (int i = 0; i < msgList.Count; i++)
                        {
                            str_saved_IL += "-" + msgList[i];

                            if (double.TryParse(msgList[i], out double power))
                            {
                                power = power * -1;
                                if (vm.dB_or_dBm)  //dB
                                {
                                    if (vm.float_WL_Ref.Count > 0)
                                        power = Math.Round(power - vm.float_WL_Ref[0], 4);

                                    vm.Double_Powers[ch - 1] = power;
                                }
                                else  //dBm
                                {
                                    vm.Double_Powers[ch - 1] = power;
                                }

                                listDD[0].Add(power);
                            }
                        }

                        vm.IL_ALL = vm.IL_ALL.Substring(str_saved_IL.Length);
                    }
                }
            }
            else  //set IL immediately
            {
                if (double.TryParse(msg, out double power))
                    try
                    {
                        if (vm.dB_or_dBm)  //dB
                        {
                            if (vm.float_WL_Ref.Count > 0)
                            {
                                vm.Save_Log(new LogMember() { isShowMSG = false, Status = "WL Scan", Channel = "1", Message = $"WL:{vm.Double_Laser_Wavelength}", Result = $"dBm:{power}, Ref:{vm.float_WL_Ref[0]}" });
                                power = Math.Round(power - vm.float_WL_Ref[0], vm.decimal_place);
                            }

                            vm.Double_Powers[ch - 1] = Math.Round(power, vm.decimal_place);
                        }
                        else  //dBm
                        {
                            vm.Double_Powers[ch - 1] = Math.Round(power, vm.decimal_place);
                        }

                        await Task.Delay(vm.Int_Set_WL_Delay);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.StackTrace.ToString()); }

                listDD.Add(new List<double>() { vm.Double_Powers[ch - 1] });
            }

            return listDD;
        }


        public async Task<string> Get_PD_Value(string comport)
        {
            if (!string.IsNullOrEmpty(comport))
            {
                if (!vm.port_PD.IsOpen)
                    await vm.Port_ReOpen(comport);
            }

            vm.port_PD.Write("P0?\r");

            if (vm.isGetPowerWaitForReadBack)
                await Task.Delay(vm.Int_Read_Delay);
            else
                await Task.Delay(vm.Int_Write_Delay);

            string str = await Cmd_RecieveData_BufferJudge("P0_Read", false);

            return str;
        }

        public async Task<double> Get_PM_Value(int ch) //PM Value save in vm.Double_Powers
        {
            double power = 0;
            try
            {
                if (vm.dB_or_dBm)  //dB
                {
                    double refValue = vm.float_WL_Ref.Count > 0 ? vm.float_WL_Ref[0] : 0;

                    power = Math.Round(vm.pm.ReadPower() - refValue, vm.decimal_place);
                }
                else  //dBm
                    power = Math.Round(vm.pm.ReadPower(), vm.decimal_place);

                vm.Double_Powers[ch] = power;

                vm.list_GaugeModels[ch].GaugeValue = vm.Double_Powers[ch].ToString();

                await Task.Delay(vm.Int_Set_WL_Delay);
            }
            catch (Exception ex) { MessageBox.Show(ex.StackTrace.ToString()); }

            return power;
        }

        private void OSA_Init()
        {
            try
            {
                if (!vm.isOSAConnected)
                {
                    vm.Str_Status = "OSA Initializing";
                    vm.OSA.protocol = 0;
                    vm.OSA.BoardNumber = vm.tls_BoardNumber;
                    vm.OSA.Addr = vm.tls_Addr;
                    vm.OSA.Open();
                    vm.OSA.init();

                    vm.isOSAConnected = true;
                }
            }
            catch (Exception ex)
            {
                vm.Str_cmd_read = "OSA Setting Error";
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        public async Task<List<DataPoint>> OSA_Scan()
        {
            List<DataPoint> result = new List<DataPoint>();

            //OSA Setting
            //Task t = Task.Run(() => OSA_Init());
            //if(!t.Wait(3000))
            //{
            //    vm.Str_cmd_read = "Connect to OSA Failed";
            //    return result;
            //}
            try
            {
                if (!vm.isOSAConnected)
                {
                    vm.Str_Status = "OSA Initializing";
                    vm.OSA.protocol = 0;
                    vm.OSA.BoardNumber = vm.OSA_BoardNumber;
                    vm.OSA.Addr = vm.OSA_Addr;
                    vm.OSA.Open();
                    vm.OSA.init();

                    vm.isOSAConnected = true;
                }
            }
            catch (Exception ex)
            {
                vm.Str_cmd_read = "OSA Setting Error";
                MessageBox.Show(ex.StackTrace.ToString());
            }

            await Task.Delay(200);

            //OSA Scanning
            try
            {
                vm.Str_Status = "OSA Scanning";

                int points = Convert.ToInt32((vm.float_WL_Scan_End - vm.float_WL_Scan_Start) / vm.float_WL_Scan_Gap + 1);

                //vm.OSA.SetResolution(vm.OSA_RBW);

                //Cal l OSA scan function
                string rawdata = vm.OSA.StartTrace(vm.float_WL_Scan_Start, vm.float_WL_Scan_End, vm.float_WL_Scan_Gap, vm.OSA_Sensitivity, vm.OSA_RBW);

                #region Query data
                //string rawdata = vm.OSA.Read(17 * points); // set length of geting attenuation, 1 raw = 17 byte
                //await Task.Delay(500);

                int counter = 0;
                while (rawdata == "-100" && counter < 10)
                {
                    vm.OSA.SendCommand("TRAC:DATA:Y? TRA");
                    rawdata = vm.OSA.Read(17 * points);
                    counter++;
                }
                if (rawdata == "-100")
                    Console.WriteLine("-999,Time Limit Exceeded to complete operation");
                double osastep = vm.float_WL_Scan_Gap;
                string str_wl_il = vm.OSA.GetAllPower(rawdata, osastep, vm.float_WL_Scan_Start, vm.float_WL_Scan_End);
                #endregion

                #region Analyze data from osa
                string[] list_s = str_wl_il.Split(',');

                if (list_s.Length > 1)
                {
                    for (int i = 1; i < list_s.Length; i++)
                    {
                        if (i % 2 == 1)
                        {
                            if (double.TryParse(list_s[i - 1], out double wl) && double.TryParse(list_s[i], out double IL))
                                result.Add(new DataPoint(wl, IL));
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                vm.Str_cmd_read = "OSA Scan Error";
                MessageBox.Show(ex.StackTrace.ToString());
            }

            vm.Str_Status = "OSA Scan End";

            return result;
        }

        public void Save_Calibration_Data(string Data_Type)
        {
            #region Delete and Save Bear say to txt file
            if (vm.List_bear_say.Count == 0) return;

            switch (Data_Type)
            {
                case "K WL":
                    vm.List_bear_say_DataLabel = new List<string>() { "K WL", "WL", "IL" };
                    break;

                case "K V3":
                    vm.List_bear_say_DataLabel = new List<string>() { "K V3", "DAC", "V3" };
                    break;
            }

            if (File.Exists(@"D:\PD\BearSay.txt")) File.Delete(@"D:\PD\BearSay.txt");

            using (System.IO.StreamWriter file =
                 new System.IO.StreamWriter(@"D:\PD\BearSay.txt", true))
            {
                string str = "";
                for (int i = 0; i < vm.List_bear_say.Count; i++)
                {
                    for (int j = 0; j < vm.List_bear_say[i].Count; j++)
                    {
                        str = str + vm.List_bear_say[i][j];
                        if (j == 0) str += ",";
                    }
                    str = str + "\r\n";
                }
                file.WriteLine(str);

                vm._write_line = new List<string>();
            }
            #endregion
        }

        public int Save_K_WL_Data(string Data_Type, string userID, GaugeModel gm, string production_type, int beforeORafter)
        {
            string SNnumber = gm.GaugeSN;
            int ch = int.Parse(gm.GaugeChannel) - 1;

            #region Save Bear say to txt file
            if (string.IsNullOrEmpty(gm.GaugeBearSay_1)) return 1;
            //if (vm.List_bear_say.Count == 0) return 1;  //ErrorCode:1 => BearSay is empty
            if (string.IsNullOrWhiteSpace(SNnumber)) return 3; //ErrorCode:3 => SN number is empty

            switch (Data_Type)
            {
                case "K WL":
                    vm.List_bear_say_DataLabel = new List<string>() { "K WL", "WL", "IL" };
                    break;
            }
            DateTime dt = DateTime.Now;
            string a = dt.ToString("yyyy/MM/dd HH:mm:ss");

            //string filePath = string.Concat(vm.txt_save_wl_data_path, SNnumber, ".txt");

            string filePath = Path.Combine(vm.txt_save_wl_data_path, $"{SNnumber}.txt");

            if (File.Exists(filePath))
            {
                List<string> quotelist = File.ReadAllLines(filePath).ToList();

                if (quotelist.Count > 2)  //已有兩筆數據
                {
                    quotelist[beforeORafter] = "";  //Delete Data, 1 is before, 2 is after             
                                                    //quotelist.RemoveAt(beforeORafter);    //Delete Data, 1 is before, 2 is after      

                    vm.Save_Log("Save Data", "Exist data count : " + quotelist.Count.ToString(), false);
                }
                else if (quotelist.Count == 2) //只有一筆數據
                {
                    quotelist.Add("");

                    vm.Save_Log("Save Data", "Exist data count : " + quotelist.Count.ToString(), false);
                }

                File.Delete(filePath);

                string str = "";
                try
                {
                    str += gm.GaugeBearSay_1;  //第二列-波長
                    str += ",";
                    str += a;  //第二列-時間

                    quotelist[beforeORafter] = str;
                }
                catch { vm.Str_cmd_read = "Save data error"; return 4; }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
                {
                    for (int i = 0; i < quotelist.Count; i++)
                    {
                        file.WriteLine(quotelist[i]);
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(userID)) return 2;  //ErrorCode:2 => UserID is empty

                using (System.IO.StreamWriter file =
                 new System.IO.StreamWriter(filePath, true))
                {
                    string firstline = $"{production_type},{userID},{SNnumber}";
                    file.WriteLine(firstline);  //第一列：使用者ID

                    string str = "";

                    if (beforeORafter == 2)
                    {
                        file.WriteLine(str);
                    }

                    str += gm.GaugeBearSay_1;  //第二列-波長
                    str += ",";
                    str += a;  //第二列-時間
                    file.WriteLine(str);

                    //vm._write_line = new List<string>();
                }
            }

            return 0;
            #endregion
        }

        public void Save_BR_Data()
        {
            for (int i = 0; i < vm.list_BR_DAC_WL.Count; i++)
            {
                if (vm.Plot_Series[i].Points.Count == 0) continue;

                string WL_Dac_Now = vm.list_BR_DAC_WL[i];
                string INOUT = vm.BR_INOut ? "OUT" : "IN";

                string filePath = Path.Combine(vm.txt_BR_Save_Path, $"{vm.list_GaugeModels.First().GaugeSN}-{WL_Dac_Now}-{INOUT}.txt");

                if (!string.IsNullOrEmpty(filePath))
                {
                    if (File.Exists(filePath))
                        File.Create(filePath).Close();

                    using (StreamWriter sr = new StreamWriter(filePath))
                    {
                        foreach (DataPoint dp in vm.Plot_Series[i].Points)
                        {
                            if (vm.dB_or_dBm)
                                sr.WriteLine($"{dp.X},{dp.Y + vm.BR_Diff}");  //dB mode
                            else
                            {
                                double ref_value = 0;

                                RefModel rm = vm.Ref_memberDatas.Where(r => r.Wavelength == dp.X).FirstOrDefault();

                                if (rm != null) ref_value = rm.Ch_1;

                                sr.WriteLine($"{dp.X},{Math.Round(dp.Y + vm.BR_Diff - ref_value, 3)}");  //dBm mode, chart points IL wasn't minus ref value, have to convert dBm to dB to save data.
                            }
                        }

                        sr.Close();
                    }
                }
            }

            vm.Str_cmd_read = "Saved";
        }

        public int Save_ChartNow_Data(string path, Dictionary<string, string> dic_keyPairs)
        {
            using (System.IO.StreamWriter file =
                 new System.IO.StreamWriter(path, true))
            {
                foreach (string wl in dic_keyPairs.Keys)
                {
                    string line = $"{wl},{dic_keyPairs[wl]}";
                    file.WriteLine(line);
                }
            }

            return 0;
        }

        public void Save_Log_Message(string Data_Type, string content, string time)
        {
            #region Save log to txt file         

            //if (File.Exists(@"D:\PD\Log.txt")) File.Delete(@"D:\PD\Log.txt");

            using (System.IO.StreamWriter file =
                 new System.IO.StreamWriter(@"D:\PD\Log.txt", true))
            {
                string str = "";
                str = content + "  " + time + "\r\n";
                file.WriteLine(str);

                //vm._write_line = new List<string>();
            }
            #endregion
        }

        public void Clean_Chart()
        {
            vm.Save_PD_Value = new List<DataPoint>();
            vm.Save_All_PD_Value = Analysis.ListDefine<DataPoint>(vm.Save_All_PD_Value, vm.ch_count, new List<DataPoint>());

            if (vm.station_type == ComViewModel.StationTypes.BR)
            {
                ObservableCollection<BR_Model> cmTemp = new ObservableCollection<BR_Model>(vm.ChartNowModel.list_BR_Model);
                vm.ChartNowModel = new ChartModel(vm.ch_count);
                vm.ChartNowModel.list_BR_Model = new ObservableCollection<BR_Model>(cmTemp);
            }
            else
                vm.ChartNowModel = new ChartModel(vm.ch_count);

            vm.ChartNowModel.list_delta_IL.AddRange(Enumerable.Repeat(0.0, vm.ch_count));

            vm.LineAnnotation_X_1.StrokeThickness = 0;
            vm.LineAnnotation_X_2.StrokeThickness = 0;
            vm.LineAnnotation_Y.StrokeThickness = 0;

            vm.PointAnnotation_1.StrokeThickness = 0;
            vm.PointAnnotation_2.StrokeThickness = 0;

            for (int i = 0; i < vm.Plot_Series.Count; i++)
            {
                vm.Plot_Series[i].Points.Clear();
            }
        }

        public void Reset_Chart(int LineSeriesCount)
        {
            for (int i = 0; i < LineSeriesCount; i++)
            {
                vm.list_Chart_UI_Models.Add(new Chart_UI_Model()
                {
                    Button_Color = i < vm.list_brushes.Count() ? vm.list_brushes[i] : vm.list_brushes[i - vm.list_brushes.Count()],
                    Button_Channel = i + 1,
                    Button_Content = $"Ch {i + 1}",
                    Button_IsChecked = true,
                    Button_IsVisible = Visibility.Visible,
                    Button_Tag = "../../Resources/right-arrow.png"
                });

                OxyPlot.Series.LineSeries lineSerie = new OxyPlot.Series.LineSeries
                {
                    Title = $"Ch {i + 1}",
                    FontSize = 20,
                    StrokeThickness = 1.8,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 0,  //4
                    Smooth = false,
                    MarkerFill = vm.list_OxyColor[i],
                    Color = vm.list_OxyColor[i],
                    CanTrackerInterpolatePoints = true,
                    TrackerFormatString = vm.Chart_x_title + " : {2}\n" + vm.Chart_y_title + " : {4}",
                    LineLegendPosition = OxyPlot.Series.LineLegendPosition.None,
                    IsVisible = vm.list_Chart_UI_Models[i].Button_IsChecked
                };

                vm.Plot_Series.Add(lineSerie);
                vm.PlotViewModel.Series.Add(vm.Plot_Series[i]);
            }
        }

        public void Reset_Chart(List<string> lineTitles)
        {
            vm.Plot_Series.Clear();
            vm.PlotViewModel_BR.Series.Clear();

            vm.list_Chart_UI_Models.Clear();

            //若lineTitles的參數量多於16(color list的預設數)，則需擴充color list
            int expand_times = 5;
            for (int i = 0; i < expand_times; i++)
            {
                if (lineTitles.Count > vm.list_brushes.Count)
                {
                    vm.list_brushes.AddRange(vm.list_brushes);
                    vm.list_OxyColor.AddRange(vm.list_OxyColor);
                }
                else
                    break;
            }

            if (lineTitles.Count > vm.list_brushes.Count)
            {
                vm.Str_cmd_read = $"List LineTitle count > {expand_times * 16}";
                //vm.Save_Log(new LogMember() { Status = "Station Initializing", Message = vm.Str_cmd_read });
                return;
            }

            for (int i = 0; i < lineTitles.Count; i++)
            {
                vm.list_Chart_UI_Models.Add(new Chart_UI_Model()
                {
                    Button_Color = i < vm.list_brushes.Count() ? vm.list_brushes[i] : vm.list_brushes[i - vm.list_brushes.Count()],
                    Button_Channel = i + 1,
                    Button_Content = lineTitles[i],
                    Button_IsChecked = true,
                    Button_IsVisible = Visibility.Visible,
                    Button_Tag = "../../Resources/right-arrow.png"
                });

                OxyPlot.Series.LineSeries lineSerie = new OxyPlot.Series.LineSeries
                {
                    Title = lineTitles[i],
                    FontSize = 20,
                    StrokeThickness = 1.8,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 0,  //4
                    Smooth = false,
                    MarkerFill = vm.list_OxyColor[i],
                    Color = vm.list_OxyColor[i],
                    CanTrackerInterpolatePoints = true,
                    TrackerFormatString = vm.Chart_x_title + " : {2}\n" + vm.Chart_y_title + " : {4}",
                    LineLegendPosition = OxyPlot.Series.LineLegendPosition.None,
                    IsVisible = vm.list_Chart_UI_Models[i].Button_IsChecked
                };

                vm.Plot_Series.Add(lineSerie);
                vm.PlotViewModel.Series.Add(vm.Plot_Series[i]);
            }

            vm.Update_ALL_PlotView();
        }

        /// <summary>
        /// Save ChartNowModel into list_charts
        /// </summary>
        /// <returns></returns>
        public async Task Save_Chart()
        {
            ChartModel nowChartModel = new ChartModel(vm.ChartNowModel);
            nowChartModel.Chart_No = vm.int_chart_now;
            nowChartModel.title_x = vm.Chart_x_title;
            nowChartModel.title_y = vm.Chart_y_title;

            foreach (GaugeModel gm in vm.list_GaugeModels)
            {
                nowChartModel.SN_List.Add(gm.GaugeSN);

                List<List<string>> bearSayList = new List<List<string>>();
                bearSayList.Add(new List<string>() { gm.GaugeBearSay_1, gm.GaugeBearSay_2, gm.GaugeBearSay_3 });
                nowChartModel.BearSay_List = bearSayList;
            }

            nowChartModel.Plot_Series = new ObservableCollection<OxyPlot.Series.LineSeries>();

            foreach (OxyPlot.Series.LineSeries ls in vm.Plot_Series)
            {
                OxyPlot.Series.LineSeries serie = new OxyPlot.Series.LineSeries();
                serie.Title = ls.Title;

                serie.Points.AddRange(ls.Points);
                nowChartModel.Plot_Series.Add(serie);
            }

            nowChartModel.list_BR_Model = new ObservableCollection<BR_Model>(vm.ChartNowModel.list_BR_Model);

            nowChartModel.list_Annotation = new List<OxyPlot.Annotations.Annotation>(vm.ChartNowModel.list_Annotation);

            vm.list_ChartModels.Add(nowChartModel);

            vm.Chart_All_Datapoints_History.Add(vm.Chart_All_DataPoints);
            vm.int_chart_count = vm.Chart_All_Datapoints_History.Count;
            vm.int_chart_now = vm.int_chart_count;
            await Task.Delay(3);
        }

        public void Update_Chart(double X, double Y, int ch)
        {
            #region Set Chart data points   

            if (vm.station_type == ComViewModel.StationTypes.Testing
                || vm.station_type == ComViewModel.StationTypes.UTF600
                || vm.station_type == ComViewModel.StationTypes.Hermetic_Test
                || vm.IsDistributedSystem)
            {
                if (vm.isTimerOn)
                {
                    if (X > vm.int_timer_timespan)
                    {
                        vm.IsGoOn = false;
                        vm.isTimerOn = false;
                    }

                    // 更新Timer顯示的"剩餘時間"
                    TimeSpan time = new TimeSpan(0, 0, vm.int_timer_timespan - (int)X);
                    vm.int_timer_hrs = time.Hours;
                    vm.int_timer_min = time.Minutes;
                    vm.int_timer_sec = time.Seconds;
                }

                vm.Save_All_PD_Value[ch].Add(vm.ChartNowModel.list_dataPoints[ch].Last());

                //vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);

                //vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries


                vm.Plot_Series[ch].Points.Add(new DataPoint(X, Y));
                vm.Update_ALL_PlotView();
            }

            //PD mode
            else if (!vm.PD_or_PM)
            {
                //vm.Save_All_PD_Value[ch].Add(new DataPoint(Math.Round(X, 2), Y));

                //vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);

                //vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries


                vm.Plot_Series[ch].Points.Add(new DataPoint(X, Y));
                vm.Update_ALL_PlotView();
            }

            else
            {
                if (vm.isTimerOn)
                {
                    if (X > vm.int_timer_timespan)
                    {
                        vm.IsGoOn = false;
                        vm.isTimerOn = false;
                    }

                    // 更新Timer顯示的"剩餘時間"
                    TimeSpan time = new TimeSpan(0, 0, vm.int_timer_timespan - (int)X);
                    vm.int_timer_hrs = time.Hours;
                    vm.int_timer_min = time.Minutes;
                    vm.int_timer_sec = time.Seconds;
                }

                vm.Save_All_PD_Value[ch].Add(vm.ChartNowModel.list_dataPoints[ch].Last());

                //vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);

                //vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries

                vm.Plot_Series[ch].Points.Add(new DataPoint(X, Y));
                vm.Update_ALL_PlotView();
            }
            #endregion
        }

        /// <summary>
        /// Add a new datapoint into selected plotview series
        /// </summary>
        /// <param name="X">Datapoint.X</param>
        /// <param name="Y">Datapoint.Y</param>
        /// <param name="ch">Selected Channel (Series)</param>
        public void Update_Chart_Data(double X, double Y, int ch)
        {
            #region Set Chart data points   

            if (vm.station_type == ComViewModel.StationTypes.Testing
                || vm.station_type == ComViewModel.StationTypes.UTF600
                || vm.station_type == ComViewModel.StationTypes.Hermetic_Test
                || vm.IsDistributedSystem)
            {
                if (vm.isTimerOn)
                {
                    if (X > vm.int_timer_timespan)
                    {
                        vm.IsGoOn = false;
                        vm.isTimerOn = false;
                    }

                    // 更新Timer顯示的"剩餘時間"
                    TimeSpan time = new TimeSpan(0, 0, vm.int_timer_timespan - (int)X);
                    vm.int_timer_hrs = time.Hours;
                    vm.int_timer_min = time.Minutes;
                    vm.int_timer_sec = time.Seconds;
                }

                //vm.Save_All_PD_Value[ch].Add(vm.ChartNowModel.list_dataPoints[ch].Last());

                //vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);

                //vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries


                vm.Plot_Series[ch].Points.Add(new DataPoint(X, Y));
            }

            //PD mode
            else if (!vm.PD_or_PM)
            {
                //vm.Save_All_PD_Value[ch].Add(new DataPoint(Math.Round(X, 2), Y));

                //vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);

                //vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries


                vm.Plot_Series[ch].Points.Add(new DataPoint(X, Y));
            }

            else
            {
                if (vm.isTimerOn)
                {
                    if (X > vm.int_timer_timespan)
                    {
                        vm.IsGoOn = false;
                        vm.isTimerOn = false;
                    }

                    // 更新Timer顯示的"剩餘時間"
                    TimeSpan time = new TimeSpan(0, 0, vm.int_timer_timespan - (int)X);
                    vm.int_timer_hrs = time.Hours;
                    vm.int_timer_min = time.Minutes;
                    vm.int_timer_sec = time.Seconds;
                }

                //vm.Save_All_PD_Value[ch].Add(vm.ChartNowModel.list_dataPoints[ch].Last());

                //vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);

                //vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries

                vm.Plot_Series[ch].Points.Add(new DataPoint(X, Y));
            }
            #endregion
        }


        public void Update_DeltaIL(int dataCount)
        {
            #region Cal. Delta IL   

            double power = 0;

            if (dataCount == 0)
            {
                vm.list_ch_title.Clear();
                for (int i = 0; i < vm.ch_count; i++)
                {
                    vm.list_ch_title.Add($"Ch{i + 1}");
                    if (vm.list_GaugeModels[i].boolGauge || vm.BoolAllGauge)
                    {
                        vm.maxIL[i] = 0;
                        vm.minIL[i] = 0;
                    }
                }
            }
            else if (dataCount == 1)
            {
                for (int i = 0; i < vm.ch_count; i++)
                {
                    if (vm.list_GaugeModels[i].boolGauge || vm.BoolAllGauge)
                    {
                        if (vm.ChartNowModel.list_dataPoints[i].Count <= 0) continue;

                        power = vm.ChartNowModel.list_dataPoints[i].Last().Y;
                        vm.maxIL[i] = power;
                        vm.minIL[i] = power;
                    }
                }
            }
            else
            {
                for (int i = 0; i < vm.ch_count; i++)
                {
                    if (vm.list_GaugeModels[i].boolGauge || vm.BoolAllGauge)
                    {
                        if (vm.ChartNowModel.list_dataPoints[i].Count <= 0) continue;

                        power = vm.ChartNowModel.list_dataPoints[i].Last().Y;
                        vm.maxIL[i] = power > vm.maxIL[i] ? power : vm.maxIL[i];
                        vm.minIL[i] = power < vm.minIL[i] ? power : vm.minIL[i];

                        double deltaIL = Math.Round(Math.Abs(vm.maxIL[i] - vm.minIL[i]), 4);

                        if (vm.list_ch_title.Count > i)
                        {
                            vm.list_ch_title[i] = string.Concat("ch1", " ,Delta IL : ", deltaIL.ToString());

                            if (vm.ChartNowModel.list_delta_IL.Count == 0)
                                vm.ChartNowModel.list_delta_IL.AddRange(Enumerable.Repeat(0.0, vm.ch_count));

                            vm.ChartNowModel.list_delta_IL[i] = deltaIL;
                        }

                        if (vm.Plot_Series.Count > i)
                            vm.Plot_Series[i].Title = $"Ch{i + 1}, Delta IL : {deltaIL}";
                    }
                }
            }

            vm.Update_ALL_PlotView();
            #endregion
        }

        public void Update_DeltaIL()
        {
            double power = 0;

            for (int i = 0; i < vm.ch_count; i++)
            {
                if (vm.list_GaugeModels[i].boolGauge || vm.BoolAllGauge)
                {
                    power = vm.Double_Powers[i];

                    vm.maxIL[i] = power > vm.maxIL[i] ? power : vm.maxIL[i];
                    vm.minIL[i] = power < vm.minIL[i] ? power : vm.minIL[i];

                    double deltaIL = Math.Round(Math.Abs(vm.maxIL[i] - vm.minIL[i]), 4);

                    if (vm.list_ch_title.Count <= i) break;

                    if (vm.list_Chart_UI_Models[i].Button_IsChecked)
                        vm.list_ch_title[i] = $"ch{i + 1}, Delta IL : {deltaIL}";

                    if (vm.ChartNowModel.list_delta_IL.Count == 0)
                        vm.ChartNowModel.list_delta_IL.AddRange(Enumerable.Repeat(0.0, vm.ch_count));

                    vm.ChartNowModel.list_delta_IL[i] = deltaIL;

                    if (vm.Plot_Series.Count > i)
                        vm.Plot_Series[i].Title = $"ch{i + 1}, Delta IL : {deltaIL}";
                }
            }

            vm.Update_ALL_PlotView();
        }
    }
}
