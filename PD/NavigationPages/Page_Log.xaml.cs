using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;

using PD.ViewModel;
using PD.Models;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Page_Log.xaml
    /// </summary>
    public partial class Page_Log : UserControl
    {
        ComViewModel vm;

        //ObservableCollection<Member> memberData = new ObservableCollection<Member>();

        public Page_Log(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;

            //dataGrid.DataContext = memberData;
            dataGrid.DataContext = vm.LogMembers;

            //AddMsgItem(vm.LogMembers, "K WL", "IL too high", DateTime.Now.Date.ToShortDateString(), DateTime.Now.ToShortTimeString());

            //AddMsgItem("K WL", "IL too high", DateTime.Now.Date.ToShortDateString(), DateTime.Now.ToShortTimeString());
        }

        //public class Member
        //{
        //    public string Status { get; set; }
        //    public string Message { get; set; }
        //    public string Date { get; set; }
        //    public string Time { get; set; }
        //}

        //private void AddMsgItem(string status, string msg, string date, string time)
        //{
        //    memberData.Add(new Member()
        //    {
        //        Status = status,
        //        Message = msg,
        //        Date = date,
        //        Time = time,
        //    });
        //}

        private void AddMsgItem(ObservableCollection<LogMember> members, string status, string msg, string date, string time, string rst)
        {
            members.Add(new LogMember()
            {
                Status = status,
                Message = msg,
                Result = rst,
                Date = date,
                Time = time,
            });
        }
    }
}
