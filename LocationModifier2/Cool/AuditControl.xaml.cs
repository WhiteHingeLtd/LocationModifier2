using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for AuditControl.xaml
    /// </summary>
    public partial class AuditControl : UserControl
    {
        public AuditControl(Dictionary<string,object> data)
        {
            InitializeComponent();
            AdjustText.Text = auditstrings[(data["AuditEvent"] as int?).Value] + " " + data["stock"].ToString();
            DetailText.Text = (data["DateRecorded"] as DateTime?).Value.ToString("dd/MM/yyyy HH:mm:ss") + " - User " +
                              data["AuditUserID"].ToString() + " on " + data["packsize"].ToString() + " pack at " +
                              data["loctext"].ToString();
        }


        private static string[] auditstrings = {"---","NEW", "ADJ","DEL","A-ADJ", "A-DEL", "A-NEW"};

    }

    
}
