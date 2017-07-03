using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for AuditControl.xaml
    /// </summary>
    public partial class AuditControl
    {
        [SuppressMessage("ReSharper", "RedundantToStringCall")]
        public AuditControl(Dictionary<string,object> data)
        {
            InitializeComponent();
            AdjustText.Text = auditstrings[((int?) data["AuditEvent"]).Value] + " " + data["stock"].ToString();

            DetailText.Text = ((DateTime?) data["DateRecorded"]).Value.ToString("dd/MM/yyyy HH:mm:ss") + " - User " +
                              data["AuditUserID"].ToString() + " on " + data["packsize"].ToString() + " pack at " +
                              data["loctext"].ToString();
        }


        private static string[] auditstrings = {"---","NEW", "ADJ","DEL","A-ADJ", "A-DEL", "A-NEW","","","","","ADJ +","ADJ -", "ADJ", "MV FR", "MV TO"};

    }

    
}
